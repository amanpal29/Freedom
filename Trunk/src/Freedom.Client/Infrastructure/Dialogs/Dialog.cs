using Freedom.Client.Infrastructure.Dialogs.ViewModels;
using System.Windows;

namespace Freedom.Client.Infrastructure.Dialogs
{
    public static class Dialog
    {
        public static bool? Show(DialogViewModel dialogViewModel)
        {
            return Show(null, dialogViewModel);
        }

        public static bool? Show(object owner, DialogViewModel dialogViewModel)
        {
            IDialogService dialogService = IoC.Get<IDialogService>();

            return dialogService.Show(owner, dialogViewModel);
        }

        public static MessageBoxResult Show(object owner, string text, MessageBoxButton button = MessageBoxButton.OK, MessageBoxImage image = MessageBoxImage.None)
        {
            IDialogService dialogService = IoC.Get<IDialogService>();

            MessageBoxViewModel viewModel = new MessageBoxViewModel(text, button, image);

            dialogService.Show(owner, viewModel);

            return viewModel.MessageBoxResult;
        }

        public static MessageBoxResult Show(object owner, string text, MessageBoxButton button, MessageBoxImage image, MessageBoxResult defaultResult)
        {
            IDialogService dialogService = IoC.Get<IDialogService>();

            MessageBoxViewModel viewModel = new MessageBoxViewModel(text, button, image, defaultResult);

            dialogService.Show(owner, viewModel);

            return viewModel.MessageBoxResult;
        }

        public static bool? ShowWarning(object owner, string mainText, string secondaryText = null)
        {
            IDialogService dialogService = IoC.Get<IDialogService>();

            CancelMessageViewModel viewModel = new CancelMessageViewModel(mainText);
            viewModel.MessageBoxImage = MessageBoxImage.Warning;
            viewModel.SecondaryInstructionText = secondaryText;
            dialogService.Show(owner, viewModel);

            return viewModel.DialogResult;
        }

        public static bool? ShowError(object owner, string mainText, string secondaryText = null)
        {
            IDialogService dialogService = IoC.Get<IDialogService>();

            CancelMessageViewModel viewModel = new CancelMessageViewModel(mainText);
            viewModel.MessageBoxImage = MessageBoxImage.Error;
            viewModel.SecondaryInstructionText = secondaryText;
            dialogService.Show(owner, viewModel);

            return viewModel.DialogResult;
        }
    }
}
