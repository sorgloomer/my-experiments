namespace WindowsFormsApp1.PhysicsEngine
{
    public struct Circle
    {
        public Vec2 center;
        public float radius;

        public Circle(Vec2 c, float r)
        {
            center = c;
            radius = r;
        }

        public static ClosestPoints Collide(Circle a, Circle b)
        {
            Vec2 delta = b.center - a.center;
            float centerdist = delta.Len();

            Vec2 normal = delta.SafeDivPositive(centerdist, out var degenerate);

            return new ClosestPoints(
                point0: a.center + normal * a.radius,
                point1: b.center - normal * b.radius,
                normal: normal,
                distance: centerdist - (a.radius + b.radius)
            ) {
                degenerate = degenerate,
            };
        }
    }
}