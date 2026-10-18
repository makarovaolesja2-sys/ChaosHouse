using System;
using System.Collections.Generic;

namespace ChaosHouse.Core
{
    public interface IRepository<T> where T : HouseMember
    {
        void Add(T item);

        void Remove(T item);

        IEnumerable<T> GetAll();

        T? Find(Func<T, bool> predicate);

        string ExecuteCrazyAction(T item, string input);
    }
}
