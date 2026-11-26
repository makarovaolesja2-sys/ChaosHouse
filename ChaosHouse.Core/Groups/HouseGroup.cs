using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace ChaosHouse.Core.Groups
{
    public class HouseGroup
    {
        public string Id { get; }
        public string Name { get; }

        public ObservableCollection<HouseMember> Members { get; }
            = new();

        public HouseGroup(string id, string name)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException(
                    "Grupi ID ei tohi olla tühi.", nameof(id));

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException(
                    "Grupi nimi ei tohi olla tühi.", nameof(name));

            Id = id;
            Name = name;
        }

        public void AddMember(HouseMember member)
        {
            ArgumentNullException.ThrowIfNull(member);

            if (Members.Any(m => m.Id == member.Id))
                throw new InvalidOperationException(
                    "See tegelane on juba selles grupis.");

            Members.Add(member);
        }

        public bool RemoveMember(string memberId)
        {
            var member = Members.FirstOrDefault(
                m => m.Id == memberId);

            if (member == null)
                return false;

            Members.Remove(member);
            return true;
        }
    }
}


