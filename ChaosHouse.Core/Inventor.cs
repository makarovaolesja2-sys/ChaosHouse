
namespace ChaosHouse.Core
{
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
            Energy = System.Math.Max(0, Energy - 25);

            string result =
                $"{Name} leiutas rämpsust ja teibist vidina „{CurrentGadget}”! Energiat on alles {Energy}%.";

            NotifyCrazyActionPerformed(result);
            return result;
        }
    }
}
