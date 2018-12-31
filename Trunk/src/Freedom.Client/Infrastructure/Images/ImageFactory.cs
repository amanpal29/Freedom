using Freedom.Client.Resources;
using log4net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Freedom.Client.Infrastructure.Images
{
    public static class ImageFactory
    {
        #region Fields

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly int[] Sizes = { 16, 24, 32, 48, 64, 128, 256 };

        private static readonly string AssemblyName = Assembly.GetExecutingAssembly().GetName().Name;

        private static readonly Uri FontIconGlyphTypefaceUri = new Uri("pack://application:,,,/Resources/Fonts/Freedom-Regular.ttf");

        private static readonly Lazy<GlyphTypeface> GlyphTypefaceSingleton =
            new Lazy<GlyphTypeface>(() => new GlyphTypeface(FontIconGlyphTypefaceUri));

        private static readonly double[] FontIconAdvanceWidths = { 0d };

        private static readonly SolidColorBrush DefaultFontIconBrush = new SolidColorBrush(Color.FromRgb(0x69, 0x69, 0x69));

        private static readonly int IconMissingCodePoint = FontIcons.IconMissingIcon[0];

        private static readonly Brush IconMissingBrush = new SolidColorBrush(
             (Color)(ColorConverter.ConvertFromString(FontIcons.IconMissingColorCode) ?? Colors.Red));

        private static List<string> _images;

        private static int _dpi;

        #endregion

        #region Private Properties and Methods

        private static int Dpi
        {
            get
            {
                if (_dpi != 0)
                    return _dpi;

                try
                {
                    const BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Static;

                    PropertyInfo dpiProperty = typeof(SystemParameters).GetProperty("Dpi", bindingFlags);

                    _dpi = Math.Max((int)dpiProperty.GetValue(null, null), 96);
                }
                catch (Exception ex)
                {
                    Log.Warn("An exception occurred trying to read the current screen dpi", ex);

                    _dpi = 96;
                }

                return _dpi;
            }
        }

        private static RenderTargetBitmap RenderFrameworkElement(Size size, FrameworkElement element)
        {
            int pixelWidth = (int)Math.Ceiling(size.Width * Dpi / 96d);

            int pixelHeight = (int)Math.Ceiling(size.Height * Dpi / 96d);

            RenderTargetBitmap result = new RenderTargetBitmap(pixelWidth, pixelHeight, Dpi, Dpi, PixelFormats.Pbgra32);

            double scaleX = size.Width / element.Width;

            double scaleY = size.Height / element.Height;

            if (Math.Abs(scaleX - 1.0d) > 0.01 || Math.Abs(scaleY - 1.0d) > 0.01)
                element.LayoutTransform = new ScaleTransform(scaleX, scaleY);
            else
                element.LayoutTransform = null;

            element.Measure(size);

            element.Arrange(new Rect(size));

            result.Render(element);

            return result;
        }

        private static IEnumerable<string> Images
        {
            get
            {
                if (_images != null)
                    return _images;

                Assembly assembly = Assembly.GetExecutingAssembly();

                CultureInfo culture = Thread.CurrentThread.CurrentCulture;

                string resourceName = assembly.GetName().Name + ".g";

                ResourceManager resourceManager = new ResourceManager(resourceName, assembly);

                try
                {
                    ResourceSet resourceSet = resourceManager.GetResourceSet(culture, true, true);

                    _images = resourceSet.Cast<DictionaryEntry>()
                        .Select(e => $"/{assembly.GetName().Name};component/{e.Key.ToString()}".ToLowerInvariant())
                        .ToList();
                }
                finally
                {
                    resourceManager.ReleaseAllResources();
                }

                return _images;
            }
        }

        private static bool IsHexDigit(char c)
        {
            return ('0' <= c && c <= '9') || ('A' <= c && c <= 'F') || ('a' <= c && c <= 'f');
        }

        private static GlyphRun BuildFontIconGlyphRun(ushort glyphIndex, double renderingEmSize)
        {
            return new GlyphRun(
                glyphTypeface: GlyphTypefaceSingleton.Value,
                bidiLevel: 0,
                isSideways: false,
                renderingEmSize: renderingEmSize,
                glyphIndices: new[] { glyphIndex },
                baselineOrigin: new Point(0, 12.29),
                advanceWidths: FontIconAdvanceWidths,
                glyphOffsets: null,
                characters: null,
                deviceFontName: null,
                clusterMap: null,
                caretStops: null,
                language: null);
        }

        #endregion

        #region Deprecated Obsolete Methods

        public static ImageSource Get(string uri)
        {
            return Images.Contains(uri.ToLowerInvariant())
                ? new BitmapImage(new Uri("pack://application:,,," + uri, UriKind.Absolute))
                : null;
        }       

        #endregion

        #region Public Static Factory Methods

        public static ImageSource Get(string fontIcon, double desiredSize)
        {
            if (string.IsNullOrEmpty(fontIcon))
                return null;

            IDictionary<int, ushort> glyphMap = GlyphTypefaceSingleton.Value.CharacterToGlyphMap;

            Brush foreground = Application.Current.TryFindResource("SliderSelectionBorderNormalBrushKey") as Brush ??
                               DefaultFontIconBrush;

            // Short-cut for monochromatic images...
            if (fontIcon.Length == 1)
            {
                ushort glyphIndex;

                if (!glyphMap.TryGetValue(fontIcon[0], out glyphIndex))
                {
                    Debug.Print($"Unable to find GlyphIndex for CodePoint \\u{(int)fontIcon[0]:x}");

                    return new DrawingImage(new GlyphRunDrawing(IconMissingBrush,
                        BuildFontIconGlyphRun(glyphMap[IconMissingCodePoint], desiredSize)));
                }

                return new DrawingImage(new GlyphRunDrawing(foreground, BuildFontIconGlyphRun(glyphIndex, desiredSize)));
            }

            DrawingGroup drawingGroup = new DrawingGroup();

            for (int i = 0; i < fontIcon.Length; i++)
            {
                if (fontIcon[i] == '#')
                {
                    int startIndex = i;
                    int length = 1;

                    while (i < fontIcon.Length - 1 && length < 9 && IsHexDigit(fontIcon[i + 1]))
                    {
                        length++;
                        i++;
                    }

                    string colorCode = fontIcon.Substring(startIndex, length);

                    try
                    {
                        object color = ColorConverter.ConvertFromString(colorCode);

                        if (color is Color)
                        {
                            foreground = new SolidColorBrush((Color)color);
                        }
                    }
                    catch (FormatException ex)
                    {
                        Debug.Print($"Unable to convert colorCode '{colorCode}' to a color. {ex.Message}");
                    }
                }
                else
                {
                    ushort glyphIndex;

                    if (glyphMap.TryGetValue(fontIcon[i], out glyphIndex))
                    {
                        GlyphRunDrawing glyphRunDrawing = new GlyphRunDrawing(
                            foreground, BuildFontIconGlyphRun(glyphIndex, desiredSize));

                        drawingGroup.Children.Add(glyphRunDrawing);
                    }
#if DEBUG
                    else
                    {
                        Debug.Print($"Unable to find GlyphIndex for CodePoint \\u{(int)fontIcon[i]:x}");

                        GlyphRunDrawing glyphRunDrawing = new GlyphRunDrawing(
                            IconMissingBrush,
                            BuildFontIconGlyphRun(glyphMap[IconMissingCodePoint], desiredSize));

                        drawingGroup.Children.Add(glyphRunDrawing);
                    }
#endif
                }
            }

            if (drawingGroup.Children.Count == 0)
            {
                return new DrawingImage(new GlyphRunDrawing(IconMissingBrush,
                    BuildFontIconGlyphRun(glyphMap[IconMissingCodePoint], desiredSize)));
            }

            return new DrawingImage(drawingGroup);
        }

        public static ImageSource Get(string category, string imageName, int idealSize)
        {
            if (string.IsNullOrEmpty(imageName))
                return null;
            
            string uri = null;

            int size = (int)Math.Ceiling(idealSize * Dpi / 96d);

            // Start by looking for an image of the ideal size, if not found, check the next size up until we find one...

            while (size > 0)
            {
                uri = $"/{AssemblyName};component/Resources/{category}/{imageName}_{size}.png";

                uri = uri.Replace("//", "/").ToLowerInvariant();

                if (Images.Contains(uri)) break;

                int currentSize = size;

                size = Sizes.FirstOrDefault(sz => sz > currentSize);
            }

            if (size == 0 && idealSize > 0)
            {
                // Couldn't find an image larger then requested... so look for a smaller one instead.

                size = (int)Math.Ceiling(idealSize * Dpi / 96d);

                do
                {
                    int currentSize = size;

                    size = Sizes.LastOrDefault(sz => sz < currentSize);

                    if (size == 0) break;

                    uri = $"/{AssemblyName};component/Resources/{category}/{imageName}_{size}.png";

                    uri = uri.Replace("//", "/").ToLowerInvariant();
                }
                while (!Images.Contains(uri));
            }

            // Couldn't find it at any size, try without a size extension

            if (size != 0)
                return new BitmapImage(new Uri("pack://application:,,," + uri, UriKind.Absolute));

            uri = $"/{AssemblyName};component/Resources/{category}/{imageName}.png";

            uri = uri.Replace("//", "/").ToLowerInvariant();

            if (Images.Contains(uri))
                return new BitmapImage(new Uri("pack://application:,,," + uri, UriKind.Absolute));

            Log.Warn("ImageFactory could not find an image. " +
                     $"Category: {category}, Name: {imageName}, IdealSize: {idealSize}x{idealSize}");

            return null;
        }

        #endregion
    }
}
