
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

        protected void OnPropertyChanged(
            [CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(propertyName));
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

        protected HouseMember(string id, string name, string typeName)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("ID ei tohi olla tühi.", nameof(id));

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Nimi ei tohi olla tühi.", nameof(name));

            Id = id;
            Name = name;
            TypeName = typeName;
            Energy = 100;
        }

        public abstract string CrazyAction(string input);
    }

    public class TalkingFridge : HouseMember, ICook, IRepair
    {
        public bool IsLocked { get; private set; }

        public TalkingFridge(string id, string name)
            : base(id, name, "Rääkiv külmkapp")
        {
            IsLocked = false;
        }

        public string CookSomething() =>
            $"{Name} proovib valmistada jäist suppi.";

        public string RepairSomething() =>
            $"{Name} taaskäivitab külmutuskompressori.";

        public override string CrazyAction(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return $"{Name} ümiseb pahaselt: käsk puudub! Midagi ei juhtunud.";
            }

            if (input.Equals("lock", StringComparison.OrdinalIgnoreCase))
            {
                IsLocked = true;
                Energy = Math.Max(0, Energy - 15);

                return $"{Name} lukustas köögiukse! Toidule ligipääsuks on vaja parooli. Energia: {Energy}%.";
            }

            if (input.Equals("unlock", StringComparison.OrdinalIgnoreCase))
            {
                IsLocked = false;
                return $"{Name} avas luku. Võite vorsti võtta.";
            }

            return $"{Name} ei saanud käsust „{input}” aru ja väljastas vea 404.";
        }
    }

    public class Ghost : HouseMember, IScare
    {
        public int SpookinessLevel { get; private set; }

        public Ghost(string id, string name)
            : base(id, name, "Kummitus")
        {
            SpookinessLevel = 50;
        }

        public string ScareSomeone() =>
            $"{Name} hüüab voodi all jubedalt: „Buuu!”";

        public override string CrazyAction(string input)
        {
            if (int.TryParse(input, out int level))
            {
                if (level < 0 || level > 100)
                {
                    return $"{Name} takerdus voodilina sisse. Hirmutase peab olema 0–100.";
                }

                SpookinessLevel = level;
                Energy = Math.Max(0, Energy - 10);

                return $"{Name} muutis hirmutasemeks {SpookinessLevel}%. Lühtrid hakkasid kõikuma!";
            }

            return $"{Name} ei mõista teksti „{input}”. Ta vajab hirmutaseme numbrit!";
        }
    }

    public class Inventor : HouseMember, IRepair
    {
        public string CurrentGadget { get; private set; }

        public Inventor(string id, string name)
            : base(id, name, "Leiutaja")
        {
            CurrentGadget = "Lihtne kruvikeeraja";
        }

        public string RepairSomething() =>
            $"{Name} parandab röstrit sinise isoleerlindiga.";

        public override string CrazyAction(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return $"{Name} kaotas mõttelõnga. Sisesta vidina nimi.";
            }

            CurrentGadget = input.Trim();
            Energy = Math.Max(0, Energy - 25);

            return $"{Name} leiutas rämpsust ja teibist vidina „{CurrentGadget}”! Energiat on alles {Energy}%.";
        }
    }
}
