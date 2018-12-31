using Freedom.Client.Properties;
using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Freedom.Client.Infrastructure
{
    public static class WindowPositionExtensions
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static void SaveWindowPosition(this Window window, string settingsKey)
        {
            if (string.IsNullOrEmpty(settingsKey))
                throw new ArgumentNullException(nameof(settingsKey));

            try
            {
                WindowPositionSettings settings = new WindowPositionSettings(settingsKey);

                settings.HasSettings = true;
                settings.WindowState = window.WindowState;

                if (window.WindowState == WindowState.Normal)
                {
                    settings.Left = window.Left;
                    settings.Top = window.Top;
                    settings.Width = window.Width;
                    settings.Height = window.Height;
                }
                else
                {
                    settings.Rect = window.RestoreBounds;
                }

                settings.Save();
            }
            catch (Exception exception)
            {
                Log.Warn($"Unable to save window position for window '{settingsKey}'.", exception);
            }
        }

        private static bool IsReasonableSize(Size size)
        {
            return !double.IsNaN(size.Width) && !double.IsInfinity(size.Width) &&
                   !double.IsNaN(size.Height) && !double.IsInfinity(size.Height) &&
                   160 < size.Width && size.Width < 15360 &&
                   120 < size.Height && size.Height < 8640;
        }

        private static Rect GetVisiblePortion(Rect windowPosition)
        {
            Rect virtualScreen = new Rect(
                SystemParameters.VirtualScreenLeft,
                SystemParameters.VirtualScreenTop,
                SystemParameters.VirtualScreenWidth,
                SystemParameters.VirtualScreenHeight);

            return Rect.Intersect(windowPosition, virtualScreen);
        }

        public static void RestoreWindowPosition(this Window window, string settingsKey)
        {
            try
            {
                WindowPositionSettings settings = new WindowPositionSettings(settingsKey);

                if (!settings.HasSettings)
                {
                    try
                    {
                        // If there isn't a window position found, check for setting in a previous version...
                        settings.Upgrade();
                    }
                    catch (ConfigurationErrorsException exception)
                    {
                        Log.Warn("An error occurred while trying to upgrade settings from a previous version.",
                            exception);
                    }
                }

                if (!settings.HasSettings) return;

                if (!IsReasonableSize(settings.Size))
                {
                    Log.Info($"Not restoring window position '{settingsKey}' because the saved setting is too large or small.");
                }
                else if (!IsReasonableSize(GetVisiblePortion(settings.Rect).Size))
                {
                    Log.Info(
                        $"Not restoring window '{settingsKey}' to its previous position because the size, quantity, " +
                        "and/or orientation of the monitors connected to this computer has changed.");
                }
                else
                {
                    window.Left = settings.Left;
                    window.Top = settings.Top;
                    window.Width = settings.Width;
                    window.Height = settings.Height;
                }

                // We only care need to care about the previous state if it was Maximized. The window
                // state is already "Normal" and we don't want a new window to be created "Minimized"
                // since that would confuse the user.
                //
                // So, if the window was maximized before this will maximized it again. However the WPF
                // window positioning code gets confused if you set this before the window has been
                // initialized, so we need to add an event handler to maximize the window after it's
                // initialized instead. - DGG 2016-10-06
                if (settings.WindowState == WindowState.Maximized)
                    window.SourceInitialized += (s, a) => ((Window)s).WindowState = WindowState.Maximized;
            }
            catch (Exception exception)
            {
                string message = $"Unable to restore window position for window '{settingsKey}'.";
                Log.Warn(message, exception);
            }
        }
    }
}
