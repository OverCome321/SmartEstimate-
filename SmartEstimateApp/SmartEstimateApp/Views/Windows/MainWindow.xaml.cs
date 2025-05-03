using Microsoft.Extensions.DependencyInjection;
using SmartEstimateApp.Navigation;
using SmartEstimateApp.Views.Pages;
using System.Windows;
using System.Windows.Controls;

namespace SmartEstimateApp.Views.Windows
{
    public partial class MainWindow : Window
    {
        private readonly IServiceProvider _serviceProvider;
        public Frame MainFrame { get; private set; }

        public MainWindow(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _serviceProvider = serviceProvider;
            MainFrame = FindName("MainFrameU") as Frame;
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var navigationService = _serviceProvider.GetService<INavigationService>();
            navigationService.NavigateTo<LoginPage>();
        }
    }
}