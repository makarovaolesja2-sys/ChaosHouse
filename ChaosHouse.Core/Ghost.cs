using System;

namespace ChaosHouse.Core
{
    public class Ghost : HouseMember, IScare
    {
        public int SpookinessLevel { get; private set; }

        public Ghost(string id, string name)
            : base(id, name, "Kummitus")
        {
            SpookinessLevel = 50;
        }

        public Ghost(string id, string name, int energy, int spookinessLevel)
            : base(id, name, "Kummitus")
        {
            if (spookinessLevel < 0 || spookinessLevel > 100)
                throw new ArgumentOutOfRangeException(nameof(spookinessLevel));

            SpookinessLevel = spookinessLevel;
            RestoreEnergy(energy);
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
                Energy = System.Math.Max(0, Energy - 10);

                string result =
                    $"{Name} muutis hirmutasemeks {SpookinessLevel}%. Lühtrid hakkasid kõikuma!";

                NotifyCrazyActionPerformed(result);
                return result;
            }

            return $"{Name} ei mõista teksti „{input}”. Ta vajab hirmutaseme numbrit!";
        }
    }
}
