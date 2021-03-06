﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;

using System.Windows.Forms;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Mathematics.Interop;
using SharpDX.DirectWrite;
using Factory = SharpDX.Direct2D1.Factory;
using FontFactory = SharpDX.DirectWrite.Factory;
using Format = SharpDX.DXGI.Format;
using Font = System.Drawing.Font;
using System.Runtime.InteropServices;

namespace BrickBreaker
{
    public partial class Form1 : Form
    {
        [DllImport("user32.dll")]
        static extern short GetAsyncKeyState(int vKey);
        public static bool IsKeyPushedDown(System.Windows.Forms.Keys vKey)
        {
            return 0 != (GetAsyncKeyState((int)vKey) & 0x8000);
        }

        bool GameOver = false;
        Factory factory = null;
        FontFactory fontFactory = null;
        WindowRenderTarget device = null;
        HwndRenderTargetProperties renderProperties;
        TextFormat textFormat = null;
        TextFormat GameOverTextFormat = null;
        SolidColorBrush BackColor = null;
        Thread MainThread = null;
        bool KeepRunning = true;
        GameObjects.Wall wall = null;
        GameObjects.Bumper bumper = null;
        GameObjects.Ball ball = null;
        GameObjects.RuleSet ruleSet = null;
        Vector2 maxSpeed = new Vector2(10f,10f);
        float UpdateTime = 0;


        public Form1()
        {
            InitializeComponent();

            UpdateTime = 1f / 60f; // set Update time (60fps)
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Init factory
            factory = new Factory();
            fontFactory = new FontFactory();

            // Render settings
            renderProperties = new HwndRenderTargetProperties()
            {
                Hwnd = this.Handle,
                PixelSize = new Size2(Screen.PrimaryScreen.WorkingArea.Width,Screen.PrimaryScreen.WorkingArea.Height),
                PresentOptions = PresentOptions.Immediately
            };

            // Init device
            device = new WindowRenderTarget(factory, new RenderTargetProperties(new SharpDX.Direct2D1.PixelFormat(Format.B8G8R8A8_UNorm, AlphaMode.Premultiplied)), renderProperties);

            // Init brush
            BackColor = new SolidColorBrush(device, new Color4(255, 0, 255, 255));
            // init textformat
            textFormat = new TextFormat(fontFactory, "Verdana", 22);
            GameOverTextFormat = new TextFormat(fontFactory, "Arial", 75);
            if (this.ClientSize != Screen.PrimaryScreen.WorkingArea.Size)
                this.ClientSize = Screen.PrimaryScreen.WorkingArea.Size;
            // start main Thread.
            MainThread = new Thread(new ParameterizedThreadStart(MainFunction));
            MainThread.Start();
        }

        public void SetupGameRule()
        {
            wall = new GameObjects.Wall(16, 16, 60, 20); // Settup wall
            bumper = new GameObjects.Bumper(); // Init Bumber
            ball = new GameObjects.Ball(8);
        }

        public void MainFunction(object sender)
        {
            ruleSet = new GameObjects.RuleSet();
            SetupGameRule();
            while (KeepRunning)
            {
                device.BeginDraw();
                device.Clear(new RawColor4(0, 0, 0, 255));
                if (ruleSet.start)
                {
                    if (!GameOver)
                    {
                        MoveBall();
                        // Begin Drawing Scene;
                        wall.DrawWall(device);
                        bumper.DrawBumper(device);
                        ball.DrawBall(device, factory);
                    }
                    else
                    {
                        DrawText((Form1.ActiveForm.Width / 2) - (("GameOver!".Length/4) * GameOverTextFormat.FontSize), Form1.ActiveForm.Height / 4, "GameOver!", GameOverTextFormat);
                    }
                }
                device.EndDraw();

                
                Thread.Sleep((int)UpdateTime);
            }
        }


