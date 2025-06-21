using Bl.Interfaces;
using Common.Search;
using SmartEstimateApp.Commands;
using SmartEstimateApp.Mappings;
using SmartEstimateApp.Models;
using SmartEstimateApp.Navigation.Interfaces;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace SmartEstimateApp.ViewModels
{
    public class ProjectEditViewModel : PropertyChangedBase
    {
        private readonly IProjectBL _projectBL;
        private readonly IClientBL _clientBL;
        private readonly INavigationService _navigationService;
        private readonly CurrentUser _currentUser;

        private Project _project;
        public Project Project
        {
            get => _project;
            set => SetProperty(ref _project, value, nameof(Project));
        }

        private Entities.Client _selectedClient;
        public Entities.Client SelectedClient
        {
            get => _selectedClient;
            set
            {
                if (SetProperty(ref _selectedClient, value, nameof(SelectedClient)))
                {
                    if (Project != null && value != null)
                    {
                        Project.ClientId = value.Id;
                        Project.ClientName = value.Name;
                    }
                }
            }
        }

        public ObservableCollection<Entities.Client> _clients;
        public ObservableCollection<Entities.Client> Clients
        {
            get => _clients;
            set => SetProperty(ref _clients, value, nameof(Clients));
        }

        public ObservableCollection<StatusOption> _projectStatusOptions;
        public ObservableCollection<StatusOption> ProjectStatusOptions
        {
            get => _projectStatusOptions;
            set => SetProperty(ref _projectStatusOptions, value, nameof(ProjectStatusOptions));
        }


        public ObservableCollection<string> _currencies;
        public ObservableCollection<string> Currencies
        {
            get => _currencies;
            set => SetProperty(ref _currencies, value, nameof(Currencies));
        }

        public bool HasEstimates => Project?.Estimates.Any() ?? false;
        public bool NoEstimates => !HasEstimates;

        public ICommand SaveProjectCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand AddEstimateCommand { get; }
        public ICommand RemoveEstimateCommand { get; }
        public ICommand AddEstimateItemCommand { get; }
        public ICommand RemoveEstimateItemCommand { get; }

        public ProjectEditViewModel(
            IProjectBL projectBL,
            IClientBL clientBL,
            INavigationService navigationService, CurrentUser currentUser)
        {
            _clientBL = clientBL;
            _navigationService = navigationService;
            _currentUser = currentUser;

            SaveProjectCommand = new RelayCommand(async _ => await OnSaveProject(), _ => CanSave());
            CancelCommand = new RelayCommand(_ => OnCancel());
            AddEstimateCommand = new RelayCommand(_ => OnAddEstimate());
            RemoveEstimateCommand = new RelayCommand(param => OnRemoveEstimate(param as Estimate));
            AddEstimateItemCommand = new RelayCommand(param => OnAddEstimateItem(param as Estimate));
            RemoveEstimateItemCommand = new RelayCommand(param => OnRemoveEstimateItem(param as EstimateItem));

            _ = LoadInitialData();
        }

        private async Task LoadInitialData()
        {
            await LoadClientsAsync();
            LoadStatuses();
            LoadCurrencies();

            if (Project != null)
            {
                SelectedClient = Clients.FirstOrDefault(c => c.Id == Project.ClientId);

                Project.Estimates.CollectionChanged += (s, e) =>
                {
                    OnPropertyChanged(nameof(HasEstimates));
                    OnPropertyChanged(nameof(NoEstimates));
                };
            }
            OnPropertyChanged(nameof(HasEstimates));
            OnPropertyChanged(nameof(NoEstimates));
        }

        /// <summary>
        /// Загружает полный список клиентов для ComboBox.
        /// </summary>
        private async Task LoadClientsAsync()
        {
            var searchParams = new ClientSearchParams { ObjectsCount = int.MaxValue, UserId = _currentUser.User.Id };
            var result = await _clientBL.GetAsync(searchParams);

            Clients = new ObservableCollection<Entities.Client>(result.Objects);
        }


        public void LoadProject(Project project)
        {
            Project = project;
        }

        /// <summary>
        /// Заполняет коллекцию опций статуса для ComboBox.
        /// </summary>
        private void LoadStatuses()
        {
            ProjectStatusOptions = new ObservableCollection<StatusOption>()
            {
                new StatusOption { Id = 1, Name = "Активен" },
                new StatusOption { Id = 2, Name = "Неактивен" }
            };
        }
        private void LoadCurrencies()
        {
            Currencies = new ObservableCollection<string>()
            {
                "RUB", "USD", "EUR"
            };
        }

        private bool CanSave()
        {
            return Project != null && !string.IsNullOrWhiteSpace(Project.Name) && Project.ClientId > 0;
        }

        private async Task OnSaveProject()
        {
            if (!CanSave())
            {
                MessageBox.Show("Пожалуйста, укажите название проекта и выберите клиента.", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var entityToSave = Mapper.ToEntity(Project);

            await _projectBL.AddOrUpdateAsync(entityToSave);

            _navigationService.GoBack();
        }

        private void OnCancel()
        {
            _navigationService.GoBack();
        }

        private void OnAddEstimate()
        {
            Project?.Estimates.Add(new Estimate
            {
                Number = (Project.Estimates.Count + 1).ToString(),
                Status = "Черновик",
                CreatedAt = System.DateTime.Now,
                ValidUntil = System.DateTime.Now.AddDays(30),
                Currency = Currencies.FirstOrDefault() ?? "RUB"
            });
        }

        private void OnRemoveEstimate(Estimate estimateToRemove)
        {
            Project?.Estimates.Remove(estimateToRemove);
        }

        private void OnAddEstimateItem(Estimate parentEstimate)
        {
            parentEstimate?.Items.Add(new EstimateItem { Description = "Новая позиция", Quantity = 1, UnitPrice = 0 });
        }

        private void OnRemoveEstimateItem(EstimateItem itemToRemove)
        {
            if (itemToRemove == null) return;
            var parentEstimate = Project.Estimates.FirstOrDefault(e => e.Items.Contains(itemToRemove));
            parentEstimate?.Items.Remove(itemToRemove);
        }
    }
}