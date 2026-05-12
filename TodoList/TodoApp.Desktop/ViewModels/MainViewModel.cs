using CommunityToolkit.Mvvm.ComponentModel;
using TodoApp.Desktop.Services;

namespace TodoApp.Desktop.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private INavigationService? _navigation;

        [ObservableProperty]
        private ObservableObject? currentViewModel;

        public INavigationService? Navigation => _navigation;

        public void Initialize(INavigationService navigation)
        {
            _navigation = navigation;
        }

        public void NavigateToTasks(Guid profileId)
        {
            _navigation?.NavigateTo(new TodoListViewModel(profileId));
        }
    }
}