using System;
using System.Windows;
using Freedom.ViewModels;

namespace Freedom.UI
{
    public static class WindowFactory
    {
        public static Window FromViewModelView(ViewModelBase viewModel, Type viewType)
        {
            Window window = new Window();

            window.SizeToContent = SizeToContent.WidthAndHeight;

            window.DataContext = viewModel;

            window.Content = viewModel;

            window.ContentTemplate = new DataTemplate(viewModel.GetType())
            {
                VisualTree = new FrameworkElementFactory(viewType)
            };

            return window;
        }
    }
}
