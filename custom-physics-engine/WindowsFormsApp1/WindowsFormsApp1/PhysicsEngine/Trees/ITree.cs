using System;
using System.Collections.Generic;

namespace WindowsFormsApp1.PhysicsEngine
{
    public interface ITree<T>
    {
        bool Contains(T body);
        bool Add(T body);
        bool Remove(T body);
        void TraverseOverlapping(AaRect rect, Action<T> action);
        IEnumerable<T> GetAll();
        void AddRange(IEnumerable<T> bodies);
    }

    public abstract class AbstractTree<T> : ITree<T>
    {
        public abstract bool Contains(T body);
        public abstract bool Add(T body);
        public abstract bool Remove(T body);
        public abstract void TraverseOverlapping(AaRect rect, Action<T> action);
        public abstract IEnumerable<T> GetAll();

        public void AddRange(IEnumerable<T> bodies)
        {
            foreach (var b in bodies)
            {
                Add(b);
            }
        }
    }
}