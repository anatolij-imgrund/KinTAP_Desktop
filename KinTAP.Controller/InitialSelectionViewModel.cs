/// !!!   !!!   !!!   !!!
/// Nicht für Projektarbeit... Habe angefangen Android-version zu basteln...
/// !!!   !!!   !!!   !!!

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KinTAP.Model;
using KinTAP.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace KinTAP.Controller
{
    public partial class InitialSelectionViewModel : ObservableObject
    {
        private readonly DataService _dataService;

        public ObservableCollection<Child> Children { get; }

        [ObservableProperty]
        private Child? _selectedChildForLogin;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(LoginAsChildCommand))]
        private bool _isChildSelectionVisible = false;

        public IAsyncRelayCommand ParentModeCommand { get; }
        public IAsyncRelayCommand ChildModeCommand { get; }
        public IAsyncRelayCommand LoginAsChildCommand { get; }

        public InitialSelectionViewModel(DataService dataService)
        {
            _dataService = dataService;
            Children = new ObservableCollection<Child>();

            if (_dataService.Children.Any())
            {
                foreach (var child in _dataService.Children)
                {
                    Children.Add(child);
                }
            }
            else
            {
                Debug.WriteLine("InitialSelectionViewModel: DataService.Children ist leer beim Erstellen.");
            }

            ParentModeCommand = new AsyncRelayCommand(GoToParentModeAsync);
            ChildModeCommand = new AsyncRelayCommand(PrepareChildLoginAsync);
            LoginAsChildCommand = new AsyncRelayCommand(LoginAsSelectedChildAsync, CanLoginAsSelectedChild);

            PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(SelectedChildForLogin) || e.PropertyName == nameof(IsChildSelectionVisible))
                {
                    LoginAsChildCommand.NotifyCanExecuteChanged();
                }
            };
        }

        private Task GoToParentModeAsync()
        {
            Debug.WriteLine("Parent Mode ist ausgewählt. TODO: ParentPasswordPromptPage.");
            return Task.CompletedTask;
        }

        private Task PrepareChildLoginAsync()
        {
            Debug.WriteLine("Child Mode ist ausgewählt. TODO: Child-liste anzeigen.");
            if (Children.Any())
            {
                IsChildSelectionVisible = true;
            }
            else
            {
                IsChildSelectionVisible = false;
            }
            return Task.CompletedTask;
        }

        private bool CanLoginAsSelectedChild()
        {
            return IsChildSelectionVisible && SelectedChildForLogin != null;
        }

        private Task LoginAsSelectedChildAsync()
        {
            if (SelectedChildForLogin == null)
            {
                return Task.CompletedTask;
            }

            Debug.WriteLine($"Login als Kind: {SelectedChildForLogin.Name} (ID: {SelectedChildForLogin.Id}). TODO: ChildTabsPage.");

            return Task.CompletedTask;
        }
    }
}
