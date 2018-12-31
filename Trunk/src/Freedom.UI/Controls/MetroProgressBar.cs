using System;
using System.Collections;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Freedom.UI.Controls
{
    /// <summary>
    /// A metrofied ProgressBar.
    /// <see cref="ProgressBar"/>
    /// </summary>
    public class MetroProgressBar : ProgressBar
    {
        public static readonly DependencyProperty EllipseDiameterProperty =
            DependencyProperty.Register("EllipseDiameter", typeof(double), typeof(MetroProgressBar),
                                        new PropertyMetadata(default(double)));

        public static readonly DependencyProperty EllipseOffsetProperty =
            DependencyProperty.Register("EllipseOffset", typeof(double), typeof(MetroProgressBar),
                                        new PropertyMetadata(default(double)));

        private readonly object _storyboardLock = new object();
        private Storyboard _indeterminateStoryboard;

        static MetroProgressBar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MetroProgressBar), new FrameworkPropertyMetadata(typeof(MetroProgressBar)));
            IsIndeterminateProperty.OverrideMetadata(typeof(MetroProgressBar), new FrameworkPropertyMetadata(OnIsIndeterminateChanged));
        }

        public MetroProgressBar()
        {
            SizeChanged += SizeChangedHandler;
            IsVisibleChanged += VisibleChangedHandler;
        }

        private void VisibleChangedHandler(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is bool && (bool)e.NewValue)
                ResetStoryboard(ActualWidth);
        }

        private static void OnIsIndeterminateChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            MetroProgressBar bar = dependencyObject as MetroProgressBar;

            if (bar == null || e.NewValue == e.OldValue) return;

            VisualState indeterminateState = bar.GetIndeterminate();

            FrameworkElement containingObject = bar.GetTemplateChild("ContainingGrid") as FrameworkElement;

            if (indeterminateState == null || containingObject == null) return;

            indeterminateState.Storyboard?.Stop(containingObject);

            bar.ResetStoryboard(bar.ActualWidth);
        }

        /// <summary>
        /// Gets/sets the diameter of the ellipses used in the indeterminate animation.
        /// </summary>
        public double EllipseDiameter
        {
            get { return (double)GetValue(EllipseDiameterProperty); }
            set { SetValue(EllipseDiameterProperty, value); }
        }

        /// <summary>
        /// Gets/sets the offset of the ellipses used in the indeterminate animation.
        /// </summary>
        public double EllipseOffset
        {
            get { return (double)GetValue(EllipseOffsetProperty); }
            set { SetValue(EllipseOffsetProperty, value); }
        }

        private void SizeChangedHandler(object sender, SizeChangedEventArgs e)
        {
            if (Visibility == Visibility.Visible)
                ResetStoryboard(ActualWidth);
        }

        private void ResetStoryboard(double width)
        {
            if (_indeterminateStoryboard == null)
                return;

            lock (_storyboardLock)
            {
                double containerAnimStart = CalcContainerAnimStart(width);
                double containerAnimEnd = CalcContainerAnimEnd(width);
                double ellipseAnimWell = CalcEllipseAnimWell(width);
                double ellipseAnimEnd = CalcEllipseAnimEnd(width);

                //reset the main double animation
                try
                {
                    VisualState indeterminate = GetIndeterminate();

                    if (indeterminate == null || _indeterminateStoryboard == null) return;

                    Storyboard newStoryboard = _indeterminateStoryboard.Clone();

                    Timeline doubleAnim = newStoryboard.Children.First(t => t.Name == "MainDoubleAnim");

                    doubleAnim.SetValue(DoubleAnimation.FromProperty, containerAnimStart);
                    doubleAnim.SetValue(DoubleAnimation.ToProperty, containerAnimEnd);

                    for (int i = 1; i <= 5; i++)
                    {
                        DoubleAnimationUsingKeyFrames doubleAnimParent = (DoubleAnimationUsingKeyFrames)
                            newStoryboard.Children.First(t => t.Name == $"E{i}Anim");

                        DoubleKeyFrame first, second, third;

                        if (i == 1)
                        {
                            first = doubleAnimParent.KeyFrames[1];
                            second = doubleAnimParent.KeyFrames[2];
                            third = doubleAnimParent.KeyFrames[3];
                        }
                        else
                        {
                            first = doubleAnimParent.KeyFrames[2];
                            second = doubleAnimParent.KeyFrames[3];
                            third = doubleAnimParent.KeyFrames[4];
                        }

                        first.Value = ellipseAnimWell;
                        second.Value = ellipseAnimWell;
                        third.Value = ellipseAnimEnd;

                        first.InvalidateProperty(DoubleKeyFrame.ValueProperty);
                        second.InvalidateProperty(DoubleKeyFrame.ValueProperty);
                        third.InvalidateProperty(DoubleKeyFrame.ValueProperty);

                        doubleAnimParent.InvalidateProperty(Storyboard.TargetPropertyProperty);
                        doubleAnimParent.InvalidateProperty(Storyboard.TargetNameProperty);
                    }

                    FrameworkElement containingGrid = GetTemplateChild("ContainingGrid") as FrameworkElement;

                    if (indeterminate.Storyboard != null && containingGrid != null)
                    {
                        indeterminate.Storyboard.Stop(containingGrid);
                        indeterminate.Storyboard.Remove(containingGrid);
                    }

                    indeterminate.Storyboard = newStoryboard;

                    if (!IsIndeterminate)
                        return;

                    if (indeterminate.Storyboard != null && containingGrid != null)
                        indeterminate.Storyboard.Begin(containingGrid, true);
                }
                catch (Exception)
                {
                    //we just ignore 
                }
            }
        }

        private VisualState GetIndeterminate()
        {
            FrameworkElement templateGrid = GetTemplateChild("ContainingGrid") as FrameworkElement;

            if (templateGrid == null)
                return null;

            IList groups = VisualStateManager.GetVisualStateGroups(templateGrid);

            return groups?.OfType<VisualStateGroup>()
                .SelectMany(grp => grp.States.Cast<VisualState>())
                .FirstOrDefault(state => state.Name == "Indeterminate");
        }


        private void SetEllipseDiameter(double width)
        {
            if (width <= 180d)
            {
                EllipseDiameter = 4d;
            }
            else if (width <= 280d)
            {
                EllipseDiameter = 5d;
            }
            else
            {
                EllipseDiameter = 6d;
            }
        }

        private void SetEllipseOffset(double width)
        {
            if (width <= 180)
            {
                EllipseOffset = 4;
            }
            else if (width <= 280)
            {
                EllipseOffset = 7;
            }
            else
            {
                EllipseOffset = 9;
            }
        }

        private static double CalcContainerAnimStart(double width)
        {
            if (width <= 180)
                return -34;

            if (width <= 280)
                return -50.5;

            return -63;
        }

        private static double CalcContainerAnimEnd(double width)
        {
            double firstPart = 0.4352 * width;

            if (width <= 180)
                return firstPart - 25.731;

            if (width <= 280)
                return firstPart + 27.84;

            return firstPart + 58.862;
        }

        private static double CalcEllipseAnimWell(double width)
        {
            return width * 1.0 / 3.0;
        }

        private static double CalcEllipseAnimEnd(double width)
        {
            return width * 2.0 / 3.0;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            lock (_storyboardLock)
            {
                _indeterminateStoryboard = TryFindResource("IndeterminateStoryboard") as Storyboard;
            }

            SizeChangedHandler(null, null);
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            // Update the Ellipse properties to their default values
            // only if they haven't been user-set.
            if (EllipseDiameter.Equals(0))
                SetEllipseDiameter(ActualWidth);

            if (EllipseOffset.Equals(0))
                SetEllipseOffset(ActualWidth);
        }
    }
}
