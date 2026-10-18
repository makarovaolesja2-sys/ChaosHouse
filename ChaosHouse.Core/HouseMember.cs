using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ChaosHouse.Core
{
    public interface ICook
    {
        string CookSomething();
    }

    public interface IScare
    {
        string ScareSomeone();
    }

    public interface IRepair
    {
        string RepairSomething();
    }

    public abstract class HouseMember : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public event EventHandler<HouseEventArgs>? CrazyActionPerformed;

        protected void OnPropertyChanged(
            [CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(propertyName));
        }

        protected void NotifyCrazyActionPerformed(string message)
        {
            CrazyActionPerformed?.Invoke(
                this,
                new HouseEventArgs(this, message));
        }

        public string Id { get; }
        public string Name { get; protected set; }
        public string TypeName { get; }

        private int _energy;

        public int Energy
        {
            get => _energy;
            protected set
            {
                if (value < 0 || value > 100)
                    throw new ArgumentOutOfRangeException(nameof(value));

                if (_energy == value)
                    return;

                _energy = value;
                OnPropertyChanged();
            }
        }

        public void ChangeEnergy(int amount)
        {
            Energy = Math.Clamp(Energy + amount, 0, 100);
        }

        public virtual string ReactToAction(string characterName)
        {
            ChangeEnergy(-2);

            return $"{Name} reageerib tegelase {characterName} tegevusele. Energia: {Energy}.";
        }

        protected HouseMember(string id, string name, string typeName)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException(
                    "ID ei tohi olla tühi.", nameof(id));

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException(
                    "Nimi ei tohi olla tühi.", nameof(name));

            Id = id;
            Name = name;
            TypeName = typeName;
            Energy = 100;
        }

        public abstract string CrazyAction(string input);
    }
}
