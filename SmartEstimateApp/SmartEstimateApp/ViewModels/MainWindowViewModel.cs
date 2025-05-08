using SmartEstimateApp.Models;
using System.Windows;
using System.Windows.Threading;

namespace SmartEstimateApp.ViewModels
{
    public class MainWindowViewModel : PropertyChangedBase
    {
        private string _errorMessage;
        private bool _isErrorVisible;
        private Visibility _loadingVisibility;
        private string _currentDateTime;
        private DispatcherTimer _dateTimeTimer;
        private DispatcherTimer _errorTimer;

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value, nameof(ErrorMessage));
        }

        public bool IsErrorVisible
        {
            get => _isErrorVisible;
            set => SetProperty(ref _isErrorVisible, value, nameof(IsErrorVisible));
        }

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

        public MainWindowViewModel()
        {
            IsErrorVisible = false;
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
        }


        private void UpdateDateTime()
        {
            CurrentDateTime = DateTime.Now.ToString("dd.MM.yyyy HH:mm");
        }

        public void ShowError(string message)
        {
            ErrorMessage = message;
            IsErrorVisible = true;

            if (_errorTimer.IsEnabled)
            {
                _errorTimer.Stop();
            }

            _errorTimer.Start();
        }

        public void ShowLoading() => LoadingVisibility = Visibility.Visible;

        public void HideLoading() => LoadingVisibility = Visibility.Collapsed;

    }
}