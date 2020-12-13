namespace WindowsFormsApp1.PhysicsEngine
{
    public struct ContactHalf
    {
        public Rigidbody body0;
        public Rigidbody body1;
        public Vec2 normal;
        public Vec2 position;
        public float penetration;
        public int tag;
    }
}