using Freedom.Client.Infrastructure.Images;
using System.Windows;
using System.Windows.Media;

namespace Freedom.Client.Infrastructure.Dialogs.ViewModels
{
    public class CustomMessageViewModel : DialogViewModel
    {
        private string _mainInstructionText;
        private string _secondaryInstructionText;
        private MessageBoxImage _messageBoxImage = MessageBoxImage.Warning;

        public string MainInstructionText
        {
            get { return _mainInstructionText; }
            set
            {
                if (_mainInstructionText != value)
                {
                    _mainInstructionText = value;
                    OnPropertyChanged(nameof(MainInstructionText));
                }
            }
        }

        public string SecondaryInstructionText
        {
            get { return _secondaryInstructionText; }
            set
            {
                if (_secondaryInstructionText != value)
                {
                    _secondaryInstructionText = value;
                    OnPropertyChanged(nameof(SecondaryInstructionText));
                }
            }
        }

        public MessageBoxImage MessageBoxImage
        {
            get { return _messageBoxImage; }
            set
            {
                if (_messageBoxImage != value)
                {
                    _messageBoxImage = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ImageSource));
                }
            }
        }

        public ImageSource ImageSource
        {
            get
            {
                switch (MessageBoxImage)
                {
                    case MessageBoxImage.Error:
                        return ImageFactory.Get("/FreedomClient;component/Resources/MessageIcons/error.png");

                    case MessageBoxImage.Question:
                        return ImageFactory.Get("/FreedomClient;component/Resources/MessageIcons/question.png");

                    case MessageBoxImage.Warning:
                        return ImageFactory.Get("/FreedomClient;component/Resources/MessageIcons/warning.png");

                    case MessageBoxImage.Information:
                        return ImageFactory.Get("/FreedomClient;component/Resources/MessageIcons/info.png");

                    default:
                        return null;
                }
            }
        }
    }
}
