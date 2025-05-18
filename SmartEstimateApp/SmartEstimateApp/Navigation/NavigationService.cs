using Microsoft.Extensions.DependencyInjection;
using SmartEstimateApp.Navigation.Interfaces;
using System.Windows.Controls;
using System.Windows.Navigation;

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

            if (_frame != null)
            {
                _frame.Navigated += Frame_Navigated;
            }
        }

        private void Frame_Navigated(object sender, NavigationEventArgs e)
        {
            if (e.ExtraData != null && e.Content is Page page)
            {
                if (page is IParameterReceiver parameterReceiver)
                {
                    parameterReceiver.SetParameter(e.ExtraData);
                }
                else
                {
                    var method = page.GetType().GetMethod("SetParameter");
                    if (method != null)
                    {
                        method.Invoke(page, new[] { e.ExtraData });
                    }
                }
            }
        }

        public void NavigateTo<TPage>() where TPage : Page
        {
            if (_frame == null)
                throw new InvalidOperationException("Navigation service not initialized. Call Initialize first.");

            var page = _serviceProvider.GetService<TPage>();
            _frame.Navigate(page);
        }

        public void NavigateTo<TPage>(object parameter) where TPage : Page
        {
            if (_frame == null)
                throw new InvalidOperationException("Navigation service not initialized. Call Initialize first.");

            var page = _serviceProvider.GetService<TPage>();
            _frame.Navigate(page, parameter);
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