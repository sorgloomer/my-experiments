using System;
using System.Collections.Generic;

namespace WindowsFormsApp1.PhysicsEngine
{
    public interface ISpatialIndex<T>
    {
        bool Contains(T body);
        bool Add(T body);
        bool Remove(T body);
        void TraverseOverlapping(T body, Action<T> action);
        IEnumerable<T> GetAll();
        void AddRange(IEnumerable<T> bodies);
        
        IRectTreeNode Root { get; }
    }

    public abstract class AbstractSpatialIndex<T> : ISpatialIndex<T>
    {
        public abstract bool Contains(T body);
        public abstract bool Add(T body);
        public abstract bool Remove(T body);
        public abstract void TraverseOverlapping(T body, Action<T> action);
        public abstract IEnumerable<T> GetAll();

        public abstract IRectTreeNode Root { get; }
        
        public abstract AaRect GetBodyFitRect(T body);

        public abstract AaRect GetBodyFatRect(T body);

        public void AddRange(IEnumerable<T> bodies)
        {
            foreach (var b in bodies)
            {
                Add(b);
            }
        }

    }
}