using Microsoft.Extensions.DependencyInjection;
using SmartEstimateApp.Navigation.Interfaces;
using System.Windows.Controls;

namespace SmartEstimateApp.Navigation
{
    public class NavigationServiceFactory : INavigationServiceFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public NavigationServiceFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public INavigationService Create(Frame frame)
        {
            var scope = _serviceProvider.CreateScope();
            var navigationService = scope.ServiceProvider.GetRequiredService<INavigationService>();
            navigationService.Initialize(frame);
            return navigationService;
        }
    }
}
