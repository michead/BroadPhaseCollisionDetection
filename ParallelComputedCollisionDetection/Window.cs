//#define ORTHO

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System.ComponentModel;
using System.Data;
using System.Windows.Forms;
using OpenTK.Platform;
using System.Diagnostics;

namespace ParallelComputedCollisionDetection
{
    public class Window : GameWindow
    {
        #region Global Members
        MouseState old_mouse;
        float offsetX = 0f, offsetY = 0f;
        Vector3 eye, target, up;
        float mouse_sensitivity=0.2f;
        Matrix4 modelView;
        float scale_factor;
        float rotation_speed = 2f;
        KeyboardState old_key;
        bool xRot;
        bool yRot;
        bool zRot;
        #endregion

        public Window()
            : base(1366, 768, GraphicsMode.Default, "Parallel Computed Collision Detection")
        {
            WindowState = WindowState.Fullscreen;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            VSync = VSyncMode.On;
            eye = new Vector3(0, 0, 14);
            target = new Vector3(0, 0, 0);
            up = new Vector3(0, 1, 0);

            GL.ClearColor(0.0f, 0.0f, 0.0f, 0.0f);

            old_mouse = OpenTK.Input.Mouse.GetState();
            old_key = OpenTK.Input.Keyboard.GetState();
        }

        protected override void OnUnload(EventArgs e)
        {
        }

        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(0, 0, Width, Height);

            float aspect_ratio = Width / (float)Height;
            Matrix4 perpective = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver4, aspect_ratio, 1, 64);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref perpective);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            if (Keyboard[OpenTK.Input.Key.Escape])
                this.Exit();

            if (Keyboard[OpenTK.Input.Key.Space])
                if (WindowState != WindowState.Fullscreen)
                    WindowState = WindowState.Fullscreen;
                else
                    WindowState = WindowState.Maximized;
            checkMouseInput();
            checkKeyboardInput();
        }

        void checkMouseInput()
        {
            var mouse = OpenTK.Input.Mouse.GetState();
            if (mouse.IsButtonDown(MouseButton.Right))
            {
                CursorVisible = false;
                offsetX += (mouse.X - old_mouse.X) * mouse_sensitivity;
                offsetY += (mouse.Y - old_mouse.Y) * mouse_sensitivity;
            }
            else
                CursorVisible = true;
#if ORTHO
            scale_factor = mouse.WheelPrecise + 10f;
            if(scale_factor > 20f)
                scale_factor=20f;
            if(scale_factor < 1f)
                scale_factor=1f;
#else
            eye.Z -= (mouse.WheelPrecise - old_mouse.WheelPrecise);
            if (eye.Z < 3)
                eye.Z = 3;
#endif
            old_mouse = mouse;
        }

        void checkKeyboardInput()
        {
            if (Keyboard[Key.Left])
                offsetX -= rotation_speed;
            if (Keyboard[Key.Right])
                offsetX += rotation_speed;
            if (Keyboard[Key.Up])
                offsetY += rotation_speed;
            if (Keyboard[Key.Down])
                offsetY -= rotation_speed;

            #region XYZ Grid Rotations
            if (Keyboard[Key.Z] || zRot)
            {
                zRot = true;
                if (offsetX > 0f)
                    offsetX--;
                else if (offsetX < 0f)
                    offsetX++;
                if (offsetY > 0f)
                    offsetY--;
                else if (offsetY < 0f)
                    offsetY++;
                if ((offsetX  >-1f && offsetX < 1f) && (offsetY > -1f && offsetY < 1f))
                {
                    offsetX = 0f;
                    offsetY = 0f;
                    zRot = false;
                }
                
            }
            if (Keyboard[Key.Y] || yRot)
            {
                yRot = true;
                if (offsetX > 0f)
                    offsetX--;
                else if (offsetX < 0f)
                    offsetX++;
                if (offsetY > 90f)
                    offsetY--;
                else if (offsetY < 90f)
                    offsetY++;
                if ((offsetX > -1f && offsetX < 1f) && (offsetY > 89f && offsetY < 91f))
                {
                    offsetX = 0f;
                    offsetY = 90f;
                    yRot = false;
                }
            }
            if (Keyboard[Key.X] || xRot)
            {
                xRot = true;
                if (offsetX > 90f)
                    offsetX--;
                else if (offsetX < 90f)
                    offsetX++;
                if (offsetY > 0f)
                    offsetY--;
                else if (offsetY < 0f)
                    offsetY++;
                if ((offsetX > 89f && offsetX < 91f) && (offsetY > -1f && offsetY < 1f))
                {
                    offsetX = 90f;
                    offsetY = 0f;
                    xRot = false;
                }
            }
            #endregion
            //old_key = OpenTK.Input.Keyboard.GetState();

        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit);

