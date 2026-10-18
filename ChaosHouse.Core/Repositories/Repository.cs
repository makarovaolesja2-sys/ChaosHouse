using System;
using System.Collections.Generic;
using System.Linq;

namespace ChaosHouse.Core
{
    public class Repository<T> : IRepository<T>
        where T : HouseMember
    {
        private readonly List<T> _items = new();

        public event EventHandler<HouseEventArgs>? EventLogged;

        public void Add(T item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            if (_items.Contains(item))
                return;

            foreach (T existing in _items)
            {
                existing.ChangeEnergy(5);

                EventLogged?.Invoke(
                    this,
                    new HouseEventArgs(
                        existing,
                        $"{existing.Name} tervitab tegelast {item.Name}. Energia: {existing.Energy}."));
            }

            _items.Add(item);

            item.CrazyActionPerformed += OnMemberCrazyActionPerformed;

            EventLogged?.Invoke(
                this,
                new HouseEventArgs(item, $"{item.Name} lisati hoidlasse."));
        }

        public void Remove(T item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            if (!_items.Contains(item))
                return;

            foreach (T existing in _items.Where(
                x => !ReferenceEquals(x, item)))
            {
                existing.ChangeEnergy(-5);

                EventLogged?.Invoke(
                    this,
                    new HouseEventArgs(
                        existing,
                        $"{existing.Name} jätab tegelasega {item.Name} hüvasti. Energia: {existing.Energy}."));
            }

            item.CrazyActionPerformed -= OnMemberCrazyActionPerformed;

            _items.Remove(item);

            EventLogged?.Invoke(
                this,
                new HouseEventArgs(item, $"{item.Name} eemaldati hoidlast."));
        }

        public string ExecuteCrazyAction(T item, string input)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            if (!_items.Contains(item))
                throw new InvalidOperationException(
                    "Tegelast ei leitud sellest hoidlast.");

            return item.CrazyAction(input);
        }

        private void OnMemberCrazyActionPerformed(
            object? sender,
            HouseEventArgs e)
        {

            foreach (T other in _items.Where(
                x => !ReferenceEquals(x, e.Character)))
            {
                string reaction = other.ReactToAction(e.Character.Name);

                EventLogged?.Invoke(
                    this,
                    new HouseEventArgs(other, reaction));
            }
        }

        public IEnumerable<T> GetAll()
        {
            return _items.ToList();
        }

        public T? Find(Func<T, bool> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return _items.FirstOrDefault(predicate);
        }
    }
}
