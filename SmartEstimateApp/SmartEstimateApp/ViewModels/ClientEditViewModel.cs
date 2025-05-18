using Bl.Interfaces;
using Entities;
using SmartEstimateApp.Commands;
using SmartEstimateApp.Models;
using SmartEstimateApp.Mappings;
using SmartEstimateApp.Navigation.Interfaces;
using System.Windows;
using System.Windows.Input;

namespace SmartEstimateApp.ViewModels
{
    public class ClientEditViewModel : PropertyChangedBase
    {
        private readonly IClientBL _clientBL;
        private readonly INavigationService _navigationService;
        private readonly CurrentUser _currentUser;

        private Client _client;
        public Client Client
        {
            get => _client;
            set => SetProperty(ref _client, value, nameof(Client));
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public ClientEditViewModel(IClientBL clientBL, INavigationService navigationService, CurrentUser currentUser)
        {
            _clientBL = clientBL;
            _navigationService = navigationService;
            _currentUser = currentUser;

            SaveCommand = new RelayCommand(async _ => await SaveAsync());
            CancelCommand = new RelayCommand(_ => _navigationService.GoBack());
        }

        public void LoadClient(Client client)
        {
            // Клонируем, чтобы изменения не применялись к коллекции до сохранения
            Client = client != null ? new Client
            {
                Id = client.Id,
                Name = client.Name,
                Email = client.Email,
                Phone = client.Phone,
                Address = client.Address,
                Status = client.Status,
                CreatedAt = client.CreatedAt,
                User = client.User ?? Mapper.ToEntity(_currentUser.User)
            } : new Client { User = Mapper.ToEntity(_currentUser.User) };
        }

        private async Task SaveAsync()
        {
            try
            {
                if (Client == null)
                    return;

                if (Client.User == null)
                    Client.User = Mapper.ToEntity(_currentUser.User);

                await _clientBL.AddOrUpdateAsync(Client);

                MessageBox.Show("Клиент успешно сохранён!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                _navigationService.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения клиента: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public void SetClient(Client client)
        {
            // Клонируем, чтобы изменения не применялись к коллекции до сохранения
            Client = client != null ? new Client
            {
                Id = client.Id,
                Name = client.Name,
                Email = client.Email,
                Phone = client.Phone,
                Address = client.Address,
                Status = client.Status,
                CreatedAt = client.CreatedAt,
                User = client.User
            } : new Client();
        }
    }
}