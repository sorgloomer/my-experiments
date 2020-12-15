namespace TimeConstraintPhysics
{
    public struct Vec2
    {
        public float x, y;

        public Vec2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public static Vec2 operator +(Vec2 a, Vec2 b)
        {
            return xy(a.x + b.x, a.y + b.y);
        }
        public static Vec2 operator -(Vec2 a, Vec2 b)
        {
            return xy(a.x - b.x, a.y - b.y);
        }
        public static Vec2 operator -(Vec2 a)
        {
            return xy(-a.x, -a.y);
        }
        public static Vec2 operator +(Vec2 a)
        {
            return a;
        }
        public static float operator *(Vec2 a, Vec2 b)
        {
            return dot(a, b);
        }
        public static Vec2 operator *(float s, Vec2 v)
        {
            return scale(v, s);
        }
        public static Vec2 operator *(Vec2 v, float s)
        {
            return scale(v, s);
        }
        public static Vec2 operator /(Vec2 v, float s)
        {
            return scale(v, 1.0f / s);
        }
        public static float operator %(Vec2 a, Vec2 b)
        {
            return cross(a, b);
        }
        public static float dot(Vec2 a, Vec2 b)
        {
            return a.x * b.x + a.y * b.y;
        }
        public static Vec2 scale(Vec2 v, float s)
        {
            return xy(s * v.x, s * v.y);
        }
        public static float cross(Vec2 a, Vec2 b)
        {
            return a.x * b.y - a.y * b.x;
        }
        public static Vec2 xy(float x, float y)
        {
            return new Vec2(x, y);
        }
        public static Vec2 zero()
        {
            return xy(0, 0);
        }

        public float len2()
        {
            return dot(this, this);
        }

        public Vec2 rot()
        {
            return xy(-y, x);
        }
    }
}