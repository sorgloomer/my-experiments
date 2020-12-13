namespace WindowsFormsApp1.PhysicsEngine
{
    public class Rigidbody
    {
        public int index;
        public BodyType type;
        public Vec2 position;
        public float angle;
        public float lastAngle;
        public Vec2 lastPosition;
        public Vec2 positionVelocity;
        public float angularVelocity;
        public Capsule fixture;
        public CapsuleCache fixtureCache;
        public Transform transform;
        public float invMass, invAngularMass;
    }
}