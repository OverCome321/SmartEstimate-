using Bl.Interfaces;
using Common.Search;
using SmartEstimateApp.Commands;
using SmartEstimateApp.Mappings;
using SmartEstimateApp.Models;
using SmartEstimateApp.Navigation.Interfaces;
using System.Windows;
using System.Windows.Input;

namespace SmartEstimateApp.ViewModels
{
    public class ProjectsViewModel : BasePaginatedViewModel<Project>
    {
        private readonly IProjectBL _projectBL;
        private readonly CurrentUser _currentUser;
        private readonly HomeWindowViewModel _homeWindowViewModel;
        private readonly INavigationService _navigationService;

        public ICommand ShowProjectDetailsCommand { get; }
        public ICommand DeleteProjectCommand { get; }
        public ICommand AddProjectCommand { get; }

        public ProjectsViewModel(
            IProjectBL projectBL,
            CurrentUser currentUser,
            HomeWindowViewModel homeWindowViewModel,
            INavigationService navigationService)
        {
            _projectBL = projectBL;
            _currentUser = currentUser;
            _homeWindowViewModel = homeWindowViewModel;
            _navigationService = navigationService;

            ShowProjectDetailsCommand = new RelayCommand(obj => OnDetails(obj as Project), obj => obj != null);
            DeleteProjectCommand = new RelayCommand(obj => OnDelete(obj as Project), obj => obj != null);
            AddProjectCommand = new RelayCommand(obj => OnAddProject());

            Task.Run(LoadItemsAsync);
        }

        protected override async Task LoadItemsAsync()
        {
            try
            {
                var searchParams = new ProjectSearchParams
                {
                    UserId = _currentUser.User.Id,
                    StartIndex = (CurrentPage - 1) * PageSize,
                    ObjectsCount = PageSize
                };

                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    searchParams.Name = SearchText;
                }
                else _homeWindowViewModel.ShowLoading();

                var result = await _projectBL.GetAsync(searchParams, includeRelated: false);

                App.Current.Dispatcher.Invoke(() =>
                {
                    Items.Clear();
                    foreach (var p in result.Objects)
                        Items.Add(Mapper.ToModel(p));

                    TotalCount = result.Total;
                    TotalPages = (int)System.Math.Ceiling(TotalCount / (double)PageSize);
                    if (TotalPages == 0) TotalPages = 1;

                    UpdatePageNumbers();
                    OnPropertyChanged(nameof(HasResults));
                    OnPropertyChanged(nameof(ResultsInfo));
                    OnPropertyChanged(nameof(PageInfo));
                });
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading projects: {ex.Message}");
            }
            finally
            {
                _homeWindowViewModel.HideLoading();
            }
        }

        private void OnDetails(Project project)
        {
            if (project == null)
                return;
            //_navigationService.NavigateTo<ProjectsEditPage>(project);
        }

        private async void OnDelete(Project project)
        {
            if (project == null) return;

            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить проект \"{project.Name}\"?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            var deleted = await _projectBL.DeleteAsync(project.Id);
            if (deleted)
            {
                if (Items.Count == 1 && CurrentPage > 1)
                {
                    CurrentPage--;
                }
                else
                {
                    await Task.Run(LoadItemsAsync);
                }
            }
        }

        private void OnAddProject()
        {
            //_navigationService.NavigateTo<ProjectsEditPage>();
        }
    }
}
