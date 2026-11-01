using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using ChaosHouse.Core;
using ChaosHouse.Core.Simulation;

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
        private readonly WorldSimulationService _simulationService = new();
        private CancellationTokenSource? _simulationCancellation;
        private bool _isSimulationRunning;
        private int _workUnits = 20_000_000;
        private string _simulationMode = "Järjestikune";
        private string _simulationReport = "Vali vähemalt kolm objekti ja käivita arvutus.";
        private string _simulationTotal = "";
        private (string Fingerprint, string Mode, SimulationResult[] Results)? _previousSimulation;

        public ObservableCollection<string> RepositoryTypes { get; } =
            new()
            {
                "Rääkiv külmkapp",
                "Kummitus",
                "Leiutaja"
            };

        public ObservableCollection<HouseMember> HouseMembers { get; } = new();
        public ObservableCollection<HouseMember> VisibleMembers { get; } = new();
        public ObservableCollection<SimulationTarget> SimulationTargets { get; } = new();
        public ObservableCollection<string> SimulationModes { get; } = new() { "Järjestikune", "Paralleelne" };

        public int WorkUnits
        {
            get => _workUnits;
            set { if (_workUnits != value) { _workUnits = value; OnPropertyChanged(); } }
        }

        public string SimulationMode
        {
            get => _simulationMode;
            set { if (_simulationMode != value) { _simulationMode = value; OnPropertyChanged(); } }
        }

        public bool IsSimulationRunning
        {
            get => _isSimulationRunning;
            private set
            {
                if (_isSimulationRunning == value) return;
                _isSimulationRunning = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanEditSimulationTargets));
                RunSimulationCommand.RaiseCanExecuteChanged();
                CancelSimulationCommand.RaiseCanExecuteChanged();
            }
        }

        public bool CanEditSimulationTargets => !IsSimulationRunning;

        public string SimulationReport
        {
            get => _simulationReport;
            private set { if (_simulationReport != value) { _simulationReport = value; OnPropertyChanged(); } }
        }

        public string SimulationTotal
        {
            get => _simulationTotal;
            private set { if (_simulationTotal != value) { _simulationTotal = value; OnPropertyChanged(); } }
        }

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
        public RelayCommand RunSimulationCommand { get; }
        public RelayCommand CancelSimulationCommand { get; }

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

            RunSimulationCommand = new RelayCommand(
                _ => _ = RunSimulationAsync(),
                _ => !IsSimulationRunning);

            CancelSimulationCommand = new RelayCommand(
                _ => _simulationCancellation?.Cancel(),
                _ => IsSimulationRunning);

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
            SynchronizeSimulationTargets();
            RefreshVisibleMembers();
            OnPropertyChanged(nameof(GroupedMemberSummary));
        }

        private void SynchronizeSimulationTargets()
        {
            var currentIds = HouseMembers.Select(member => member.Id).ToHashSet();
            foreach (SimulationTarget target in SimulationTargets.Where(target => !currentIds.Contains(target.Member.Id)).ToList())
                SimulationTargets.Remove(target);

            foreach (HouseMember member in HouseMembers)
            {
                if (SimulationTargets.All(target => target.Member.Id != member.Id))
                    SimulationTargets.Add(new SimulationTarget(member) { IsSelected = true });
            }
        }

        private async Task RunSimulationAsync()
        {
            SimulationTarget[] chosen = SimulationTargets.Where(target => target.IsSelected).ToArray();
            if (chosen.Length < 3)
            {
                SimulationReport = "Vali arvutuseks vähemalt kolm maailma objekti.";
                return;
            }
            if (WorkUnits < 1 || WorkUnits > 50_000_000)
            {
                SimulationReport = "Töömaht peab olema vahemikus 1 kuni 50 000 000 tsüklit objekti kohta.";
                return;
            }

            SimulationSnapshot[] snapshots = chosen.Select(target => target.Capture()).ToArray();
            int workUnits = WorkUnits;
            string mode = SimulationMode;
            string fingerprint = string.Join("|", snapshots.Select(snapshot =>
                $"{snapshot.Id}:{snapshot.Name}:{snapshot.TypeName}:{snapshot.Energy}:{snapshot.InjectFailure}")) + $"|{workUnits}";

            _simulationCancellation = new CancellationTokenSource();
            CancellationToken token = _simulationCancellation.Token;
            IsSimulationRunning = true;
            SimulationTotal = "Arvutus käib…";
            SimulationReport = $"Režiim: {mode}; objekte: {snapshots.Length}; töömaht: {workUnits:N0} tsüklit objekti kohta.{Environment.NewLine}" +
                "Sisend on käivituse alguses hetktõmmisena fikseeritud.";
            var total = Stopwatch.StartNew();
            SimulationResult[] results;
            string? orchestrationError = null;

            try
            {
                if (mode == "Paralleelne")
                {
                    Task<SimulationResult>[] tasks = snapshots
                        .Select(snapshot => Task.Run(() => _simulationService.Calculate(snapshot, workUnits, token)))
                        .ToArray();
                    results = await Task.WhenAll(tasks);
                }
                else
                {
                    var sequentialResults = new List<SimulationResult>(snapshots.Length);
                    foreach (SimulationSnapshot snapshot in snapshots)
                    {
                        if (token.IsCancellationRequested)
                        {
                            sequentialResults.Add(new SimulationResult(snapshot.Name, SimulationStatus.Cancelled,
                                "Töö jäi ühise tühistamise tõttu alustamata.", TimeSpan.Zero));
                            continue;
                        }

                        Task<SimulationResult> task = Task.Run(
                            () => _simulationService.Calculate(snapshot, workUnits, token));
                        try { sequentialResults.Add(await task); }
                        catch (OperationCanceledException)
                        {
                            sequentialResults.Add(new SimulationResult(snapshot.Name, SimulationStatus.Cancelled,
                                "Töö tühistati enne käivitumist.", TimeSpan.Zero));
                        }
                    }
                    results = sequentialResults.ToArray();
                }
            }
            catch (Exception ex)
            {
                orchestrationError = $"Tööde käivitamise viga: {ex.Message}";
                results = Array.Empty<SimulationResult>();
            }
            finally
            {
                total.Stop();
                _simulationCancellation?.Dispose();
                _simulationCancellation = null;
                IsSimulationRunning = false;
            }

            SimulationTotal = $"Kogu töö aeg: {total.Elapsed.TotalSeconds:F3} s";
            SimulationReport = string.Join(Environment.NewLine, results.Select(result =>
                $"{result.ObjectName} | {StatusText(result.Status)} | {result.Details} | {result.Duration.TotalSeconds:F3} s"));
            if (orchestrationError != null)
                SimulationReport += Environment.NewLine + orchestrationError;

            if (_previousSimulation is { } previous
                && previous.Fingerprint == fingerprint
                && previous.Mode != mode)
            {
                bool same = previous.Results.Length == results.Length
                    && previous.Results.Zip(results).All(pair =>
                        pair.First.Status == pair.Second.Status
                        && pair.First.Checksum == pair.Second.Checksum
                        && pair.First.Details == pair.Second.Details);
                SimulationReport += Environment.NewLine + Environment.NewLine +
                    $"Võrdlus režiimiga {previous.Mode}: {(same ? "kõigi objektide tulemused on samad" : "tulemustes on erinevusi")}.";
            }

            _previousSimulation = (fingerprint, mode, results);
            SimulationReport += Environment.NewLine + SimulationTotal;
            AddLog($"{mode} arvutus lõpetas: {results.Count(result => result.Status == SimulationStatus.Success)} õnnestus, " +
                $"{results.Count(result => result.Status == SimulationStatus.Error)} viga, " +
                $"{results.Count(result => result.Status == SimulationStatus.Cancelled)} tühistatud. {SimulationTotal}");
        }

        private static string StatusText(SimulationStatus status) => status switch
        {
            SimulationStatus.Success => "Õnnestus",
            SimulationStatus.Error => "Viga",
            SimulationStatus.Cancelled => "Tühistatud",
            _ => status.ToString()
        };

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
