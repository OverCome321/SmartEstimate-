using Microsoft.Extensions.DependencyInjection;
using SmartEstimateApp.Navigation;
using SmartEstimateApp.ViewModels;
using SmartEstimateApp.Views.Pages;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SmartEstimateApp.Views.Windows
{
    public partial class MainWindow : Window
    {
        private readonly IServiceProvider _serviceProvider;

        public Frame MainFrame { get; private set; }

        private MainWindowViewModel _viewModel;

        public MainWindow(IServiceProvider serviceProvider)
        {
            InitializeComponent();

            _serviceProvider = serviceProvider;

            MainFrame = FindName("MainFrameU") as Frame;

            _viewModel = _serviceProvider.GetService<MainWindowViewModel>();

            DataContext = _viewModel;

            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var navigationService = _serviceProvider.GetService<INavigationService>();

            navigationService.NavigateTo<LoginPage>();
        }
        // Add these methods to your MainWindow.xaml.cs file

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                MaximizeButton_Click(sender, e);
            }
            else
            {
                this.DragMove();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
            }
            else
            {
                this.WindowState = WindowState.Maximized;
            }
        }
    }
}