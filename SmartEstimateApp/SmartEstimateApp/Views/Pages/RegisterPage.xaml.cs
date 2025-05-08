using Bl;
using Bl.Interfaces;
using SmartEstimateApp.Manager;
using SmartEstimateApp.Models;
using SmartEstimateApp.Navigation;
using SmartEstimateApp.ViewModels;
using SmartEstimateApp.Views.Windows;
using System.Windows;
using System.Windows.Controls;

namespace SmartEstimateApp.Views.Pages
{
    public partial class RegisterPage : Page
    {
        private readonly RegisterViewModel _viewModel;

        public RegisterPage(IUserBL userBL, INavigationService navigationService, CurrentUser currentUser, MainWindow mainWindow, CredentialsManager credentialsManager, MainWindowViewModel mainWindowViewModel, IServiceProvider serviceProvider,
        EmailVerificationServiceBL emailVerificationService)
        {
            InitializeComponent();
            _viewModel = new RegisterViewModel(userBL, navigationService, currentUser, mainWindow, credentialsManager, mainWindowViewModel, emailVerificationService, serviceProvider);
            DataContext = _viewModel;
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox passwordBox)
            {
                _viewModel.Password = passwordBox.Password;
            }
        }

        private void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox passwordBox)
            {
                _viewModel.ConfirmPassword = passwordBox.Password;
            }
        }
    }
}