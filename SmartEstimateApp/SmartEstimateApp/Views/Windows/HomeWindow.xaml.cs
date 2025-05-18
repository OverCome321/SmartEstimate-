using Microsoft.Extensions.DependencyInjection;
using SmartEstimateApp.Manager;
using SmartEstimateApp.Models;
using SmartEstimateApp.Navigation.Interfaces;
using SmartEstimateApp.ViewModels;
using SmartEstimateApp.Views.Pages;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SmartEstimateApp.Views.Windows
{
    public partial class HomeWindow : Window
    {
        private readonly IServiceProvider _serviceProvider;
        public Frame MainFrame { get; private set; }
        private WindowResizeManager _resizeManager;
        private INavigationService _navigationService;


        public HomeWindow(IServiceProvider serviceProvider, HomeWindowViewModel homeWindowViewModel)
        {
            InitializeComponent();
            MainFrame = FindName("MainFrameHome") as Frame;
            _serviceProvider = serviceProvider;
            _resizeManager = new WindowResizeManager(this);
            Loaded += MainWindow_Loaded;
            this.DataContext = homeWindowViewModel;
        }

        private void UpdateNavigationButtons(NavigationPage page)
        {
            // Передаем this в качестве параметра
            var buttons = FindVisualChildren<Button>(this).Where(b => b.Tag != null);
            foreach (Button button in buttons)
            {
                if (button.Tag?.ToString() == page.ToString())
                {
                    button.Style = FindResource("NavButtonActive") as Style;
                }
                else
                {
                    button.Style = FindResource("NavButton") as Style;
                }
            }
        }

        // Вспомогательный метод для поиска всех элементов определенного типа
        private IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
            yield break;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var navigationFactory = _serviceProvider.GetService<INavigationServiceFactory>();
            _navigationService = navigationFactory.Create(MainFrame);
            _navigationService.NavigateTo<DashboardPage>();

            var screen = System.Windows.SystemParameters.WorkArea;
            MaxHeight = screen.Height + 10;
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _resizeManager.HandleMouseLeftButtonDown(sender, e);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            _resizeManager.Close();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            _resizeManager.Minimize();
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            _resizeManager.ToggleMaximize();
        }

        /// <summary>
        /// Переход на страницу Dashboard
        /// </summary>
        private void NavigateToDashboard_Click(object sender, RoutedEventArgs e)
        {
            UpdateNavigationButtons(NavigationPage.Dashboard);
            _navigationService.NavigateTo<DashboardPage>();
        }

        /// <summary>
        /// Переход на страницу Settings
        /// </summary>
        private void NavigateToSettings_Click(object sender, RoutedEventArgs e)
        {
            UpdateNavigationButtons(NavigationPage.Settings);
            _navigationService.NavigateTo<SettingsPage>();
        }

        /// <summary>
        /// Переход на страницу Projects
        /// </summary>
        private void NavigateToProjects_Click(object sender, RoutedEventArgs e)
        {
            UpdateNavigationButtons(NavigationPage.Projects);
            _navigationService.NavigateTo<ProjectsPage>();
        }

        /// <summary>
        /// Переход на страницу Clients
        /// </summary>
        private void NavigateToClients_Click(object sender, RoutedEventArgs e)
        {
            UpdateNavigationButtons(NavigationPage.Clients);
            _navigationService.NavigateTo<ClientsPage>();
        }

        /// <summary>
        /// Переход на страницу Analytics
        /// </summary>
        private void NavigateToAnalytics_Click(object sender, RoutedEventArgs e)
        {
            UpdateNavigationButtons(NavigationPage.Reports);
            _navigationService.NavigateTo<AnalyticsPage>();
        }
        /// <summary>
        /// Переход на страницу Help
        /// </summary>
        private void NavigateToHelp_Click(object sender, RoutedEventArgs e)
        {
            UpdateNavigationButtons(NavigationPage.Help);
            _navigationService.NavigateTo<AnalyticsPage>();
        }
        /// <summary>
        /// Переход на страницу Statistic
        /// </summary>
        private void NavigateToStatistics_Click(object sender, RoutedEventArgs e)
        {
            UpdateNavigationButtons(NavigationPage.Statistics);
            _navigationService.NavigateTo<AnalyticsPage>();
        }
        /// <summary>
        /// Переход на страницу Statistic
        /// </summary>
        private void NavigateToDocuments_Click(object sender, RoutedEventArgs e)
        {
            UpdateNavigationButtons(NavigationPage.Documents);
            _navigationService.NavigateTo<AnalyticsPage>();
        }
    }
}