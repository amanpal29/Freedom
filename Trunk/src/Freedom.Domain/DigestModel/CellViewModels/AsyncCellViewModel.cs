using System;
using Freedom.ViewModels;
using System.Threading.Tasks;

namespace Freedom.Domain.DigestModel.CellViewModels
{
    public abstract class AsyncCellViewModel : ViewModelBase, IAsyncInitializable
    {
        private bool _isInitialized;
        private bool _isLoading;
        private Exception _loadingError;

        public bool IsInitialized
        {
            get { return _isInitialized; }
            set
            {
                if (value == _isInitialized) return;
                _isInitialized = value;
                OnPropertyChanged();
            }
        }

        public async Task InitializeAsync()
        {
            if (IsLoading || IsInitialized) return;

            IsLoading = true;
            LoadingError = null;

            try
            {
                await InitializeCoreAsync();

                IsInitialized = true;
            }
            catch (Exception ex)
            {
                LoadingError = ex;
            }
            finally
            {
                IsLoading = false;
            }
        }

        public abstract Task InitializeCoreAsync();

        public bool IsLoading
        {
            get { return _isLoading; }
            set
            {
                if (value == _isLoading) return;
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        public bool LoadingFailed => _loadingError != null;

        public Exception LoadingError
        {
            get { return _loadingError; }
            set
            {
                if (Equals(value, _loadingError)) return;
                _loadingError = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(LoadingFailed));
            }
        }
    }
}
