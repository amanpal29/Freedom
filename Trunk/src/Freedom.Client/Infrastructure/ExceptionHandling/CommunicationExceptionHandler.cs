using System;
using Freedom.Domain.Infrastructure.ExceptionHandling;
using Freedom.Domain.Exceptions;
using Freedom.Client.Infrastructure.Dialogs.ViewModels;
using Freedom.Client.Infrastructure.Dialogs;

namespace Freedom.Client.Infrastructure.ExceptionHandling
{
    public class CommunicationExceptionHandler : IExceptionHandler
    {
        public ExceptionHandledStatus Handle(Exception exception, ExceptionContext context)
        {
            if (context.IsLastChance || !(exception is CommunicationException))
                return ExceptionHandledStatus.Unhandled;

            ExceptionHandledStatus status = ExceptionHandledStatus.Unhandled;

            CustomMessageViewModel customMessageViewModel;

            string additionalInstructions = null;

            if (context.CanRetry)
            {
                customMessageViewModel = new RetryOrCancelViewModel();
                additionalInstructions = "Click \"Retry\" to attempt the operation again.";
            }
            else
            {
                customMessageViewModel = new CancelMessageViewModel();
            }

            customMessageViewModel.MainInstructionText = $"Unable to communicate with the {App.Name} Server.";

            customMessageViewModel.SecondaryInstructionText = !string.IsNullOrEmpty(additionalInstructions)
                ? $"{exception.Message}\n\n{additionalInstructions}"
                : exception.Message;

            bool? dialogResult = Dialog.Show(customMessageViewModel);

            if (dialogResult == true)
                status = context.CanRetry ? ExceptionHandledStatus.Retry : ExceptionHandledStatus.Handled;

            if (dialogResult == false)
                status = ExceptionHandledStatus.Cancel;

            return status;
        }       
    }
}
