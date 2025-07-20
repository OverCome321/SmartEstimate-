using Bl.Interfaces;
using SmartEstimateApp.Commands;
using SmartEstimateApp.Mappings;
using SmartEstimateApp.Models;
using SmartEstimateApp.Navigation.Interfaces;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using UI.Helpers;
using Client = SmartEstimateApp.Models.Client;

namespace SmartEstimateApp.ViewModels
{
    public class ClientEditViewModel : PropertyChangedBase
    {
        private readonly IClientBL _clientBL;
        private readonly INavigationService _navigationService;
        private readonly CurrentUser _currentUser;
        private readonly HomeWindowViewModel _homeWindowViewModel;

        private Client _originalClient;

        private Client _clientForEdit;
        public Client ClientForEdit
        {
            get => _clientForEdit;
            set => SetProperty(ref _clientForEdit, value, nameof(ClientForEdit));
        }

        private ObservableCollection<StatusOption> _statusOptions;
        public ObservableCollection<StatusOption> StatusOptions
        {
            get => _statusOptions;
            set => SetProperty(ref _statusOptions, value, nameof(StatusOptions));
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public ClientEditViewModel(IClientBL clientBL, INavigationService navigationService, CurrentUser currentUser, HomeWindowViewModel homeWindowViewModel)
        {
            _clientBL = clientBL;
            _navigationService = navigationService;
            _currentUser = currentUser;
            _homeWindowViewModel = homeWindowViewModel;

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
                _originalClient = client;
                ClientForEdit = _originalClient.Clone();
            }
            else
            {
                _originalClient = null;
                ClientForEdit = new Client
                {
                    User = _currentUser.User,
                    Status = 1
                };
            }
        }

        private bool CanSave()
        {
            return ClientForEdit != null && !string.IsNullOrWhiteSpace(ClientForEdit.Name) && ClientForEdit.Status > 0;
        }

        private async Task SaveAsync()
        {
            if (!CanSave())
            {
                MessageBox.Show("Пожалуйста, заполните обязательные поля (*) и выберите статус.", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _homeWindowViewModel.ShowLoading();
            try
            {
                if (ClientForEdit.User == null)
                    ClientForEdit.User = _currentUser.User;

                if (ClientForEdit.Id == 0)
                {
                    ClientForEdit.CreatedAt = DateTime.UtcNow;
                }
                ClientForEdit.UpdatedAt = DateTime.UtcNow;

                var savedClientId = await _clientBL.AddOrUpdateAsync(Mapper.ToEntity(ClientForEdit));

                if (ClientForEdit.Id == 0)
                {
                    ClientForEdit.Id = savedClientId;
                }
                _homeWindowViewModel.ShowSuccess($"Клиент успешно {(ClientForEdit.Id == 0 ? "добавлен" : "изменен")}!");

                AppMessenger.SendEntityUpdateMessage<Client>(this.ClientForEdit);

                _navigationService.GoBack();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения клиента: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                _homeWindowViewModel.ShowError($"Ошибка при {(ClientForEdit.Id == 0 ? "добавлении" : "изменении")} клиента!");
            }
            finally
            {
                _homeWindowViewModel.HideLoading();
            }
        }
    }
}