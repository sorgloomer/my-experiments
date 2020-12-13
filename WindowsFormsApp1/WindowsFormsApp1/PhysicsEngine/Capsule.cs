namespace WindowsFormsApp1.PhysicsEngine
{
    public struct Capsule
    {
        public Vec2 p0;
        public Vec2 p1;
        public float radius;

        public Capsule(Vec2 p0, Vec2 p1, float radius)
        {
            this.p0 = p0;
            this.p1 = p1;
            this.radius = radius;
        }

        public Capsule TranslateAndRotate(Transform transform)
        {
            return new Capsule
            {
                p0 = transform * p0,
                p1 = transform * p1,
                radius = radius,
            };
        }
    }
}