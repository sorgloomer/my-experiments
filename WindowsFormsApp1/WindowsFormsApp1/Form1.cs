﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Timers;
using System.Windows.Forms;
using WindowsFormsApp1.PhysicsEngine;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        private Bitmap _bitmap;
        private Vec2 _viewMouse, _worldMouse, _p1;
        private Graphics _graphics;
        
        private Stopwatch stopwatch = Stopwatch.StartNew();
        private int fpsCounter, fps;
        private TimeSpan lastFpsTurnover;
        
        private World _world = new World();
        private bool drawflag = true;

        private AabbTreeNode _hoveredNode = null;
        public Form1()
        {
            InitializeComponent();
            DoubleBuffered = true;
            UpdateTreeBalancingSetting();
        }

        private bool DebugDrawContacts => cbDrawContacts.Checked;
        private bool DrawAabbTreeChecked => cbDrawAabbTree.Checked;
        private bool DrawAabbBoxesChecked => cbDrawAabbBoxes.Checked;


        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            _worldMouse = new Vec2(e.X - 800f, e.Y - 600) * (1/0.4f); // TODO
            _viewMouse = new Vec2(e.X, e.Y);
            if (e.Button == MouseButtons.Left)
            {
                _p1 = _worldMouse;
            }
        }

        private void timer1_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (drawflag)
            {
                UpdatePhysics();
                TriggerDraw();
                UpdateFpsCounter();
                drawflag = false;
            }
            Invalidate();
        }

        private void UpdatePhysics()
        {
            var b = _world.Bodies.Last();
            b.position = _worldMouse;
            b.positionVelocity = Vec2.Zero;
            b.invMass = 0;
            _world.Step(0.02f);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (_bitmap != null)
            {
                e.Graphics.DrawImage(_bitmap, 0, 0);
            }

            drawflag = true;
            base.OnPaint(e);
        }

        private void TriggerDraw()
        {
            EnsureBitmapSize();
            if (_graphics != null && _bitmap != null)
            {
                DrawScene();
            }
        }

        private void DrawHoveredRect()
        {
            var node = _hoveredNode;
            if (node != null)
            {
                DrawNodeRect(node, Pens.Gray);
            }
        }

        private void DrawScene()
        {
            _graphics.ResetTransform();
            _graphics.Clear(Color.Black);

            
            _graphics.TranslateTransform(800, 500);
            _graphics.ScaleTransform(0.4f, 0.4f);
            var numratio = DrawRects();
            var bodyi = 0;
            foreach (var body in _world.Bodies)
            {
                Pen color = body.type == BodyType.Dynamic ? Pens.Aqua : Pens.White; 
                DrawCapsule(body.fixtureCache, color);

                if (DebugDrawContacts) {
                    foreach (var contact in _world.contacts)
                    {
                        DrawLine(contact.tag == 1 ? Pens.Red : Pens.Orange, contact.position, contact.position + contact.normal * 10);
                    }
                    foreach (var notable in _world.notables)
                    {
                        _graphics.DrawArc(Pens.Purple, notable.x - 5, notable.y - 5, 10, 10, 0, 360);
                    }
                }
            }
            DrawHoveredRect();
            _graphics.ResetTransform();

            if (DrawAabbTreeChecked)
            {
                DrawAabbTree();
            }
            _graphics.DrawString($"{fps:F0} FPS", Font, Brushes.White, 5, 5);
            _graphics.DrawString($"AABB: {numratio} / {_world.tree.root.depth}", Font, Brushes.White, 5, 20);
        }

        private void DrawAabbTree()
        {
            var treeRootDrawParams = new DrawnParams
            {
                position = new Vec2((ClientSize.Width + 225) / 2, 10),
                layersize = new Vec2(ClientSize.Width - 250, 15f),
            };
            _hoveredNode = null;
            DrawTree(_world.tree.root, treeRootDrawParams, treeRootDrawParams);
        }

        private int DrawRects()
        {
            if (DrawAabbBoxesChecked) {
                DrawRects(_world.tree.root);
            }
            var list = new List<AabbTreeNode>();
            _world.tree.PossibleCollisions(_world.tree.GetBodyFitRect(_world.tree.bodies.Last()), null, list);

            if (DrawAabbBoxesChecked) {
                foreach (var n in list)
                {
                    DrawNodeRect(n, Pens.Red);
                }
            }
            return list.Count;
        }
        private void DrawRects(AabbTreeNode node)
        {
            if (node == null) return;
            if (node.leaf)
            {
                DrawNodeRect(node, Pens.Gray);
                return;
            }
            DrawNodeRect(node, Pens.DarkSlateGray);
            foreach (var c in node.children)
            {
                DrawRects(c);
            }
        }
        private void DrawTree(AabbTreeNode node, DrawnParams drawTarget, DrawnParams drawActual)
        {
            if (node == null) return;
            if (node.leaf)
            {
                return;
            }


            var newDrawActual = drawTarget;
            if (node.drawn.HasValue)
            {
                float spd = ClientSize.Width / 50f;
                newDrawActual = node.drawn.Value.Approach(spd, 2 * spd, drawTarget);
            }
            node.drawn = newDrawActual;


            int i = 0;
            foreach (var child in node.children)
            {
                var childDrawActual = ComputeChildDrawLocation(node, i, newDrawActual);
                var pen = Pens.Green;
                if (_hoveredNode == null && Vec2.Dist(childDrawActual.position, _viewMouse) < 10)
                {
                    _hoveredNode = child;
                    pen = Pens.Red;
                }
                _graphics.DrawLine(
                    pen,
                    newDrawActual.position.x,
                    newDrawActual.position.y,
                    childDrawActual.position.x,
                    childDrawActual.position.y
                );
                if (!child.leaf)
                {
                    var childDrawTarget = ComputeChildDrawLocation(node, i, drawTarget);
                    DrawTree(child, childDrawTarget, childDrawActual);
                }
                i++;
            }
        }

        private static DrawnParams ComputeChildDrawLocation(AabbTreeNode node, int childIndex, DrawnParams parentDrawParams)
        {
            float temp = (childIndex + 1) / (float) (node.children.Count + 1) - 0.5f;
            return new DrawnParams
            {
                position = parentDrawParams.position + new Vec2(parentDrawParams.layersize.x * temp, parentDrawParams.layersize.y),
                layersize =new Vec2(parentDrawParams.layersize.x * (1f / node.children.Count), parentDrawParams.layersize.y),
            };
        }

        private void DrawNodeRect(AabbTreeNode node, Pen pen)
        {
            _graphics.DrawRectangle(
                pen,
                node.bounds.min.x,
                node.bounds.min.y,
                node.bounds.max.x - node.bounds.min.x,
                node.bounds.max.y - node.bounds.min.y
            );
        }

        private Color TreeColor(AabbTreeNode node)
        {
            var x = 200 - node.depth * 20;
            return Color.FromArgb(255, x, x, x);
        }

        private void UpdateFpsCounter()
        {
            fpsCounter++;
            var current = stopwatch.Elapsed;
            if (current - lastFpsTurnover >= TimeSpan.FromSeconds(1))
            {
                lastFpsTurnover = current;
                fps = fpsCounter;
                fpsCounter = 0;
            }

            foreach (var body in _world.Bodies)
            {
                if (body.position.y > 1000)
                {
                    body.position = new Vec2(0, -150);
                    body.positionVelocity = Vec2.Y * 500;
                }
            }
        }

        private void DrawCapsule(CapsuleCache c0, Pen p)
        {
            var n = c0.normal;
            var r = c0.capsule.radius;
            var twor = 2 * r;
            var x = r * n;
            var p0 = c0.capsule.p0;
            var p1 = c0.capsule.p1;
            DrawLine(p, p0 + x, p1 + x);
            DrawLine(p, p0 - x, p1 - x);
            float angle = Mathf.RAD_TO_DEG * (float)Math.Atan2(n.y, n.x);
            _graphics.DrawArc(p, p0.x - r, p0.y - r, twor, twor, angle, 180);
            _graphics.DrawArc(p, p1.x - r, p1.y - r, twor, twor, angle + 180, 180);
        }

        private void DrawLine(Pen p, Vec2 l0, Vec2 l1)
        {
            _graphics.DrawLine(p, l0.x, l0.y, l1.x, l1.y);
        }


        private void EnsureBitmapSize()
        {
            if (_bitmap is null || _bitmap.Size != ClientSize)
            {
                _graphics?.Dispose();
                if (ClientSize.Height <= 0 || ClientSize.Width <= 0)
                {
                    _bitmap = null;
                    _graphics = null;
                }
                else
                {
                    _bitmap = new Bitmap(ClientSize.Width, ClientSize.Height);
                    _graphics = Graphics.FromImage(_bitmap);
                }
            }
        }

        private void cbEnableRotations_CheckedChanged(object sender, EventArgs e)
        {
            UpdateTreeBalancingSetting();
        }

        private void UpdateTreeBalancingSetting()
        {
            _world.tree.balanceEnableRotations = cbEnableRotations.Checked;
        }
    }
}
