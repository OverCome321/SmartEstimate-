using Bl;
using Bl.Managers;
using Entities;
using SmartEstimateApp.Commands;
using SmartEstimateApp.Models;
using SmartEstimateApp.Navigation.Interfaces;
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
        private string _email;
        private bool _isResendEnabled;
        private bool _isResendCooldownActive;
        private string _countdownText;
        private DispatcherTimer _resendTimer;
        private DateTime _resendCooldownEnd;
        private string _sessionId;

        // Field to track the purpose of verification
        private VerificationPurpose _verificationPurpose;

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

            VerifyCodeCommand = new RelayCommand(
                obj => VerifyCodeAsync(),
                obj => !string.IsNullOrWhiteSpace(VerificationCode)
            );

            CancelVerificationCommand = new RelayCommand(obj => CancelVerification());

            ResendCodeCommand = new RelayCommand(
                obj => ResendCodeAsync(),
                obj => IsResendEnabled
            );

            IsResendEnabled = true;
            IsResendCooldownActive = false;

            // Default to login verification
            _verificationPurpose = VerificationPurpose.Login;
        }

        public async void SetEmail(string email, VerificationPurpose purpose = VerificationPurpose.Login)
        {
            // Clear any existing verification state
            ClearState();

            _email = email;
            _verificationPurpose = purpose;
            await SendInitialCodeAsync();
        }

        public void ClearVerificationHandlers()
        {
            VerificationSuccess = null;
        }

        private void ClearState()
        {
            // Clear previous verification code
            VerificationCode = string.Empty;

            // Invalidate any existing codes for this email/purpose
            if (!string.IsNullOrEmpty(_email))
            {
                _emailVerificationService.InvalidateCode(_email, _verificationPurpose);
            }

            // Clear the session ID
            _sessionId = null;

            // Clear event handlers
            ClearVerificationHandlers();
        }

        private async Task SendInitialCodeAsync()
        {
            try
            {
                _mainViewModel.ShowLoading();

                var (canSend, errorMessage, remainingSeconds) = SendAttemptStore.CanSendCode(_email);

                if (!canSend)
                {
                    _mainViewModel.ShowError(errorMessage);
                    StartResendCooldown(remainingSeconds.Value);
                    return;
                }

                // Отправляем код с указанием цели
                _sessionId = await _emailVerificationService.SendVerificationCodeAsync(_email, _verificationPurpose);

                // Регистрируем попытку отправки
                var (isInCooldown, cooldownSeconds) = SendAttemptStore.RecordAttempt(_email);

                // Если нужно включить период ожидания
                if (isInCooldown)
                {
                    StartResendCooldown(cooldownSeconds.Value);
                }

                _mainViewModel.ShowSuccess("Код подтверждения отправлен на вашу почту");
            }
            catch (Exception ex)
            {
                _mainViewModel.ShowError(ex.Message);
            }
            finally
            {
                _mainViewModel.HideLoading();
            }
        }

        private void VerifyCodeAsync()
        {
            try
            {
                _mainViewModel.ShowLoading();

                if (_emailVerificationService.VerifyCode(_email, VerificationCode, _verificationPurpose, _sessionId))
                {
                    var handler = VerificationSuccess;

                    handler?.Invoke();
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
            if (!IsResendEnabled)
                return;

            // Clear any old verification state when resending
            _emailVerificationService.InvalidateCode(_email, _verificationPurpose);
            _sessionId = null;

            await SendInitialCodeAsync();
        }

        private void CancelVerification()
        {
            if (!string.IsNullOrEmpty(_email))
            {
                _emailVerificationService.InvalidateCode(_email, _verificationPurpose);
            }

            ClearVerificationHandlers();
            _navigationService.GoBack();
        }

        private void StartResendCooldown(int seconds)
        {
            IsResendEnabled = false;
            IsResendCooldownActive = true;
            _resendCooldownEnd = DateTime.UtcNow.AddSeconds(seconds);

            if (_resendTimer != null && _resendTimer.IsEnabled)
            {
                _resendTimer.Stop();
            }

            _resendTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500)
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
                    CountdownText = $"Повторная отправка доступна через {(int)Math.Ceiling(timeLeft.TotalSeconds)} сек.";
                }
            };

            _resendTimer.Start();
        }
    }
}