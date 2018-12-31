using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Freedom.ViewModels;

namespace Freedom.Client.Infrastructure.Dialogs
{
    public class DialogViewModel : WindowViewModel
    {
        private bool _allowResize;
        private bool _overrideDialogVerticalScrollViewer;
        private bool? _dialogResult;
        private string _caption = App.Name;
        private string _defaultButtonCaption = "_OK";
        private string _cancelButtonCaption = "_Cancel";

        private DialogButtonViewModelCollection _dialogButtons;

        public bool AllowResize
        {
            get { return _allowResize; }
            set
            {
                if (_allowResize == value) return;
                _allowResize = value;
                OnPropertyChanged();
            }
        }

        public bool OverrideDialogVerticalScrollViewer
        {
            get { return _overrideDialogVerticalScrollViewer; }
            set
            {
                if (_overrideDialogVerticalScrollViewer == value) return;
                _overrideDialogVerticalScrollViewer = value;
                OnPropertyChanged();
            }
        }

        public string Caption
        {
            get { return _caption; }
            set
            {
                if (_caption == value) return;
                _caption = value;
                OnPropertyChanged();
            }
        }

        public bool? DialogResult
        {
            get { return _dialogResult; }
            set
            {
                if (_dialogResult == value) return;
                _dialogResult = value;
                OnPropertyChanged();
            }
        }

        public string DefaultButtonCaption
        {
            get { return _defaultButtonCaption; }
            set
            {
                if (_defaultButtonCaption == value) return;
                _defaultButtonCaption = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DialogButtons));
            }
        }

        public string CancelButtonCaption
        {
            get { return _cancelButtonCaption; }
            set
            {
                if (_cancelButtonCaption == value) return;
                _cancelButtonCaption = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DialogButtons));
            }
        }

        public virtual bool CanClickDefaultButton => true;

        public virtual bool CanClickCancelButton => true;

        public virtual IEnumerable<DialogButtonViewModelBase> DialogButtons
        {
            get
            {
                yield return new DialogButtonViewModel(DefaultButtonCaption, () => DialogResult = true,
                    () => CanClickDefaultButton, DialogButtonOptions.IsDefault);

                yield return new DialogButtonViewModel(CancelButtonCaption, () => DialogResult = false,
                    () => CanClickCancelButton, DialogButtonOptions.IsCancel);
            }
        }

        public void ClickDefaultButton()
        {
            ICommand command = DialogButtons.FirstOrDefault(b => b.IsDefault);

            command?.Execute(null);
        }

        public void ClickCancelButton()
        {
            ICommand command = DialogButtons.FirstOrDefault(b => b.IsCancel);

            command?.Execute(null);
        }

        protected void ReloadButtons()
        {
            _dialogButtons = new DialogButtonViewModelCollection(DialogButtons);

            OnPropertyChanged(nameof(DangerousButtons));
            OnPropertyChanged(nameof(SafeButtons));
        }

        public IEnumerable<DialogButtonViewModelBase> DangerousButtons
        {
            get
            {
                if (_dialogButtons == null)
                    _dialogButtons = new DialogButtonViewModelCollection(DialogButtons);

                return _dialogButtons.Where(b => b.IsDangerous);
            }
        }

        public IEnumerable<DialogButtonViewModelBase> SafeButtons
        {
            get
            {
                if (_dialogButtons == null)
                    _dialogButtons = new DialogButtonViewModelCollection(DialogButtons);

                return _dialogButtons.Where(b => !b.IsDangerous);
            }
        }
    }
}
