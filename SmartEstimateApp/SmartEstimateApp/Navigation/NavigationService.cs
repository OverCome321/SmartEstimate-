using Microsoft.Extensions.DependencyInjection;
using SmartEstimateApp.Navigation.Interfaces;
using System.Windows.Controls;

namespace SmartEstimateApp.Navigation
{
    public class NavigationService : INavigationService
    {
        private readonly IServiceProvider _serviceProvider;
        private Frame _frame;

        public NavigationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void Initialize(Frame frame)
        {
            _frame = frame;
        }

        public void NavigateTo<TPage>() where TPage : Page
        {
            if (_frame == null)
                throw new InvalidOperationException("Navigation service not initialized. Call Initialize first.");

            var page = _serviceProvider.GetService<TPage>();
            _frame.Navigate(page);
        }

        public void GoBack()
        {
            if (_frame?.CanGoBack == true)
            {
                _frame.GoBack();
            }
        }
    }
}
