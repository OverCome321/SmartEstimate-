using SmartEstimateApp.Interfaces;
using SmartEstimateApp.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SmartEstimateApp.Views.Pages
{
    public partial class RegisterPage : Page
    {
        private readonly RegisterViewModel _viewModel;

        public RegisterPage(IRegisterContext ctx)
        {
            InitializeComponent();
            _viewModel = new RegisterViewModel(ctx);
            DataContext = _viewModel;
        }

        private void InputField_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && _viewModel.RegisterCommand.CanExecute(null))
            {
                _viewModel.RegisterCommand.Execute(null);
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

        private void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox passwordBox)
            {
                _viewModel.ConfirmPassword = passwordBox.Password;
                var pb = (PasswordBox)sender;
                pb.Tag = string.IsNullOrEmpty(pb.Password) ? "" : "*";
            }
        }
    }
}