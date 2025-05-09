using Bl.Interfaces;
using MaterialDesignThemes.Wpf;
using SmartEstimateApp.Manager;
using SmartEstimateApp.Models;
using SmartEstimateApp.Navigation;
using SmartEstimateApp.ViewModels;
using SmartEstimateApp.Views.Windows;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SmartEstimateApp.Views.Pages
{
    public partial class LoginPage : Page
    {
        private readonly LoginViewModel _viewModel;

        public LoginPage(IUserBL userBL, INavigationService navigationService, CurrentUser currentUser, MainWindow mainWindow, CredentialsManager credentialsManager, MainWindowViewModel mainWindowViewModel, IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _viewModel = new LoginViewModel(userBL, navigationService, currentUser, mainWindow, credentialsManager, mainWindowViewModel, serviceProvider);
            DataContext = _viewModel;
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
        private bool _isPasswordVisible;

        private void TogglePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var passwordBox = FindParent<PasswordBox>(button);
            var eyeIcon = button?.FindName("EyeIcon") as PackIcon;

            if (passwordBox == null || eyeIcon == null) return;

            _isPasswordVisible = !_isPasswordVisible;

            if (_isPasswordVisible)
            {
                // Показать пароль
                var password = passwordBox.Password;
                passwordBox.Clear();
                passwordBox.Tag = password; // Сохраняем пароль в Tag
                passwordBox.SetValue(PasswordBox.PasswordCharProperty, '\0'); // Убираем маскировку
                eyeIcon.Kind = PackIconKind.EyeOffOutline; // Меняем иконку
            }
            else
            {
                // Скрыть пароль
                passwordBox.Password = passwordBox.Tag?.ToString() ?? string.Empty;
                passwordBox.SetValue(PasswordBox.PasswordCharProperty, '•'); // Возвращаем маскировку
                eyeIcon.Kind = PackIconKind.EyeOutline; // Меняем иконку
            }
        }

        // Вспомогательный метод для поиска родительского элемента
        private static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            while (child != null)
            {
                child = VisualTreeHelper.GetParent(child);
                if (child is T parent)
                    return parent;
            }
            return null;
        }
    }
}