using Freedom.Client.Infrastructure.Images;
using Freedom.Client.Resources;
using Freedom.Domain.Interfaces;
using Freedom.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Freedom.Client.ViewModel
{
    public class MainViewModel : WindowViewModel, IRefreshable
    {
        #region Fields
                
        private readonly ObservableCollection<ViewModelBase> _statusBarItems;

        private ViewModelBase _workspace;

        #endregion

        #region Constructor

        public MainViewModel()
            : this(null)
        {
        }

        public MainViewModel(ViewModelBase workspace)
        {
            _workspace = workspace;
            _statusBarItems = new ObservableCollection<ViewModelBase>();            
            _statusBarItems.Add(new CurrentUserViewModel());
        }

        #endregion

        #region Properties

        public ViewModelBase Workspace
        {
            get { return _workspace; }
            set
            {
                if (_workspace == value) return;
                _workspace = value;
                OnPropertyChanged();
            }
        }        
        public ICollection<ViewModelBase> StatusBarItems => _statusBarItems;
                
        public ImageSource FeedbackIcon => ImageFactory.Get(FontIcons.FeedbackIconColor, 32);

        #endregion

        #region Event Handlers

        public override void OnWindowClosing(object sender, CancelEventArgs eventArgs)
        {
            base.OnWindowClosing(sender, eventArgs);

            WindowViewModel windowViewModel = _workspace as WindowViewModel;

            windowViewModel?.OnWindowClosing(sender, eventArgs);
        }

        public override void OnWindowClosed(object sender, EventArgs eventArgs)
        {
            base.OnWindowClosed(sender, eventArgs);

            WindowViewModel windowViewModel = _workspace as WindowViewModel;

            windowViewModel?.OnWindowClosed(sender, eventArgs);
        }

        #endregion

        #region Overrides of ViewModelBase

        public override IEnumerable<ViewModelBase> Children
        {
            get
            {
                yield return _workspace;
                
                foreach (ViewModelBase statusBarItem in StatusBarItems)
                    yield return statusBarItem;
            }
        }

        #endregion

        #region Implementation of IRefreshable

        public async Task RefreshAsync()
        {
            IRefreshable refreshable = _workspace as IRefreshable;

            if (refreshable != null)
            {
                await refreshable.RefreshAsync();
            }
        }

        #endregion
    }
}
