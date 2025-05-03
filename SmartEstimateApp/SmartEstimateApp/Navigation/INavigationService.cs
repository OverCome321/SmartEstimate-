
using System.Windows.Controls;

namespace SmartEstimateApp.Navigation
{
    public interface INavigationService
    {
        void NavigateTo<TPage>() where TPage : Page;
    }
}
