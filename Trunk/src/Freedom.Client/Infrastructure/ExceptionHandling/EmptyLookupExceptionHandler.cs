using System;
using System.Windows;
using Freedom.Domain.Infrastructure.ExceptionHandling;
using Freedom.Domain.Exceptions;
using Freedom.Client.Infrastructure.Dialogs.ViewModels;
using Freedom.Client.Infrastructure.Dialogs;

namespace Freedom.Client.Infrastructure.ExceptionHandling
{
    public class EmptyLookupExceptionHandler : IExceptionHandler
    {
        public ExceptionHandledStatus Handle(Exception exception, ExceptionContext context)
        {
            if (exception is EmptyLookupException)
            {
                CustomMessageViewModel customMessageViewModel = new CancelMessageViewModel();

                customMessageViewModel.MainInstructionText = "Can't start workflow";
                customMessageViewModel.SecondaryInstructionText = exception.Message;
                customMessageViewModel.MessageBoxImage = MessageBoxImage.Exclamation;

                Dialog.Show(customMessageViewModel);

                return ExceptionHandledStatus.Handled;
            }

            return ExceptionHandledStatus.Unhandled;
            
        }
    }
}
