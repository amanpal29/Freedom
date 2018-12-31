using System.Collections.Generic;


namespace Freedom.Client.Infrastructure.Dialogs.ViewModels
{
    public class RetryOrCancelViewModel : CustomMessageViewModel
    {
        public override IEnumerable<DialogButtonViewModelBase> DialogButtons
        {
            get
            {
                yield return new DialogButtonViewModel("_Retry", () => DialogResult = true);
                yield return new DialogButtonViewModel("_Cancel", () => DialogResult = false, DialogButtonOptions.IsDefault | DialogButtonOptions.IsCancel);
            }
        }
    }
}
