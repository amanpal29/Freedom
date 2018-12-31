using System.Windows;

namespace Freedom.UI.Behaviors
{
    public class WindowBehaviors
    {
        #region DialogResultBehavior

        public static readonly DependencyProperty DialogResultProperty =
            DependencyProperty.RegisterAttached("DialogResult", typeof(bool?), typeof(WindowBehaviors),
                new PropertyMetadata(DialogResultChanged));

        private static void DialogResultChanged(
            DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            Window window = Window.GetWindow(obj);

            if (window != null)
            {
                window.DialogResult = args.NewValue as bool?;
            }
        }

        public static bool? GetDialogResult(Window target)
        {
            return target.GetValue(DialogResultProperty) as bool?;
        }

        public static void SetDialogResult(Window target, bool? value)
        {
            target.SetValue(DialogResultProperty, value);
        }

        #endregion

        #region IsOpenBehavior

        public static readonly DependencyProperty IsOpenProperty =
            DependencyProperty.RegisterAttached("IsOpen", typeof(bool), typeof(WindowBehaviors),
                new PropertyMetadata(IsOpenChanged));

        private static void IsOpenChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            Window window = Window.GetWindow(obj);

            if (window != null && !((bool)args.NewValue))
                window.Close();
        }

        public static bool GetIsOpen(Window target)
        {
            return (bool)target.GetValue(IsOpenProperty);
        }

        public static void SetIsOpen(Window target, bool value)
        {
            target.SetValue(IsOpenProperty, value);
        }

        #endregion
    }
}
