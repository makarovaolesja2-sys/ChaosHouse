
using System;

namespace ChaosHouse.Core
{
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
                return $"{Name} ümiseb pahaselt: käsk puudub! Midagi ei juhtunud.";

            if (input.Equals("lock", StringComparison.OrdinalIgnoreCase))
            {
                IsLocked = true;
                Energy = Math.Max(0, Energy - 15);

                string result =
                    $"{Name} lukustas köögiukse! Toidule ligipääsuks on vaja parooli. Energia: {Energy}%.";

                NotifyCrazyActionPerformed(result);
                return result;
            }

            if (input.Equals("unlock", StringComparison.OrdinalIgnoreCase))
            {
                IsLocked = false;

                string result =
                    $"{Name} avas luku. Võite vorsti võtta.";

                NotifyCrazyActionPerformed(result);
                return result;
            }

            return $"{Name} ei saanud käsust „{input}” aru ja väljastas vea 404.";
        }
    }
}
