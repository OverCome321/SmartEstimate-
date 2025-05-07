using System;
using System.Windows;
using System.Windows.Input;
using SmartEstimateApp.Views.Pages;

namespace SmartEstimateApp.Views.Windows
{
    /// <summary>
    /// Логика взаимодействия для HomeWindow.xaml
    /// </summary>
    public partial class HomeWindow : Window
    {
        public HomeWindow()
        {
            InitializeComponent();
        }

        #region Обработчики событий окна

        /// <summary>
        /// Обработчик нажатия кнопки закрытия окна
        /// </summary>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        /// <summary>
        /// Обработчик нажатия кнопки максимизации/восстановления окна
        /// </summary>
        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
            }
            else
            {
                WindowState = WindowState.Maximized;
            }
        }

        /// <summary>
        /// Обработчик нажатия кнопки минимизации окна
        /// </summary>
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        /// <summary>
        /// Обработчик события для перетаскивания окна при удержании ЛКМ
        /// </summary>
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        #endregion

        #region Навигация между страницами

        /// <summary>
        /// Переход на страницу Dashboard
        /// </summary>
        private void NavigateToDashboard_Click(object sender, RoutedEventArgs e)
        {
            // Здесь можно добавить логику навигации
            // Например: MainFrame.Navigate(new DashboardPage());
            MessageBox.Show("Переход на страницу Dashboard", "Навигация", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Переход на страницу Clients
        /// </summary>
        private void NavigateToClients_Click(object sender, RoutedEventArgs e)
        {
            // Здесь можно добавить логику навигации
            // Например: MainFrame.Navigate(new ClientsPage());
            MessageBox.Show("Переход на страницу Clients", "Навигация", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Переход на страницу Projects
        /// </summary>
        private void NavigateToProjects_Click(object sender, RoutedEventArgs e)
        {
            // Здесь можно добавить логику навигации
            // Например: MainFrame.Navigate(new ProjectsPage());
            MessageBox.Show("Переход на страницу Projects", "Навигация", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Переход на страницу Analytics
        /// </summary>
        private void NavigateToAnalytics_Click(object sender, RoutedEventArgs e)
        {
            // Здесь можно добавить логику навигации
            // Например: MainFrame.Navigate(new AnalyticsPage());
            MessageBox.Show("Переход на страницу Analytics", "Навигация", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Переход на страницу Settings
        /// </summary>
        private void NavigateToSettings_Click(object sender, RoutedEventArgs e)
        {
            // Здесь можно добавить логику навигации
            // Например: MainFrame.Navigate(new SettingsPage());
            MessageBox.Show("Переход на страницу Settings", "Навигация", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #endregion
    }
}