using System;

namespace WindowsFormsApp1.PhysicsEngine
{
    public struct CapsuleCache
    {
        public readonly Capsule capsule;
        public readonly Vec2 normal;
        public readonly Vec2 center;
        public readonly float length;
        public readonly float invLength;
        public readonly bool circle;

        public CapsuleCache(Capsule c)
        {
            capsule = c;
            Vec2 d = c.p1 - c.p0;
            length = d.Len();
            invLength = Mathf.TryInvertPositive(length, out circle);
            normal = d.Rot90() * invLength;
            center = Vec2.Lerp(0.5f, c.p0, c.p1);
        }

        private static ClosestPoints CollideCircles(CapsuleCache c0, CapsuleCache c1)
        {
            Vec2 dir = c1.center - c0.center;
            float dist = dir.Len();
            return new ClosestPoints(
                point0: c0.center,
                point1: c1.center,
                normal: dir.SafeDivPositive(dist),
                distance: dist - c0.capsule.radius - c1.capsule.radius
            );
        }


        private static Vec2 ClosestEnd(CapsuleCache subject, CapsuleCache reference)
        {
            if (subject.circle) return subject.center;
            return Mathf.Abs((subject.capsule.p0 - reference.center) * reference.normal) <=
                   Mathf.Abs((subject.capsule.p1 - reference.center) * reference.normal)
                ? subject.capsule.p0
                : subject.capsule.p1;
        }
        
        private static Vec2 ClosestAxisPoint(CapsuleCache subject, Vec2 reference)
        {
            // ((center + (p1 - p0) * j) - reference) * (p1 - p0) = 0
            //    dir = p1 - p0
            // (center + dir * j - reference) * dir = 0
            // j * dir * dir = (reference - p0) * dir
            // j = ((reference - p0) * dir) / (dir * dir)

            Vec2 dir = subject.capsule.p1 - subject.capsule.p0;
            float j1 = (reference - subject.center) * dir;

            float dir2 = dir * dir;

            float jclamped = dir2 > Mathf.EPS ? Mathf.ClampSymmetric(j1 / dir2, 0.5f) : 0.5f * Mathf.EpsSign(j1);
            return subject.center + jclamped * dir;
        }
        
        public static ClosestPoints Collide(CapsuleCache c0, CapsuleCache c1, Action<Vec2[]> debug=null)
        {
            if (Mathf.Abs(Vec2.Cross(c0.normal, c1.normal)) < Mathf.LARGE_EPS && !c0.circle && !c1.circle)
            {
                return CollideParallel(c0, c1);
            }
            float r0 = c0.capsule.radius;
            float r1 = c1.capsule.radius;

            Vec2 c0nearend = ClosestEnd(c0, c1);
            Vec2 c1nearend = ClosestEnd(c1, c0);
            Vec2 c0closest = ClosestAxisPoint(c0, c1nearend);
            Vec2 c1closest = ClosestAxisPoint(c1, c0nearend);

            if (debug != null)
            {
                debug(new []{c0nearend, c1nearend, c0closest, c1closest});
            }

            Vec2 dirA = c1closest - c0nearend;
            float distAsq = dirA.Len2();
            Vec2 dirB = c1nearend - c0closest;
            float distBsq = dirB.Len2();

            Vec2 contact_dir, contact_p0, contact_p1;
            float contact_distsq;

            if (distBsq < distAsq)
            {
                contact_dir = dirB;
                contact_distsq = distBsq;
                contact_p0 = c0closest;
                contact_p1 = c1nearend;
            }
            else
            {
                contact_dir = dirA;
                contact_distsq = distAsq;
                contact_p0 = c0nearend;
                contact_p1 = c1closest;
            }
            
            float dist = Mathf.Sqrt(contact_distsq);
            Vec2 n = contact_dir.SafeDivPositive(dist);
            return new ClosestPoints(
                point0: contact_p0 + r0 * n,
                point1: contact_p1 - r1 * n,
                normal: n,
                distance: dist - (r0 + r1)
            );
        }

        private static ClosestPoints CollideParallel(CapsuleCache c0, CapsuleCache c1)
        {
            var c0p0 = c0.capsule.p0;
            var c0p1 = c0.capsule.p1;
            Vec2 c1p0, c1p1;
            if (c1.normal * c0.normal >= 0)
            {
                c1p0 = c1.capsule.p0;
                c1p1 = c1.capsule.p1;
            }
            else
            {
                c1p0 = c1.capsule.p1;
                c1p1 = c1.capsule.p0;
            }
            
            Vec2 dir = -c0.normal.Rot90();
            float x00 = 0;
            float x01 = c0.length;
            float x10 = (c1p0 - c0p0) * dir;
            float x11 = (c1p1 - c0p0) * dir;

            if (x01 < x10) {
                return Circle.Collide(
                    new Circle { center = c0p1, radius = c0.capsule.radius }, 
                    new Circle { center = c1p0, radius = c1.capsule.radius }
                );
            }
            if (x11 < x00) {
                return Circle.Collide(
                    new Circle { center = c0p0, radius = c0.capsule.radius }, 
                    new Circle { center = c1p1, radius = c1.capsule.radius }
                );
            }
            
            Vec2 n = c0.normal;
            Vec2 centerDelta = c1.center - c0.center;
            float dist = centerDelta * n;
            if (dist < 0)
            {
                n = -n;
                dist = -dist;
            }

            if (dist < Mathf.EPS)
            {
                return new ClosestPoints
                {
                    degenerate = true
                };
            }
            
            float smax = Math.Min(x01, x11); 
            float smin = Math.Max(x00, x10);
            float spread = smax - smin;
            float smid = 0.5f * (smax + smin);
            
            return new ClosestPoints(
                point0: c0p0 + (c0p1 - c0p0) * (smid * c0.invLength) + n * c0.capsule.radius,
                point1: c1p0 + (c1p1 - c1p0) * ((smid - x10) * c1.invLength) - n * c1.capsule.radius,
                normal: n,
                distance: dist - (c0.capsule.radius + c1.capsule.radius)
            ) {
                spreaded = true,
                spread = spread,
            };
        }
    }
}