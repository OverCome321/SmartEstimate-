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
    public partial class LoginPage : Page
    {
        private readonly LoginViewModel _viewModel;

        public LoginPage(IUserBL userBL, INavigationService navigationService, CurrentUser currentUser, MainWindow mainWindow, CredentialsManager credentialsManager, MainWindowViewModel mainWindowViewModel, EmailVerificationServiceBL emailVerificationServiceBL, IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _viewModel = new LoginViewModel(userBL, navigationService, currentUser, mainWindow, credentialsManager, mainWindowViewModel, emailVerificationServiceBL, serviceProvider);
            DataContext = _viewModel;
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox passwordBox)
            {
                _viewModel.Password = passwordBox.Password;
            }
        }
    }
}