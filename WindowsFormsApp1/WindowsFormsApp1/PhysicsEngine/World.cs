﻿using System.Collections.Generic;

namespace WindowsFormsApp1.PhysicsEngine
{
    public class World
    {
        public AabbTree tree;
        public List<ContactHalf> contacts = new List<ContactHalf>();
        public List<Vec2> notables = new List<Vec2>();
        private int substeps = 5;
        private Vec2 gravity = new Vec2(0, 200);
        private int PARTICLE_COUNT = 500;
        public float positionDamping = 0.20f;
        public float angularDamping = 0.20f;
        public float maxPositionVelocity = 1000f;
        public float maxAngularVelocity = 40 * Mathf.TWO_PI;

        public void Step(float full_dt)
        {
            var dt = full_dt / substeps;
            var gravity_dp = gravity * Mathf.Sq(dt);

            var positionDampCoeff = Mathf.Pow(1f - positionDamping, dt);
            var angularDampCoeff = Mathf.Pow(1f - angularDamping, dt);

            tree.Rebuild();
            foreach (var body in tree.bodies)
            {
                body.positionVelocity = body.positionVelocity.Clamp(maxPositionVelocity);
                body.angularVelocity = Mathf.ClampSymmetric(body.angularVelocity, maxAngularVelocity);
                body.lastPosition = body.position - body.positionVelocity * dt;
                body.lastAngle = body.angle - body.angularVelocity * dt;
            }
            for (var substep = 0; substep < substeps; substep++)
            {
                foreach (var body in tree.bodies)
                {
                    if (body.type == BodyType.Dynamic) {
                        body.position += gravity_dp;
                        var positionDelta = body.position - body.lastPosition;
                        body.lastPosition = body.position;
                        body.position += positionDelta * positionDampCoeff;
                        var angleDelta = body.angle - body.lastAngle;
                        body.lastAngle = body.angle;
                        body.angle += angleDelta * angularDampCoeff;
                        var angleWrapper = DiffToMod(body.angle, Mathf.TWO_PI);
                        body.angle += angleWrapper;
                        body.lastAngle += angleWrapper;
                    }
                    UpdateBodyCache(body);
                }

                foreach (var body in tree.bodies)
                {
                    tree.AddOrUpdate(body);
                }

                contacts.Clear();
                notables.Clear();

                foreach (var body0 in tree.bodies)
                {
                    tree.PossibleCollisions(tree.rects.GetBodyFitRect(body0), body1 =>
                    {
                        if (body0 == body1) return;
                        ClosestPoints p = CapsuleCache.Collide(
                            body0.fixtureCache,
                            body1.fixtureCache,
                            vs => notables.AddRange(vs)
                        );

                        if (!p.degenerate)
                        {
                            StoreContact(body0, body1, p);
                            if (p.distance < 0)
                            {
                                CorrectContact(body0, body1, p);
                                UpdateBodyCache(body0);
                                UpdateBodyCache(body1);
                            }
                        }
                    }, null);
                }
                foreach (var body in tree.bodies)
                {
                    tree.AddOrUpdate(body);
                }
            }
            foreach (var body in tree.bodies)
            {
                var invDt = 1f / dt;
                body.positionVelocity = (body.position - body.lastPosition) * invDt;
                body.angularVelocity = (body.angle - body.lastAngle) * invDt;
            }
        }
        
        private void UpdateBodyCache(Rigidbody body)
        {
            body.transform = Transform.Translation(body.position) * Transform.Rotation(body.angle);
            body.fixtureCache = new CapsuleCache(body.fixture.TranslateAndRotate(body.transform));
        }

        private void CorrectContact(Rigidbody body0, Rigidbody body1, ClosestPoints cp)
        {
            var n = cp.normal;
            var r0 = cp.point0 - body0.position;
            var r1 = cp.point1 - body1.position;
            var invVirtualMass = (
                body0.invMass
                + body1.invMass
                + body0.invAngularMass * Mathf.Sq(r0 % n) 
                + body1.invAngularMass * Mathf.Sq(r1 % n)
            );
            if (invVirtualMass < Mathf.EPS)
            {
                // Probably two static bodies or something not correctable
                return;
            }
            var penetration = -cp.distance;
            var dti = penetration / invVirtualMass;

            body0.position -= n * (dti * body0.invMass);
            body1.position += n * (dti * body1.invMass);
            body0.angle -= (r0 % n) * (dti * body0.invAngularMass);
            body1.angle += (r1 % n) * (dti * body1.invAngularMass);
        }