        int Result;
        public void MoveBall()
        {
            Form1_KeyDown(); // Get KeyPResses.
            if (ball.Speed.Y == 0)
            {
                ball.Speed.Y = maxSpeed.Y;
            }
            if (ball.Speed.Y > maxSpeed.Y)
                ball.Speed.Y = maxSpeed.Y;
            if (ball.Speed.Y < -maxSpeed.Y)
                ball.Speed.Y = -maxSpeed.Y;

            if (ball.Speed.X > maxSpeed.X)
                ball.Speed.X = maxSpeed.X;
            if (ball.Speed.X < -maxSpeed.X)
                ball.Speed.X = -maxSpeed.X;

            if (ruleSet.gravity)
                ball.Speed.Y -= 0.5f * -100f * UpdateTime * UpdateTime;
            ball.ball.X += (ball.Speed.X * UpdateTime);
            ball.ball.Y += (ball.Speed.Y * UpdateTime);

            // Check Collision;
            if (ball.Collision(bumper.Bounds, out Result))
            {
                if (ball.Speed.X == 0)
                    ball.Speed.X = maxSpeed.X;
                ball.Speed.Y *= -1f;
            }
            DrawText(0, 20, ball.Speed.ToString());
            DrawText(0, 40, ball.ball.ToString());
            DrawText(0, 60, bumper.Pos.ToString());
            DrawText(0, 80, (ball.ball.Location - bumper.Pos).ToString());

            // Check collision with wall.
            for (int i = 0; i < wall.Field.Count(); i++)
            {
                if (ball.Collision(wall.Field[i].Getbounds(), out Result))
                {
                    if (Result == 1)
                    {
                        ball.Speed.X *= -1;
                        wall.Field.RemoveAt(i);
                        break;
                    }
                    if (Result == 2)
                    {
                        ball.Speed.X *= -1;
                        wall.Field.RemoveAt(i);
                        break;
                    }
                    if (Result == 3)
                    {
                        ball.Speed.Y *= -1;
                        wall.Field.RemoveAt(i);
                        break;
                    }
                    if (Result == 4)
                    {
                        ball.Speed.Y *= -1;
                        wall.Field.RemoveAt(i);
                        break;
                    }
                }
            }
            DrawText(0, 100, Result.ToString());

            if (Form1.ActiveForm == null)
                return;
            SharpDX.RectangleF ScreenBounds = new SharpDX.RectangleF(Form1.ActiveForm.ClientRectangle.Left + 1, Form1.ActiveForm.ClientRectangle.Top + menuStrip1.Bottom, Form1.ActiveForm.ClientRectangle.Width, Form1.ActiveForm.ClientRectangle.Height);
            if (ball.ball.Left < ScreenBounds.Left)
            {
                ball.Speed.X *= -1;
                return;
            }
            if (ball.ball.Right > ScreenBounds.Right)
            {
                ball.Speed.X *= -1;
                return;
            }
            if (ball.ball.Top < ScreenBounds.Top)
            {
                ball.Speed.Y *= -1;
                return;
            }
            if (ball.ball.Bottom > ScreenBounds.Bottom)
            {
                GameOver = true;// ball fell through bottom screen.
                return;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            KeepRunning = false; // signal mainthread to stop running.
        }

        private void Form1_KeyDown()
        {
            if (IsKeyPushedDown(Keys.Left))
                bumper.AdjPos(-9);
            if (IsKeyPushedDown(Keys.Right))
                bumper.AdjPos(+9);
        }
        private void DrawText(float X, float Y, string text)
        {
            SolidColorBrush solidColorBrush = new SolidColorBrush(device,new RawColor4(255,255,255,255));
            device.DrawText(text, textFormat, new SharpDX.RectangleF(X, Y, 500, 20), solidColorBrush);
        }

        private void DrawText(float X, float Y, string text, TextFormat txtfrmt)
        {
            SolidColorBrush solidColorBrush = new SolidColorBrush(device, new RawColor4(255, 255, 255, 255));
            device.DrawText(text, txtfrmt, new SharpDX.RectangleF(X, Y, 500, 20), solidColorBrush);
        }

        private void gravityToolStripMenuItem_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem.Text == "On")
                ruleSet.gravity = true;
            else if (e.ClickedItem.Text == "Off")
                ruleSet.gravity = false;
            else if (e.ClickedItem.Text == "Start")
                ruleSet.start = true;
        }

        private void startGameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ruleSet.start = true;
        }

        private void resetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GameOver = false;
            SetupGameRule();
        }

        private void BallSpeedTextBox_TextChanged(object sender, EventArgs e)
        {
            int newspeed = 0;
            int.TryParse(BallSpeedTextBox.Text,out newspeed);
            maxSpeed = new Vector2(newspeed, newspeed);
        }
    }
}
