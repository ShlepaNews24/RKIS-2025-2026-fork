using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TodoApp.Data;
using TodoApp.Desktop.Services;
using TodoApp.Models;

namespace TodoApp.Desktop.ViewModels
{
    public partial class TodoListViewModel : ObservableObject
    {
        private readonly Guid _profileId;
        private readonly TodoRepository _todoRepo = new();
        private readonly ProfileRepository _profileRepo = new();
        private readonly IDialogService _dialogService = new DialogService();

        public ObservableCollection<TodoItem> Tasks { get; } = new();

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsTaskSelected))]
        [NotifyCanExecuteChangedFor(nameof(EditTaskCommand))]
        [NotifyCanExecuteChangedFor(nameof(DeleteTaskCommand))]
        private TodoItem? selectedTask;

        [ObservableProperty]
        private string searchText = "";

        [ObservableProperty]
        private TodoStatus? filterStatus = null;

        [ObservableProperty]
        private string sortBy = "date";

        [ObservableProperty]
        private bool sortDescending;

        [ObservableProperty]
        private Profile? currentProfile;

        public bool IsTaskSelected => SelectedTask != null;

        private INavigationService? Navigation =>
            (App.Current.MainWindow.DataContext as MainViewModel)?.Navigation;

        public TodoListViewModel(Guid profileId)
        {
            _profileId = profileId;
            _ = LoadAsync().ContinueWith(t =>
            {
                if (t.Exception != null)
                {
                    var ex = t.Exception.InnerException ?? t.Exception;
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    });
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        // Явное уведомление об изменении IsTaskSelected при смене SelectedTask
        partial void OnSelectedTaskChanged(TodoItem? value)
        {
            OnPropertyChanged(nameof(IsTaskSelected));
        }

        private async Task LoadAsync()
        {
            try
            {
                CurrentProfile = await _profileRepo.GetByIdAsync(_profileId);
                if (CurrentProfile == null)
                {
                    throw new Exception("Профиль не найден в базе данных.");
                }

                var items = await _todoRepo.GetAllForProfileAsync(_profileId);
                Tasks.Clear();
                foreach (var item in items) Tasks.Add(item);

                ApplyFilters();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке задач: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        partial void OnSearchTextChanged(string value) => ApplyFilters();
        partial void OnFilterStatusChanged(TodoStatus? value) => ApplyFilters();
        partial void OnSortByChanged(string value) => ApplyFilters();
        partial void OnSortDescendingChanged(bool value) => ApplyFilters();

        private void ApplyFilters()
        {
            var allItems = _todoRepo.GetAllForProfileAsync(_profileId).Result;
            IEnumerable<TodoItem> query = allItems;

            if (!string.IsNullOrWhiteSpace(SearchText))
                query = query.Where(t => t.Text.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

            if (FilterStatus.HasValue)
                query = query.Where(t => t.Status == FilterStatus.Value);

            query = SortBy switch
            {
                "text" => SortDescending
                    ? query.OrderByDescending(t => t.Text)
                    : query.OrderBy(t => t.Text),
                _ => SortDescending
                    ? query.OrderByDescending(t => t.LastUpdate)
                    : query.OrderBy(t => t.LastUpdate)
            };

            var filtered = query.ToList();
            Tasks.Clear();
            foreach (var item in filtered) Tasks.Add(item);
        }

        [RelayCommand]
        private async Task AddTaskAsync()
        {
            var vm = new TaskEditViewModel(_profileId, isNew: true);
            if (_dialogService.ShowDialog(vm) == true)
            {
                await LoadAsync();
            }
        }

        [RelayCommand(CanExecute = nameof(IsTaskSelected))]
        private async Task EditTaskAsync()
        {
            if (SelectedTask == null) return;
            var vm = new TaskEditViewModel(_profileId, isNew: false, itemId: SelectedTask.Id);
            _dialogService.ShowDialog(vm);
            await LoadAsync();
        }

        [RelayCommand(CanExecute = nameof(IsTaskSelected))]
        private async Task DeleteTaskAsync()
        {
            if (SelectedTask == null) return;
            await _todoRepo.DeleteAsync(SelectedTask.Id);
            Tasks.Remove(SelectedTask);
        }

        [RelayCommand]
        private void Logout()
        {
            Navigation?.NavigateTo<LoginViewModel>();
        }
    }
}