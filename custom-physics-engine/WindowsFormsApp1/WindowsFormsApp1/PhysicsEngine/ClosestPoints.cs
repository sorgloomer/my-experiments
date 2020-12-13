namespace WindowsFormsApp1.PhysicsEngine
{
    public struct ClosestPoints
    {
        public Vec2 point0, point1;
        public Vec2 normal;
        public float distance, spread;
        public bool degenerate, spreaded;

        public ClosestPoints(Vec2 point0, Vec2 point1, Vec2 normal, float distance)
        {
            this.point0 = point0;
            this.point1 = point1;
            this.normal = normal;
            this.distance = distance;
            spread = 0;
            spreaded = false;
            degenerate = false;
        }
    }
}