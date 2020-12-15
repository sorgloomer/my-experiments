using System;
using System.Drawing;
using System.Timers;
using System.Windows.Forms;
using V = TimeConstraintPhysics.Vec2;

namespace TimeConstraintPhysics
{
    
    public partial class Form1 : Form
    {
        private int frames = 500;
        private int shapes = 2;
        private V[,] pos_arr;
        private V[,] vel_arr;
        private float[] radius_arr = {40, 50};
        private float[] invmass_arr = {1.5f, 1f};
        private int frame;
        private V gravity = V.xy(0, -500f);
        private float dt = 0.05f;
        private float restitution = 0.8f;

        private V wall_point = V.zero();
        private V wall_normal = V.xy(0, 1);

        public Form1()
        {
            InitializeComponent();
            DoubleBuffered = true;
            pos_arr = new V[shapes, frames];
            pos_arr[0, 0].y = 150;
            pos_arr[1, 0].y = 160;
            vel_arr = new V[shapes, frames];
            timer1.Interval = (int) (1000 * dt);
        }

        private void timer1_Elapsed(object sender, ElapsedEventArgs e)
        {
            Calculate();
            frame = (frame + 1) % frames;
            Invalidate();
        }

        private Vec2 transform(Vec2 p, Size size)
        {
            return V.xy(
                size.Width / 2f + p.x,
                size.Height * 2f / 3f - p.y
            );
        }
        private void DrawTo(Graphics g, Size size)
        {
            float total_energy = 0; 
            g.Clear(Color.Black);
            for (var i = 0; i < shapes; i++)
            {
                var p = pos_arr[i, frame];
                var v = vel_arr[i, frame];
                var r = radius_arr[i];
                var screenp = transform(p, size);
                g.DrawArc(
                    Pens.White,
                    new RectangleF(
                        screenp.x - r,
                        screenp.y - r,
                        2 * r,
                        2 * r
                    ),
                    0,
                    360
                );
                total_energy += 0.5f * v.len2() - gravity * p;
            }

            var p0 = transform(wall_point + 500 * wall_normal.rot(), size);
            var p1 = transform(wall_point - 500 * wall_normal.rot(), size);
            g.DrawLine(Pens.White, p0.x, p0.y, p1.x, p1.y);
            g.DrawString(
                $"Energy: {total_energy:F3}",
                Font, 
                Brushes.GreenYellow, 
                10,
                10
            );
        }
        
        private void Calculate()
        {
            var frame_next = frame + 1;
            if (frame_next >= frames)
                return;
            for (var shape = 0; shape < shapes; shape++)
            {
                var radius_curr = radius_arr[shape];
                var pos_curr = pos_arr[shape, frame];
                var vel_curr = vel_arr[shape, frame];
                
                
                var vel_next = vel_curr + dt * gravity;
                var pos_next = pos_curr + (vel_next + vel_curr) * dt * 0.5f;

                var t = wall_normal * (pos_next - wall_point);
                if (t < radius_curr && vel_curr * wall_normal < 0f)
                {
                    pos_next = mirror(wall_point + radius_curr * wall_normal, wall_normal, pos_next);
                    vel_next = mirror(V.zero(), wall_normal, vel_next) * restitution;
                }

                pos_arr[shape, frame_next] = pos_next;
                vel_arr[shape, frame_next] = vel_next;
            }
        }

        private V mirror(V mirror_point, V mirror_normal, V point)
        {
            var t = mirror_normal * (point - mirror_point);
            return point - 2 * t * mirror_normal;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            DrawTo(e.Graphics, ClientSize);
        }
    }
}