        private void StoreContact(Rigidbody body0, Rigidbody body1, ClosestPoints p)
        {
            if (p.spreaded)
            {
                Vec2 trans = p.normal.Rot90() * (p.spread * 0.5f); 
                contacts.Add(new ContactHalf()
                {
                    body0 = body0,
                    body1 = body1,
                    normal = p.normal,
                    tag = 1,
                    penetration = -p.distance,
                    position = p.point0 + trans,
                });
                contacts.Add(new ContactHalf()
                {
                    body0 = body1,
                    body1 = body0,
                    normal = -p.normal,
                    tag = 2,
                    penetration = -p.distance,
                    position = p.point1 + trans,
                });
                contacts.Add(new ContactHalf()
                {
                    body0 = body0,
                    body1 = body1,
                    normal = p.normal,
                    tag = 1,
                    penetration = -p.distance,
                    position = p.point0 - trans,
                });
                contacts.Add(new ContactHalf()
                {
                    body0 = body1,
                    body1 = body0,
                    normal = -p.normal,
                    tag = 2,
                    penetration = -p.distance,
                    position = p.point1 - trans,
                });
            }
            contacts.Add(new ContactHalf()
            {
                body0 = body0,
                body1 = body1,
                normal = p.normal,
                tag = 1,
                penetration = -p.distance,
                position = p.point0,
            });
            contacts.Add(new ContactHalf()
            {
                body0 = body1,
                body1 = body0,
                normal = -p.normal,
                tag = 2,
                penetration = -p.distance,
                position = p.point1,
            });
        }

        public static float DiffToMod(float a, float b)
        {
            return -Mathf.Floor(a / b) * b;
        }
        public World()
        {
            var bodies = new List<Rigidbody>()
            {
                new Rigidbody()
                {
                    fixture = new Capsule()
                    {
                        p0 = new Vec2(-1900, -300), 
                        p1 = new Vec2(0, 300),
                        radius = 10,
                    },
                },
                new Rigidbody()
                {
                    fixture = new Capsule()
                    {
                        p0 = new Vec2(1900, -300), 
                        p1 = new Vec2(0, 300),
                        radius = 10,
                    },
                },
            };
            for (var i = 0; i < PARTICLE_COUNT; i++)
            {
                bodies.Add(new Rigidbody {
                    type = BodyType.Dynamic,
                    position = new Vec2(-50 + 15 * (i % 8), 0 - 5 * i),
                    fixture = new Capsule()
                    {
                        p0 = new Vec2(-4, 0), 
                        p1 = new Vec2(4, 0),
                        radius = 4,
                    },
                    invMass = 1,
                    invAngularMass = 0.03f,
                    angle = 1,
                });
            }

            bodies[bodies.Count - 2].invMass *= 0.1f;
            bodies[bodies.Count - 2].invAngularMass *= 0.1f;
            bodies[bodies.Count - 2].fixture.radius *= 2f;
            bodies[bodies.Count - 2].fixture.p0 *= 2f;
            bodies[bodies.Count - 2].fixture.p1 *= 2f;
            
            bodies[bodies.Count - 1].invMass *= 0f;
            bodies[bodies.Count - 1].invAngularMass *= 0.01f;
            bodies[bodies.Count - 1].fixture.radius *= 3f;
            bodies[bodies.Count - 1].fixture.p0 *= 8f;
            bodies[bodies.Count - 1].fixture.p1 *= 8f;
            
            tree = new AabbTree();
            var counter = 0;
            bodies.ForEach(b => b.index = counter++);
            tree.AddOrUpdateRange(bodies);
        }
        
        public IEnumerable<Rigidbody> Bodies => tree.bodies;
    }
}