using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Mathematics.Interop;
using SharpDX.DirectWrite;
using Factory = SharpDX.Direct2D1.Factory;
using FontFactory = SharpDX.DirectWrite.Factory;
using Format = SharpDX.DXGI.Format;
using Font = System.Drawing.Font;

namespace Pong
{

    static class Alg
    {
        public static int Distance(Vector2 x, Vector2 y)
        {
            Vector2 tmpDist = x - y;
            int d = (int)Math.Sqrt((double)(tmpDist.X * tmpDist.X + tmpDist.Y * tmpDist.Y));
            return d;
        }
    }

    class GameObjects
    {
        public class RuleSet
        {
            public bool start;
            public bool gravity = false;
        }
        public class Ball 
        {
            public Vector2 Speed = new Vector2(0, 0);
            public RectangleF ball = new RectangleF();
            
            public Ball(int size)
            {
                ball = new RectangleF(0, 0, size, size);
                ball.X = Form1.ActiveForm.Width / 2;
                ball.Y = Form1.ActiveForm.Height / 100 * 80;
            }

            public void DrawBall(WindowRenderTarget device,Factory factory)
            {
                RoundedRectangle roundedball = new RoundedRectangle();
                roundedball.Rect = ball;
                roundedball.RadiusX = ball.Width/ 2;
                roundedball.RadiusY = ball.Height / 2;
                device.DrawRoundedRectangle(roundedball, new SolidColorBrush(device, new RawColor4(255, 255, 255, 255)));
            }

            public bool Collision(RectangleF bounds, out int side)
            {
                side = 0;
                while (true)
                {
                    if (ball.Top > bounds.Top && ball.Bottom < bounds.Bottom) 
                    {
                        if (ball.Left < bounds.Right)
                        {
                            side = 1;
                            break;
                        }
                        if (ball.Right > bounds.Left)
                        {
                            side = 2;
                            break;
                        }
                    }
                    if (ball.Left > bounds.Left && ball.Right < bounds.Right)
                    {

                        if (ball.Top < bounds.Bottom)
                        {
                            side = 3;
                            break;
                        }
                        if (ball.Bottom > bounds.Top)
                        {
                            side = 4;
                            break;
                        }
                    }

                    // TopLeft
                    if (Alg.Distance(ball.TopLeft,bounds.BottomRight) < 2)
                    {
                        side = 5;
                        break;
                    }
                    //TopRight
                    if (Alg.Distance(ball.TopRight,bounds.BottomLeft) < 2)
                    {
                        side = 6;
                        break;
                    }
                    //BottomLeft
                    if (Alg.Distance(ball.BottomLeft,bounds.TopRight) < 2)
                    {
                        side = 7;
                        break;
                    }
                    //Bottem.Right
                    if (Alg.Distance(ball.BottomRight,bounds.TopLeft) < 2)
                    {
                        side = 8;
                        break;
                    }
                    break;
                }
                return ball.Intersects(bounds);
            }
        }

        public class block
        {
            public Vector2 Size;
            public Vector2 Pos;
            public RectangleF Bounds;
            public SharpDX.Color color;

            public block()
            {
                Size = new Vector2(30, 10);
            }

            public block(int w,int h)
            {
                Size = new Vector2(w, h);
            }

            public RectangleF Getbounds()
            {
                return new RectangleF(Pos.X, Pos.Y, Size.X, Size.Y);
            }
        }

        public class Bumper :  block
        {
            public Bumper()
            {
                this.Size = new Vector2(100, 10);
                this.Pos = new Vector2(Form1.ActiveForm.ClientSize.Width / 2 - Size.X / 2, Form1.ActiveForm.ClientSize.Height - 8);
                this.Bounds = new RectangleF(Pos.X, Pos.Y, Size.X, Size.Y);
            }

            public void DrawBumper(WindowRenderTarget device)
            {
                this.Bounds = new RectangleF(Pos.X, Pos.Y, Size.X, Size.Y);
                device.DrawRectangle(this.Bounds, new SolidColorBrush(device, new RawColor4(0, 255, 0, 255)));
            }

            public void AdjPos(int x)
            {
                this.Pos.X += x;
            }
        }

        public class Wall
        {
            public List<block> Field = new List<block>();
            int spacing = 0;
            public Wall(int c,int r, int w, int h)
            {
                Field.Capacity = r * c;

                Vector2 CenterScreen = new Vector2((Form1.ActiveForm.ClientRectangle.Width / 2) - (c/2 * w + (c/2 * spacing) / 2),
                    (Form1.ActiveForm.ClientRectangle.Height / 4) - (r/4 * h + (r/4 * spacing) / 2));

                for (int i = 1; i < r;i++) // Rows
                {
                    for (int j = 1;j < c;j++) // Columns
                    {
                        block brick = new block(w,h);
                        brick.Pos.X = CenterScreen.X + (brick.Size.X * j) + (spacing * j);
                        brick.Pos.Y = CenterScreen.Y + (brick.Size.Y * i) + (spacing * i);
                        Field.Add(brick);
                    }
                }
            }

            public void DrawWall(SharpDX.Direct2D1.WindowRenderTarget device)
            {
                foreach (block brick in Field)
                {
                    device.StrokeWidth = 1.0f;
                    device.DrawRectangle(new RawRectangleF(brick.Pos.X, brick.Pos.Y, brick.Pos.X + brick.Size.X, brick.Pos.Y + brick.Size.Y), new SolidColorBrush(device, new RawColor4(255, 0, 0, 255)));
                }
            }
        }
    }
}
