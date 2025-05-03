using Bl.Interfaces;
using SmartEstimateApp.Models;
using SmartEstimateApp.Navigation;
using SmartEstimateApp.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace SmartEstimateApp.Views.Pages
{
    public partial class LoginPage : Page
    {
        private readonly LoginViewModel _viewModel;

        public LoginPage(IUserBL userBL, INavigationService navigationService, CurrentUser currentUser)
        {
            InitializeComponent();
            _viewModel = new LoginViewModel(userBL, navigationService, currentUser);
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