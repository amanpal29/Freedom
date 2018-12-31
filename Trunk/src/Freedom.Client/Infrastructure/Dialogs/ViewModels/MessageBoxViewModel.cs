using System.Collections.Generic;
using System.Windows;

namespace Freedom.Client.Infrastructure.Dialogs.ViewModels
{
    public class MessageBoxViewModel : CustomMessageViewModel
    {
        private MessageBoxButton _messageBoxButton = MessageBoxButton.OK;
        private MessageBoxResult _defaultButton = MessageBoxResult.None;
        private MessageBoxResult _messageBoxResult = MessageBoxResult.None;

        public MessageBoxViewModel(
                string messageBoxText = null,
                MessageBoxButton button = MessageBoxButton.OK,
                MessageBoxImage image = MessageBoxImage.None)
        {
            MainInstructionText = messageBoxText;
            MessageBoxImage = image;
            MessageBoxButton = button;

            switch (MessageBoxButton)
            {
                case MessageBoxButton.OK:
                case MessageBoxButton.OKCancel:
                    _defaultButton = MessageBoxResult.OK;
                    break;

                case MessageBoxButton.YesNo:
                case MessageBoxButton.YesNoCancel:
                    _defaultButton = MessageBoxResult.Yes;
                    break;
            }
        }

        public MessageBoxViewModel(string messageBoxText, MessageBoxButton button, MessageBoxImage image, MessageBoxResult defaultResult)
            : this(messageBoxText, button, image)
        {
            _defaultButton = defaultResult;
        }

        public MessageBoxButton MessageBoxButton
        {
            get { return _messageBoxButton; }
            set
            {
                if (MessageBoxButton != value)
                {
                    _messageBoxButton = value;
                    OnPropertyChanged("MessageBoxButton");
                    OnPropertyChanged("DialogButtons");
                }
            }
        }

        public MessageBoxResult DefaultButton
        {
            get { return _defaultButton; }
            set
            {
                if (_defaultButton != value)
                {
                    _defaultButton = value;
                    OnPropertyChanged("DefaultButton");
                    OnPropertyChanged("DialogButtons");
                }
            }
        }

        public MessageBoxResult MessageBoxResult
        {
            get { return _messageBoxResult; }
            set
            {
                if (_messageBoxResult != value)
                {
                    _messageBoxResult = value;
                    OnPropertyChanged("MessageBoxResult");
                }

                switch (_messageBoxResult)
                {
                    case MessageBoxResult.OK:
                    case MessageBoxResult.Yes:
                        DialogResult = true;
                        break;

                    default:
                        DialogResult = false;
                        break;
                }
            }
        }

        public override IEnumerable<DialogButtonViewModelBase> DialogButtons
        {
            get
            {
                switch (MessageBoxButton)
                {
                    case MessageBoxButton.OK:
                        yield return new DialogButtonViewModel(DefaultButtonCaption, () => MessageBoxResult = MessageBoxResult.OK, () => CanClickDefaultButton, DialogButtonOptions.IsDefault | DialogButtonOptions.IsCancel);
                        break;

                    case MessageBoxButton.OKCancel:
                        yield return new DialogButtonViewModel(DefaultButtonCaption, () => MessageBoxResult = MessageBoxResult.OK, () => CanClickDefaultButton, DefaultButton == MessageBoxResult.OK ? DialogButtonOptions.IsDefault : DialogButtonOptions.None);
                        yield return new DialogButtonViewModel(CancelButtonCaption, () => MessageBoxResult = MessageBoxResult.Cancel, () => CanClickCancelButton, (DefaultButton == MessageBoxResult.Cancel ? DialogButtonOptions.IsDefault : DialogButtonOptions.None) | DialogButtonOptions.IsCancel);
                        break;

                    case MessageBoxButton.YesNo:
                        yield return new DialogButtonViewModel("Yes", () => MessageBoxResult = MessageBoxResult.Yes, DefaultButton == MessageBoxResult.Yes ? DialogButtonOptions.IsDefault : DialogButtonOptions.None);
                        yield return new DialogButtonViewModel("No", () => MessageBoxResult = MessageBoxResult.No, DefaultButton == MessageBoxResult.No ? DialogButtonOptions.IsDefault : DialogButtonOptions.None);
                        break;

                    case MessageBoxButton.YesNoCancel:
                        yield return new DialogButtonViewModel("Yes", () => MessageBoxResult = MessageBoxResult.Yes, DefaultButton == MessageBoxResult.Yes ? DialogButtonOptions.IsDefault : DialogButtonOptions.None);
                        yield return new DialogButtonViewModel("No", () => MessageBoxResult = MessageBoxResult.No, DefaultButton == MessageBoxResult.No ? DialogButtonOptions.IsDefault : DialogButtonOptions.None);
                        yield return new DialogButtonViewModel("Cancel", () => MessageBoxResult = MessageBoxResult.Cancel, DialogButtonOptions.IsCancel);
                        break;

                }
            }
        }
    }
}
