using System;
using System.Collections.Generic;

namespace WindowsFormsApp1.PhysicsEngine
{
    public abstract class DummySpatialIndex<T> : AbstractSpatialIndex<T>
    {
        private HashSet<T> bodies = new HashSet<T>();
        public override bool Contains(T body)
        {
            return bodies.Contains(body);
        }

        public override bool Add(T body)
        {
            return bodies.Add(body);
        }

        public override bool Remove(T body)
        {
            return bodies.Remove(body);
        }

        public override void TraverseOverlapping(T body, Action<T> action)
        {
            var rect = GetBodyFitRect(body);
            foreach (var b in bodies)
            {
                if (AaRect.Overlaps(GetBodyFitRect(b), rect))
                {
                    action(b);
                }
            }
        }

        public override IEnumerable<T> GetAll()
        {
            return bodies;
        }

        public override IRectTreeNode Root => null;
    }
}