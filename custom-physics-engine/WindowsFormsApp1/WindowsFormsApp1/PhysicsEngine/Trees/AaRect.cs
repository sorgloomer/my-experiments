using System;

namespace WindowsFormsApp1.PhysicsEngine
{
    public struct AaRect
    {
        public Vec2 min, max;

        public static AaRect xyxy(float minx, float miny, float maxx, float maxy)
        {
            return mm(Vec2.xy(minx, miny), Vec2.xy(maxx, maxy));
        }

        public static AaRect mm(Vec2 min, Vec2 max)
        {
            return new AaRect(min, max);
        }

        public static AaRect mm(float minx, float miny, Vec2 max)
        {
            return mm(Vec2.xy(minx, miny), max);
        }
        public static AaRect mm(Vec2 min, float maxx, float maxy)
        {
            return mm(min, Vec2.xy(maxx, maxy));
        }

        public AaRect(Vec2 min, Vec2 max)
        {
            this.min = min;
            this.max = max;
        }

        public float GetArea() => (max.x - min.x) * (max.y - min.y);
        public Vec2 GetSize() => new Vec2(max.x - min.x, max.y - min.y);
        public float GetMaxSize()
        {
            var size = GetSize();
            return Math.Max(size.x, size.y);
        }

        public AaRect ExtendSides(Vec2 extension)
        {
            return new AaRect(min - extension, max + extension);
        }
        
        public static bool Overlaps(AaRect a, AaRect b)
        {
            return
                a.min.x < b.max.x && a.max.x > b.min.x && 
                a.min.y < b.max.y && a.max.y > b.min.y;
        }
        public bool Contains(AaRect small)
        {
            return min.x <= small.min.x &&
                   max.x >= small.max.x &&
                   min.y <= small.min.y &&
                   max.y >= small.max.y;
        }

        public static AaRect Merge(AaRect a, AaRect b)
        {
            return new AaRect(Vec2.Min(a.min, b.min), Vec2.Max(a.max, b.max));
        }

        public AaRect Translate(Vec2 t)
        {
            return new AaRect(min + t, max + t);
        }
    }
}