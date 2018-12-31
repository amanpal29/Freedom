using System;
using System.Reflection;
using log4net;
using Freedom.Domain.Infrastructure.ExceptionHandling;

namespace Freedom.Client.Infrastructure.ExceptionHandling
{
    public class PopUpNullReferenceExceptionSilencer : IExceptionHandler
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        
        public ExceptionHandledStatus Handle(Exception exception, ExceptionContext context)
        {
            ExceptionHandledStatus status = ExceptionHandledStatus.Unhandled;

            if (exception.GetType() == typeof(NullReferenceException) && exception.StackTrace.StartsWith("   at System.Windows.Controls.Primitives.Popup.PopupSecurityHelper.SetWindowRootVisual(Visual v)"))
            {
                Log.Info("An exception was slienced.", exception);
                status = ExceptionHandledStatus.Handled;
            }

            return status;
        }
    }
}
