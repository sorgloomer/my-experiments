using System.Collections.Generic;

namespace WindowsFormsApp1.PhysicsEngine
{
    public static class AaRects
    {
        public static AaRect MergeAll(IEnumerable<AaRect> rects)
        {
            bool first = true;
            AaRect result = default;
            foreach (var r in rects)
            {
                if (first)
                {
                    first = false;
                    result = r;
                }
                else
                {
                    result = AaRect.Merge(result, r);
                }
            }

            return result;
        }

        public static AaRect? MergeNullable(AaRect? a, AaRect? b)
        {
            if (!a.HasValue) return b;
            if (!b.HasValue) return a;
            return AaRect.Merge(a.Value, b.Value);
        }
        public static AaRect? MergeNullable(AaRect? r0, AaRect? r1, AaRect? r2)
        {
            return MergeNullable(r0, MergeNullable(r1, r2));
        }
        public static AaRect? MergeNullable(AaRect? r0, AaRect? r1, AaRect? r2, AaRect? r3)
        {
            return MergeNullable(MergeNullable(r0, r1), MergeNullable(r2, r3));
        }

        public static bool Include(ref AaRect bounds, AaRect newrect)
        {
            if (bounds.Contains(newrect))
            {
                return false;
            }
            bounds = AaRect.Merge(bounds, newrect);
            return true;
        }
    }
}