using System.Windows.Controls;

namespace SmartEstimateApp.Navigation.Interfaces
{
    public interface INavigationServiceFactory
    {
        INavigationService Create(Frame frame);
    }
}
