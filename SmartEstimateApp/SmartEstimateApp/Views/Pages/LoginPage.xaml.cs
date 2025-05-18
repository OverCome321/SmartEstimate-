using SmartEstimateApp.Interfaces;
using SmartEstimateApp.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SmartEstimateApp.Views.Pages
{
    public partial class LoginPage : Page
    {
        private readonly LoginViewModel _viewModel;

        public LoginPage(ILoginContext ctx)
        {
            InitializeComponent();
            _viewModel = new LoginViewModel(ctx);
            DataContext = _viewModel;
        }

        private void InputField_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && _viewModel.LoginCommand.CanExecute(null))
            {
                _viewModel.LoginCommand.Execute(null);
                e.Handled = true;
            }
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox passwordBox)
            {
                _viewModel.Password = passwordBox.Password;
                var pb = (PasswordBox)sender;
                pb.Tag = string.IsNullOrEmpty(pb.Password) ? "" : "*";
            }
        }
    }
}