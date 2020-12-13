using System;
using System.Diagnostics.Contracts;

namespace WindowsFormsApp1.PhysicsEngine
{
    public struct Vec2
    {
        public float x, y;

        public static readonly Vec2 Zero = new Vec2(0, 0);
        public static readonly Vec2 X = new Vec2(1, 0);
        public static readonly Vec2 Y = new Vec2(0, 1);

        public Vec2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public static Vec2 Add(Vec2 a, Vec2 b)
        {
            return new Vec2(a.x + b.x, a.y + b.y);
        }

        public static Vec2 Sub(Vec2 a, Vec2 b)
        {
            return new Vec2(a.x - b.x, a.y - b.y);
        }

        public static Vec2 Scale(Vec2 v, float s)
        {
            return new Vec2(v.x * s, v.y * s);
        }

        public static Vec2 Mul(Vec2 a, Vec2 b)
        {
            return new Vec2(a.x * b.x, a.y * b.y);
        }

        public static Vec2 Lerp(float x, Vec2 a, Vec2 b)
        {
            return Combine(1-x, a, x, b);
        }

        public static Vec2 ClampLerp(float x, Vec2 a, Vec2 b)
        {
            return Lerp(Mathf.Clamp01(x), a, b);
        }

        public static Vec2 Combine(float x0, Vec2 v0, float x1, Vec2 v1)
        {
            return x0 * v0 + x1 * v1;
        }
    
        public static float Dot(Vec2 a, Vec2 b)
        {
            return a.x * b.x + a.y * b.y;
        }
        
        public Vec2 Normalized()
        {
            return SafeDivPositive(Len());
        }
        public Vec2 SafeDivPositive(float l)
        {
            return SafeDivPositive(l, out _);
        }
        public Vec2 SafeDivPositive(float l, out bool failed)
        {
            return this * Mathf.TryInvertPositive(l, out failed);
        }

        public static Vec2 CosSin(float angle)
        {
            return new Vec2((float)Math.Cos(angle), (float)Math.Sin(angle));
        }
        
        [Pure]
        public Vec2 Rot90()
        {
            return new Vec2(-y, x);
        }
        
        public static Vec2 RotateScale(Vec2 a, Vec2 b)
        {
            return new Vec2(a.x * b.x - a.y * b.y, a.x * b.y + a.y * b.x);
        }

        public float Len2()
        {
            return Dot(this, this);
        }
        public float Len()
        {
            return (float)Math.Sqrt(Len2());
        }

        public static float Dist2(Vec2 a, Vec2 b)
        {
            return (a - b).Len2();
        }
        public static float Dist(Vec2 a, Vec2 b)
        {
            return (a - b).Len();
        }

        public static float Cross(Vec2 a, Vec2 b)
        {
            return a.x * b.y - a.y * b.x;
        }

        public static Vec2 Cross(Vec2 a, float b)
        {
            return -Cross(b, a);
        }
        
        public static Vec2 Cross(float a, Vec2 b)
        {
            return b.Rot90() * a;
        }
        

        public static Vec2 operator+(Vec2 a, Vec2 b)
        {
            return Add(a, b);
        }
        public static Vec2 operator-(Vec2 a, Vec2 b)
        {
            return Sub(a, b);
        }
        public static Vec2 operator -(Vec2 v)
        {
            return new Vec2(-v.x, -v.y);
        }
        public static float operator*(Vec2 a, Vec2 b)
        {
            return Dot(a, b);
        }
        public static Vec2 operator*(Vec2 a, float b)
        {
            return Scale(a, b);
        }
        public static Vec2 operator*(float a, Vec2 b)
        {
            return Scale(b, a);
        }


        public static float operator%(Vec2 a, Vec2 b)
        {
            return Cross(a, b);
        }
        public static Vec2 operator%(Vec2 a, float b)
        {
            return Cross(a, b);
        }
        public static Vec2 operator%(float a, Vec2 b)
        {
            return Cross(a, b);
        }

        public Vec2 Clamp(float maxLength)
        {
            var length = Len();
            if (length <= maxLength)
            {
                return this;
            }
            return this * (maxLength / length);
        }

        public Vec2 Abs()
        {
            return new Vec2(Math.Abs(x), Math.Abs(y));
        }

        public static Vec2 Min(Vec2 a, Vec2 b)
        {
            return new Vec2(Math.Min(a.x, b.x), Math.Min(a.y, b.y));
        }
        public static Vec2 Max(Vec2 a, Vec2 b)
        {
            return new Vec2(Math.Max(a.x, b.x), Math.Max(a.y, b.y));
        }
    }
}