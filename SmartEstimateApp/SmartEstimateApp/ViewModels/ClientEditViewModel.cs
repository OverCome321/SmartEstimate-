using Bl.Interfaces;
using Entities;
using SmartEstimateApp.Commands;
using SmartEstimateApp.Mappings;
using SmartEstimateApp.Models;
using SmartEstimateApp.Navigation.Interfaces;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using UI.Helpers;

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

        private ObservableCollection<StatusOption> _statusOptions;
        public ObservableCollection<StatusOption> StatusOptions
        {
            get => _statusOptions;
            set => SetProperty(ref _statusOptions, value, nameof(StatusOptions));
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public ClientEditViewModel(IClientBL clientBL, INavigationService navigationService, CurrentUser currentUser)
        {
            _clientBL = clientBL;
            _navigationService = navigationService;
            _currentUser = currentUser;

            SaveCommand = new RelayCommand(async _ => await SaveAsync(), _ => CanSave());
            CancelCommand = new RelayCommand(_ => _navigationService.GoBack());

            LoadStatusOptions();
        }

        private void LoadStatusOptions()
        {
            StatusOptions = new ObservableCollection<StatusOption>
            {
                new StatusOption { Id = 1, Name = "Активен" },
                new StatusOption { Id = 2, Name = "Неактивен" }
            };
        }

        public void LoadClient(Client? client = null)
        {
            if (client != null)
            {
                Client = client;
            }
            else
            {
                Client = new Client
                {
                    User = Mapper.ToEntity(_currentUser.User),
                    Status = 1
                };
            }
        }

        private bool CanSave()
        {
            return Client != null && !string.IsNullOrWhiteSpace(Client.Name) && Client.Status > 0;
        }

        private async Task SaveAsync()
        {
            if (!CanSave())
            {
                MessageBox.Show("Пожалуйста, заполните обязательные поля (*) и выберите статус.", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (Client.User == null)
                    Client.User = Mapper.ToEntity(_currentUser.User);


                if (Client.Id == 0)
                {
                    Client.CreatedAt = DateTime.UtcNow;
                    Client.UpdatedAt = DateTime.UtcNow;
                }

                var savedClientId = await _clientBL.AddOrUpdateAsync(Client);

                if (Client.Id == 0)
                {
                    Client.Id = savedClientId;
                }

                AppMessenger.SendEntityUpdateMessage<Client>(this.Client);
                _navigationService.GoBack();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения клиента: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}