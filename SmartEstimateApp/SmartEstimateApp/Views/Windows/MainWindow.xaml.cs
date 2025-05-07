using Microsoft.Extensions.DependencyInjection;
using SmartEstimateApp.Navigation;
using SmartEstimateApp.ViewModels;
using SmartEstimateApp.Views.Pages;
using System.Windows;
using System.Windows.Controls;

namespace SmartEstimateApp.Views.Windows
{
    public partial class MainWindow : Window
    {
        private readonly IServiceProvider _serviceProvider;
        public Frame MainFrame { get; private set; }
        private TextBlock _dateTimeDisplay;
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

            _dateTimeDisplay = FindName("DateTimeDisplay") as TextBlock;

            SetupDateTimeDisplay();
        }

        private void SetupDateTimeDisplay()
        {
            if (_dateTimeDisplay != null)
            {
                System.Windows.Data.Binding binding = new System.Windows.Data.Binding("CurrentDateTime")
                {
                    Source = _viewModel,
                    Mode = System.Windows.Data.BindingMode.OneWay,
                    UpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged
                };
                _dateTimeDisplay.SetBinding(TextBlock.TextProperty, binding);
            }
        }
    }
}