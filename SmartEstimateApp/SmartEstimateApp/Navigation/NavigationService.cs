using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;

namespace SmartEstimateApp.Navigation
{
    public class NavigationService : INavigationService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Frame _frame;

        public NavigationService(IServiceProvider serviceProvider, Frame frame)
        {
            _serviceProvider = serviceProvider;
            _frame = frame;
        }

        public void NavigateTo<TPage>() where TPage : Page
        {
            var page = _serviceProvider.GetService<TPage>();
            _frame.Navigate(page);
        }

        public void GoBack()
        {
            if (_frame.CanGoBack)
            {
                _frame.GoBack();
            }
        }
    }
}
