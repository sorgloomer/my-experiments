using System;

namespace WindowsFormsApp1.PhysicsEngine
{
    public struct Transform
    {
        public Vec2 x, y, t;
        
        public static readonly Transform Identity = new Transform(Vec2.X, Vec2.Y, Vec2.Zero);

        public Transform(Vec2 x, Vec2 y, Vec2 t)
        {
            this.x = x;
            this.y = y;
            this.t = t;
        }

        public static Transform FromElements(float xx, float xy, float yx, float yy, float tx, float ty)
        {
            return new Transform(new Vec2(xx, xy), new Vec2(yx, yy), new Vec2(tx, ty));
        }

        public Vec2 Apply(Vec2 v)
        {
            return v.x * x + v.y * y + t;
        }

        public Vec2 ApplyRotation(Vec2 v)
        {
            return v.x * x + v.y * y;
        }

        public static Transform Concatenate(Transform a, Transform b)
        {
            return new Transform(
                a.ApplyRotation(b.x),
                a.ApplyRotation(b.y),
                a.Apply(b.t)
            );
        }

        public static Transform Rotation(float angle)
        {
            return RotationTranslation(angle, Vec2.Zero);
        }
        
        public static Transform RotationTranslation(float angle, Vec2 t)
        {
            Vec2 rot = Vec2.CosSin(angle);
            return new Transform(rot, rot.Rot90(), t);
        }

        public static Transform Translation(Vec2 t)
        {
            return new Transform(Vec2.X, Vec2.Y, t);
        }
        
        public static Transform Translation(float tx, float ty)
        {
            return Translation(new Vec2(tx, ty));
        }
        
        public static Transform Scale(float s)
        {
            return FromElements(s, 0, 0, s, 0, 0);
        }

        public static Vec2 operator *(Transform t, Vec2 v)
        {
            return t.Apply(v);
        }
        
        public static Transform operator *(Transform a, Transform b)
        {
            return Concatenate(a, b);
        }
    }
}
