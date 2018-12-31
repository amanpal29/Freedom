using System;
using System.Windows;
using Freedom.Domain.Infrastructure.ExceptionHandling;
using Freedom.UI;

namespace Freedom.Client.Infrastructure.ExceptionHandling
{
    public class WindowedExceptionHandler : IExceptionHandler
    {
        public ExceptionHandledStatus Handle(Exception exception, ExceptionContext context)
        {
            if (context.IsLastChance)
            {
                ExceptionViewModel viewModel = new ExceptionViewModel(exception, context.CanRetry, context.CanCancel);

                Window window = WindowFactory.FromViewModelView(viewModel, typeof(ExceptionView));

                window.SizeToContent = SizeToContent.Manual;
                window.Height = 300;
                window.Width = 400;

                viewModel.PropertyChanged += (sender, args) => { if (viewModel.DialogResult.HasValue) window.Close(); };

                window.ShowDialog();

                return viewModel.DialogResult ?? ExceptionHandledStatus.Unhandled;
            }

            return ExceptionHandledStatus.Unhandled;
        }
    }
}