#if ORTHO
            modelView = Matrix4.CreateOrthographic(16, 9, 3, -3);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref modelView);
            GL.Ortho(-10, 10, -10, 10, 20, -20);
            GL.Scale(scale_factor, scale_factor, scale_factor);
#else
            modelView = Matrix4.LookAt(eye, target, up);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref modelView);
#endif

            GL.Rotate(offsetX, 0.0f, 1.0f, 0.0f);
            GL.Rotate(offsetY, 1.0f, 0.0f, 0.0f);

            #region Draw Grid 3x3x3
            GL.Begin(PrimitiveType.LineLoop);
            {
                GL.Color3(1.0, 1.0, 1.0);

                GL.Vertex3(-3.0, 3.0, 3.0);
                GL.Vertex3(3.0, 3.0, 3.0);
                GL.Vertex3(3.0, -3.0, 3.0);
                GL.Vertex3(-3.0, -3.0, 3.0);
            }
            GL.End();

            GL.Begin(PrimitiveType.LineLoop);
            {
                GL.Color3(1.0, 1.0, 1.0);

                GL.Vertex3(-3.0, 3.0, 1.0);
                GL.Vertex3(3.0, 3.0, 1.0);
                GL.Vertex3(3.0, -3.0, 1.0);
                GL.Vertex3(-3.0, -3.0, 1.0);
            }
            GL.End();

            GL.Begin(PrimitiveType.LineLoop);
            {
                GL.Color3(1.0, 1.0, 1.0);

                GL.Vertex3(-3.0, 3.0, -1.0);
                GL.Vertex3(3.0, 3.0, -1.0);
                GL.Vertex3(3.0, -3.0, -1.0);
                GL.Vertex3(-3.0, -3.0, -1.0);
            }
            GL.End();

            GL.Begin(PrimitiveType.LineLoop);
            {
                GL.Color3(1.0, 1.0, 1.0);

                GL.Vertex3(-3.0, 3.0, -3.0);
                GL.Vertex3(3.0, 3.0, -3.0);
                GL.Vertex3(3.0, -3.0, -3.0);
                GL.Vertex3(-3.0, -3.0, -3.0);
            }
            GL.End();

            GL.Begin(PrimitiveType.Lines);
            {
                GL.Color3(1.0, 1.0, 1.0);

                GL.Vertex3(-3.0, 3.0, 3.0);
                GL.Vertex3(-3.0, 3.0, -3.0);
            }
            GL.End();
            
            GL.Begin(PrimitiveType.Lines);
            {
                GL.Color3(1.0, 1.0, 1.0);

                GL.Vertex3(3.0, 3.0, 3.0);
                GL.Vertex3(3.0, 3.0, -3.0);
            }
            GL.End();
            
            GL.Begin(PrimitiveType.Lines);
            {
                GL.Color3(1.0, 1.0, 1.0);

                GL.Vertex3(3.0, -3.0, 3.0);
                GL.Vertex3(3.0, -3.0, -3.0);
            }
            GL.End();
            
            GL.Begin(PrimitiveType.Lines);
            {
                GL.Color3(1.0, 1.0, 1.0);

                GL.Vertex3(-3.0, -3.0, 3.0);
                GL.Vertex3(-3.0, -3.0, -3.0);
            }
            GL.End();

            GL.Begin(PrimitiveType.LineLoop);
            {
                GL.Color3(1.0, 1.0, 1.0);

                GL.Vertex3(-1.0, 3.0, 3.0);
                GL.Vertex3(-1.0, 3.0, -3.0);
                GL.Vertex3(-1.0, -3.0, -3.0);
                GL.Vertex3(-1.0, -3.0, 3.0);
            }
            GL.End();

            GL.Begin(PrimitiveType.LineLoop);
            {
                GL.Color3(1.0, 1.0, 1.0);

                GL.Vertex3(1.0, 3.0, 3.0);
                GL.Vertex3(1.0, 3.0, -3.0);
                GL.Vertex3(1.0, -3.0, -3.0);
                GL.Vertex3(1.0, -3.0, 3.0);
            }
            GL.End();

            GL.Begin(PrimitiveType.LineLoop);
            {
                GL.Color3(1.0, 1.0, 1.0);

                GL.Vertex3(3.0, 1.0, 3.0);
                GL.Vertex3(3.0, 1.0, -3.0);
                GL.Vertex3(-3.0, 1.0, -3.0);
                GL.Vertex3(-3.0, 1.0, 3.0);
            }
            GL.End();

            GL.Begin(PrimitiveType.LineLoop);
            {
                GL.Color3(1.0, 1.0, 1.0);

                GL.Vertex3(3.0, -1.0, 3.0);
                GL.Vertex3(3.0, -1.0, -3.0);
                GL.Vertex3(-3.0, -1.0, -3.0);
                GL.Vertex3(-3.0, -1.0, 3.0);
            }
            GL.End();

            GL.Begin(PrimitiveType.Lines);
            {
                GL.Color3(1.0, 1.0, 1.0);

                GL.Vertex3(-1.0, 1.0, 3.0);
                GL.Vertex3(-1.0, 1.0, -3.0);
            }
            GL.End();

            GL.Begin(PrimitiveType.Lines);
            {
                GL.Color3(1.0, 1.0, 1.0);

                GL.Vertex3(1.0, 1.0, 3.0);
                GL.Vertex3(1.0, 1.0, -3.0);
            }
            GL.End();

            GL.Begin(PrimitiveType.Lines);
            {
                GL.Color3(1.0, 1.0, 1.0);

                GL.Vertex3(-1.0, -1.0, 3.0);
                GL.Vertex3(-1.0, -1.0, -3.0);
            }
            GL.End();

            GL.Begin(PrimitiveType.Lines);
            {
                GL.Color3(1.0, 1.0, 1.0);

                GL.Vertex3(1.0, -1.0, 3.0);
                GL.Vertex3(1.0, -1.0, -3.0);
            }
            GL.End();

            GL.Begin(PrimitiveType.Lines);
            {
                GL.Color3(1.0, 1.0, 1.0);

                GL.Vertex3(-3.0, 1.0, 1.0);
                GL.Vertex3(3.0, 1.0, 1.0);
            }
            GL.End();

            GL.Begin(PrimitiveType.Lines);
            {
                GL.Color3(1.0, 1.0, 1.0);

                GL.Vertex3(-3.0, 1.0, -1.0);
                GL.Vertex3(3.0, 1.0, -1.0);
            }
            GL.End();

            GL.Begin(PrimitiveType.Lines);
            {
                GL.Color3(1.0, 1.0, 1.0);

                GL.Vertex3(-3.0, -1.0, 1.0);
                GL.Vertex3(3.0, -1.0, 1.0);
            }
            GL.End();

            GL.Begin(PrimitiveType.Lines);
            {
                GL.Color3(1.0, 1.0, 1.0);

                GL.Vertex3(-3.0, -1.0, -1.0);
                GL.Vertex3(3.0, -1.0, -1.0);
            }
            GL.End();
            #endregion
            
            SwapBuffers();

        }
    }
}
