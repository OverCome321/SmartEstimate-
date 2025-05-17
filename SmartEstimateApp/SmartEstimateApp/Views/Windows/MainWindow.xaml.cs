using Microsoft.Extensions.DependencyInjection;
using SmartEstimateApp.Manager;
using SmartEstimateApp.Navigation.Interfaces;
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
        private WindowResizeManager _resizeManager;

        public MainWindow(IServiceProvider serviceProvider, MainWindowViewModel mainWindowViewModel)
        {
            InitializeComponent();

            _serviceProvider = serviceProvider;
            MainFrame = FindName("MainFrameU") as Frame;
            DataContext = mainWindowViewModel;

            _resizeManager = new WindowResizeManager(this);

            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var navigationFactory = _serviceProvider.GetService<INavigationServiceFactory>();
            var navigationService = navigationFactory.Create(MainFrame);
            navigationService.NavigateTo<LoginPage>();

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
    }
}