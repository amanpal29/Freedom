using System.Collections.Generic;

namespace Freedom.Client.Infrastructure.Dialogs.ViewModels
{
    public class CancelMessageViewModel : CustomMessageViewModel
    {
        public CancelMessageViewModel()
        {
        }

        public CancelMessageViewModel(string mainInstructionText)
        {
            MainInstructionText = mainInstructionText;
        }

        public CancelMessageViewModel(string mainInstructionText, string secondaryInstructionText)
        {
            MainInstructionText = mainInstructionText;
            SecondaryInstructionText = secondaryInstructionText;
        }

        public override IEnumerable<DialogButtonViewModelBase> DialogButtons
        {
            get
            {
                yield return new DialogButtonViewModel("_Cancel", () => DialogResult = false, DialogButtonOptions.IsDefault | DialogButtonOptions.IsCancel);
            }
        }
    }
}
