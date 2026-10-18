using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using ChaosHouse.Core;

namespace ChaosHouse.Core.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly Repository<TalkingFridge> _fridgeRepository = new();
        private readonly Repository<Ghost> _ghostRepository = new();
        private readonly Repository<Inventor> _inventorRepository = new();

        private HouseMember? _selectedMember;
        private string _selectedRepositoryType = "Rääkiv külmkapp";
        private string _inputText = "";
        private string _searchText = "";
        private string _logText = "";
        private int _counter = 1;

        public ObservableCollection<string> RepositoryTypes { get; } =
            new()
            {
                "Rääkiv külmkapp",
                "Kummitus",
                "Leiutaja"
            };

        public ObservableCollection<HouseMember> HouseMembers { get; } = new();
        public ObservableCollection<HouseMember> VisibleMembers { get; } = new();

        public HouseMember? SelectedMember
        {
            get => _selectedMember;
            set
            {
                if (_selectedMember != value)
                {
                    _selectedMember = value;
                    OnPropertyChanged();
                    RemoveCommand.RaiseCanExecuteChanged();
                    ExecuteActionCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public string SelectedRepositoryType
        {
            get => _selectedRepositoryType;
            set
            {
                if (_selectedRepositoryType != value)
                {
                    _selectedRepositoryType = value;
                    OnPropertyChanged();

                    SelectedMember = null;
                    RefreshVisibleMembers();
                }
            }
        }

        public string InputText
        {
            get => _inputText;
            set
            {
                if (_inputText != value)
                {
                    _inputText = value;
                    OnPropertyChanged();
                    ExecuteActionCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    OnPropertyChanged();
                    RefreshVisibleMembers();
                }
            }
        }

        public string LogText
        {
            get => _logText;
            private set
            {
                if (_logText != value)
                {
                    _logText = value;
                    OnPropertyChanged();
                }
            }
        }

        public string GroupedMemberSummary =>
            string.Join(
                Environment.NewLine,
                HouseMembers
                    .GroupBy(member => member.TypeName)
                    .OrderBy(group => group.Key)
                    .Select(group => $"{group.Key}: {group.Count()}"));

        public RelayCommand AddFridgeCommand { get; }
        public RelayCommand AddGhostCommand { get; }
        public RelayCommand AddInventorCommand { get; }
        public RelayCommand RemoveCommand { get; }
        public RelayCommand ExecuteActionCommand { get; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public MainViewModel()
        {
            _fridgeRepository.EventLogged += Repository_EventLogged;
            _ghostRepository.EventLogged += Repository_EventLogged;
            _inventorRepository.EventLogged += Repository_EventLogged;

            AddFridgeCommand = new RelayCommand(
                _ => AddFridge());

            AddGhostCommand = new RelayCommand(
                _ => AddGhost());

            AddInventorCommand = new RelayCommand(
                _ => AddInventor());

            RemoveCommand = new RelayCommand(
                _ => RemoveMember(),
                _ => SelectedMember != null);

            ExecuteActionCommand = new RelayCommand(
                _ => ExecuteAction(),
                _ => SelectedMember != null
                     && !string.IsNullOrWhiteSpace(InputText));

            AddFridge(new TalkingFridge(NewId(), "Sigmund"));
            AddGhost(new Ghost(NewId(), "Kasper"));
            AddInventor(new Inventor(NewId(), "Tesla"));

            AddLog("Kaosemaja on avatud!");
        }

        private string NewId()
        {
            return Guid.NewGuid().ToString("N")[..5];
        }

        private void AddFridge()
        {
            AddFridge(
                new TalkingFridge(NewId(), $"Külmkapp-{_counter++}"));
        }

        private void AddGhost()
        {
            AddGhost(
                new Ghost(NewId(), $"Kummitus-{_counter++}"));
        }

        private void AddInventor()
        {
            AddInventor(
                new Inventor(NewId(), $"Leiutaja-{_counter++}"));
        }

        private void AddFridge(TalkingFridge member)
        {
            _fridgeRepository.Add(member);
            HouseMembers.Add(member);
            AfterCollectionChanged();
        }

        private void AddGhost(Ghost member)
        {
            _ghostRepository.Add(member);
            HouseMembers.Add(member);
            AfterCollectionChanged();
        }

        private void AddInventor(Inventor member)
        {
            _inventorRepository.Add(member);
            HouseMembers.Add(member);
            AfterCollectionChanged();
        }

        private void RemoveMember()
        {
            if (SelectedMember == null)
                return;

            HouseMember member = SelectedMember;

            switch (member)
            {
                case TalkingFridge fridge:
                    _fridgeRepository.Remove(fridge);
                    break;

                case Ghost ghost:
                    _ghostRepository.Remove(ghost);
                    break;

                case Inventor inventor:
                    _inventorRepository.Remove(inventor);
                    break;

                default:
                    AddLog("Seda tegelasetüüpi ei toetata.");
                    return;
            }

            HouseMembers.Remove(member);
            SelectedMember = null;

            AfterCollectionChanged();
        }

        private void ExecuteAction()
        {
            if (SelectedMember == null
                || string.IsNullOrWhiteSpace(InputText))
                return;

            try
            {
                string result = SelectedMember switch
                {
                    TalkingFridge fridge =>
                        _fridgeRepository.ExecuteCrazyAction(fridge, InputText),

                    Ghost ghost =>
                        _ghostRepository.ExecuteCrazyAction(ghost, InputText),

                    Inventor inventor =>
                        _inventorRepository.ExecuteCrazyAction(inventor, InputText),

                    _ => "Seda tegelasetüüpi ei toetata."
                };

                AddLog(result);

                OnPropertyChanged(nameof(GroupedMemberSummary));
            }
            catch (ArgumentException ex)
            {
                AddLog($"Vigane sisend: {ex.Message}");
            }
            catch (InvalidOperationException ex)
            {
                AddLog($"Tegevust ei saanud teha: {ex.Message}");
            }
        }

        private void Repository_EventLogged(
            object? sender,
            HouseEventArgs e)
        {
            AddLog(e.Message);
            OnPropertyChanged(nameof(GroupedMemberSummary));
        }

        private void AfterCollectionChanged()
        {
            RefreshVisibleMembers();
            OnPropertyChanged(nameof(GroupedMemberSummary));
        }

        private void RefreshVisibleMembers()
        {
            var filtered = HouseMembers
                .Where(member => member.TypeName == SelectedRepositoryType)
                .Where(member =>
                    string.IsNullOrWhiteSpace(SearchText)
                    || member.Name.Contains(
                        SearchText,
                        StringComparison.OrdinalIgnoreCase)
                    || member.TypeName.Contains(
                        SearchText,
                        StringComparison.OrdinalIgnoreCase))
                .OrderBy(member => member.Name)
                .ToList();

            VisibleMembers.Clear();

            foreach (HouseMember member in filtered)
                VisibleMembers.Add(member);
        }

        private void AddLog(string message)
        {
            LogText +=
                $"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}";
        }

        private void OnPropertyChanged(
            [CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(propertyName));
        }
    }
}
