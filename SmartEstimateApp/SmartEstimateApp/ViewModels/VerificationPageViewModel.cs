using Bl;
using SmartEstimateApp.Commands;
using SmartEstimateApp.Models;
using SmartEstimateApp.Navigation;
using System.Windows.Input;
using System.Windows.Threading;

namespace SmartEstimateApp.ViewModels
{
    public class VerificationPageViewModel : PropertyChangedBase
    {
        private readonly MainWindowViewModel _mainViewModel;
        private readonly INavigationService _navigationService;
        private readonly EmailVerificationServiceBL _emailVerificationService;

        private string _verificationCode;
        private string _errorMessage;
        private string _email;
        private bool _isResendEnabled;
        private bool _isResendCooldownActive;
        private string _countdownText;
        private DispatcherTimer _resendTimer;
        private DateTime _resendCooldownEnd;

        public string VerificationCode
        {
            get => _verificationCode;
            set => SetProperty(ref _verificationCode, value, nameof(VerificationCode));
        }

        public bool IsResendEnabled
        {
            get => _isResendEnabled;
            set => SetProperty(ref _isResendEnabled, value, nameof(IsResendEnabled));
        }

        public bool IsResendCooldownActive
        {
            get => _isResendCooldownActive;
            set => SetProperty(ref _isResendCooldownActive, value, nameof(IsResendCooldownActive));
        }

        public string CountdownText
        {
            get => _countdownText;
            set => SetProperty(ref _countdownText, value, nameof(CountdownText));
        }

        public ICommand VerifyCodeCommand { get; }
        public ICommand CancelVerificationCommand { get; }
        public ICommand ResendCodeCommand { get; }

        public event Action VerificationSuccess;

        public VerificationPageViewModel(MainWindowViewModel mainViewModel, INavigationService navigationService, EmailVerificationServiceBL emailVerificationService)
        {
            _mainViewModel = mainViewModel;
            _navigationService = navigationService;
            _emailVerificationService = emailVerificationService;

            VerifyCodeCommand = new RelayCommand(VerifyCodeAsync, () => !string.IsNullOrWhiteSpace(VerificationCode));
            CancelVerificationCommand = new RelayCommand(CancelVerification);
            ResendCodeCommand = new RelayCommand(ResendCodeAsync, () => IsResendEnabled);

            IsResendEnabled = true;
            IsResendCooldownActive = false;
        }

        public void SetEmail(string email)
        {
            _email = email;
        }

        private void VerifyCodeAsync()
        {
            try
            {
                _mainViewModel.ShowLoading();

                if (_emailVerificationService.VerifyCode(_email, VerificationCode))
                {
                    VerificationSuccess?.Invoke();
                }
                else
                {
                    _mainViewModel.ShowError("Неверный код подтверждения");
                }
            }
            catch (Exception ex)
            {
                _mainViewModel.ShowError($"Ошибка при проверке кода: {ex.Message}");
            }
            finally
            {
                _mainViewModel.HideLoading();
            }
        }

        private async void ResendCodeAsync()
        {
            try
            {
                _mainViewModel.ShowLoading();

                await _emailVerificationService.SendVerificationCodeAsync(_email);
                StartResendCooldown();
            }
            catch (Exception ex)
            {
                _mainViewModel.ShowError($"Ошибка при повторной отправке кода: {ex.Message}");
            }
            finally
            {
                _mainViewModel.HideLoading();
            }
        }

        private void CancelVerification()
        {
            _navigationService.GoBack();
        }

        private void StartResendCooldown()
        {
            IsResendEnabled = false;
            IsResendCooldownActive = true;
            _resendCooldownEnd = DateTime.UtcNow.AddSeconds(60);

            _resendTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100)
            };
            _resendTimer.Tick += (s, e) =>
            {
                var timeLeft = _resendCooldownEnd - DateTime.UtcNow;
                if (timeLeft.TotalSeconds <= 0)
                {
                    _resendTimer.Stop();
                    IsResendEnabled = true;
                    IsResendCooldownActive = false;
                    CountdownText = null;
                }
                else
                {
                    CountdownText = $"Повторная отправка доступна через {(int)timeLeft.TotalSeconds} сек.";
                }
            };
            _resendTimer.Start();
        }
    }
}