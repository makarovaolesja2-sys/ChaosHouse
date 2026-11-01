using System.ComponentModel;
using System.Runtime.CompilerServices;
using ChaosHouse.Core.Simulation;

namespace ChaosHouse.Core.ViewModels;

public sealed class SimulationTarget : INotifyPropertyChanged
{
    private bool _isSelected;
    private bool _injectFailure;

    public HouseMember Member { get; }
    public string Name => Member.Name;
    public string TypeName => Member.TypeName;
    public int Energy => Member.Energy;

    public bool IsSelected
    {
        get => _isSelected;
        set { if (_isSelected != value) { _isSelected = value; OnPropertyChanged(); } }
    }

    public bool InjectFailure
    {
        get => _injectFailure;
        set { if (_injectFailure != value) { _injectFailure = value; OnPropertyChanged(); } }
    }

    public SimulationTarget(HouseMember member)
    {
        Member = member;
        Member.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(HouseMember.Energy)) OnPropertyChanged(nameof(Energy));
            if (args.PropertyName == nameof(HouseMember.Name)) OnPropertyChanged(nameof(Name));
        };
    }

    public SimulationSnapshot Capture() =>
        new(Member.Id, Member.Name, Member.TypeName, Member.Energy, InjectFailure);

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
