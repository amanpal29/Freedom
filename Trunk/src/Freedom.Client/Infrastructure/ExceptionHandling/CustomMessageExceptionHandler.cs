using Freedom.Client.Infrastructure.Dialogs;
using Freedom.Client.Infrastructure.Dialogs.ViewModels;
using Freedom.Domain.Infrastructure.ExceptionHandling;
using System;

namespace Freedom.Client.Infrastructure.ExceptionHandling
{
    public class CustomMessageExceptionHandler : IExceptionHandler
    {
        private readonly Type _exceptionType;
        private readonly string _customMessage;
        private readonly bool _allowRetry;

        public CustomMessageExceptionHandler(Type exceptionType, string customMessage, bool allowRetry = true)
        {
            _exceptionType = exceptionType;
            _customMessage = customMessage;
            _allowRetry = allowRetry;
        }

        public ExceptionHandledStatus Handle(Exception exception, ExceptionContext context)
        {
            ExceptionHandledStatus status = ExceptionHandledStatus.Unhandled;

            if (!context.IsLastChance && _exceptionType.IsInstanceOfType(exception))
            {
                CustomMessageViewModel customMessageViewModel;

                if (_allowRetry)
                {
                    customMessageViewModel = new RetryOrCancelViewModel();
                    customMessageViewModel.SecondaryInstructionText = "Click \"Retry\" to attempt the operation again.";
                }
                else
                {
                    customMessageViewModel = new CancelMessageViewModel();
                }

                customMessageViewModel.MainInstructionText = _customMessage ?? exception.Message;

                bool? dialogResult = Dialog.Show(customMessageViewModel);

                if (dialogResult == true && _allowRetry)
                    status = ExceptionHandledStatus.Retry;

                if (dialogResult == false)
                    status = ExceptionHandledStatus.Cancel;
            }

            return status;
        }
    }
}
