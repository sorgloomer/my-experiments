using System.Collections.Generic;

namespace WindowsFormsApp1.PhysicsEngine
{
    public class BoundingRects
    {
        public float fatSizeFactor = 0.2f;
        public float fatVelocityFactor = 0.2f;

        public AaRect GetBodyFitRect(Rigidbody body)
        {
            var c = body.fixtureCache.capsule;
            var r = new Vec2(c.radius, c.radius);
            return new AaRect(
                Vec2.Min(c.p0, c.p1) - r,
                Vec2.Max(c.p0, c.p1) + r
            );
        }
        public AaRect GetBodyFatRect(Rigidbody body)
        {
            var rect = GetBodyFitRect(body);
            var fat = rect.ExtendSides(rect.GetSize() * fatSizeFactor);
            return AaRect.Merge(fat, fat.Translate(body.positionVelocity * fatVelocityFactor));
        }
    }
}