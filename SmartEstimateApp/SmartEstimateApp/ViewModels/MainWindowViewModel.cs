using SmartEstimateApp.Commands;
using SmartEstimateApp.Models;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace SmartEstimateApp.ViewModels
{
    public class MainWindowViewModel : PropertyChangedBase
    {
        #region Fields
        private string _errorMessage;
        private bool _isErrorVisible;
        private string _successMessage;
        private bool _isSuccessVisible;
        private Visibility _loadingVisibility;
        private string _currentDateTime;
        private DispatcherTimer _dateTimeTimer;
        private DispatcherTimer _errorTimer;
        private DispatcherTimer _successTimer;
        #endregion

        #region Properties
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value, nameof(ErrorMessage));
        }

        public bool IsErrorVisible
        {
            get => _isErrorVisible;
            set
            {
                if (SetProperty(ref _isErrorVisible, value, nameof(IsErrorVisible)))
                {
                    OnPropertyChanged(nameof(IsNotificationVisible));
                }
            }
        }

        public string SuccessMessage
        {
            get => _successMessage;
            set => SetProperty(ref _successMessage, value, nameof(SuccessMessage));
        }

        public bool IsSuccessVisible
        {
            get => _isSuccessVisible;
            set
            {
                if (SetProperty(ref _isSuccessVisible, value, nameof(IsSuccessVisible)))
                {
                    OnPropertyChanged(nameof(IsNotificationVisible));
                }
            }
        }

        public bool IsNotificationVisible => IsErrorVisible || IsSuccessVisible;

        public Visibility LoadingVisibility
        {
            get => _loadingVisibility;
            set => SetProperty(ref _loadingVisibility, value, nameof(LoadingVisibility));
        }

        public string CurrentDateTime
        {
            get => _currentDateTime;
            set => SetProperty(ref _currentDateTime, value, nameof(CurrentDateTime));
        }
        #endregion

        #region Commands
        private ICommand _hideErrorCommand;
        public ICommand HideErrorCommand => _hideErrorCommand ??= new RelayCommand(obj => HideError());

        private ICommand _hideSuccessCommand;
        public ICommand HideSuccessCommand => _hideSuccessCommand ??= new RelayCommand(obj => HideSuccess());
        #endregion

        #region Constructor
        public MainWindowViewModel()
        {
            IsErrorVisible = false;
            IsSuccessVisible = false;
            LoadingVisibility = Visibility.Collapsed;

            _dateTimeTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _dateTimeTimer.Tick += (s, e) => UpdateDateTime();
            _dateTimeTimer.Start();
            UpdateDateTime();

            _errorTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(5)
            };
            _errorTimer.Tick += (s, e) =>
            {
                IsErrorVisible = false;
                _errorTimer.Stop();
            };

            _successTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(5)
            };
            _successTimer.Tick += (s, e) =>
            {
                IsSuccessVisible = false;
                _successTimer.Stop();
            };
        }
        #endregion

        #region Methods
        private void UpdateDateTime()
        {
            CurrentDateTime = DateTime.Now.ToString("dd.MM.yyyy HH:mm");
        }

        public void ShowError(string message)
        {
            ErrorMessage = message;
            HideSuccess();
            IsErrorVisible = true;

            if (_errorTimer.IsEnabled)
            {
                _errorTimer.Stop();
            }
            _errorTimer.Start();
        }

        public void HideError()
        {
            IsErrorVisible = false;
            if (_errorTimer.IsEnabled)
            {
                _errorTimer.Stop();
            }
        }

        public void ShowSuccess(string message)
        {
            SuccessMessage = message;
            HideError();
            IsSuccessVisible = true;
            if (_successTimer.IsEnabled)
            {
                _successTimer.Stop();
            }
            _successTimer.Start();
        }

        public void HideSuccess()
        {
            IsSuccessVisible = false;
            if (_successTimer.IsEnabled)
            {
                _successTimer.Stop();
            }
        }

        public void ShowLoading() => LoadingVisibility = Visibility.Visible;
        public void HideLoading() => LoadingVisibility = Visibility.Collapsed;
        #endregion
    }

}