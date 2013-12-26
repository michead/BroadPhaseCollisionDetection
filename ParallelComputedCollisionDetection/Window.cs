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
        int oldX, oldY;
        float oldWheel;
        float offsetX = 0f, offsetY = 0f;
        Vector3 eye, target, up;
        float mouse_sensitivity=0.2f;
        Matrix4 modelView;
        #endregion

        public Window()
            : base(1366, 768, GraphicsMode.Default, "Parallel Computed Collision Detection")
        {
            WindowState = WindowState.Fullscreen;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            eye = new Vector3(0, 0, 7);
            target = new Vector3(0, 0, 0);
            up = new Vector3(0, 1, 0);

            GL.ClearColor(0.0f, 0.0f, 0.0f, 0.0f);

            oldX = OpenTK.Input.Mouse.GetState().X;
            oldY = OpenTK.Input.Mouse.GetState().Y;
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
        }

        void checkMouseInput()
        {
            var mouse = OpenTK.Input.Mouse.GetState();
            if (mouse.IsButtonDown(MouseButton.Right))
            {
                CursorVisible = false;
                offsetX += (mouse.X - oldX) * mouse_sensitivity;
                offsetY += (mouse.Y - oldY) * mouse_sensitivity;
            }
            else
                CursorVisible = true;

            oldX = mouse.X;
            oldY = mouse.Y;
            eye.Z += (oldWheel - mouse.WheelPrecise);
            if (eye.Z < 1)
                eye.Z = 1;
            else if (eye.Z > 15)
                eye.Z = 15;
            oldWheel = mouse.WheelPrecise;
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit);
            modelView = Matrix4.LookAt(eye, target, up);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref modelView);

            GL.Rotate(offsetX, 0.0f, 1.0f, 0.0f);
            GL.Rotate(offsetY, 1.0f, 0.0f, 0.0f);

            #region Drawing Primitives
            GL.Begin(PrimitiveType.LineLoop);
            {
                GL.Color3(1.0, 1.0, 1.0);

                GL.Vertex3(-1.0, 1.0, 1.0);
                GL.Vertex3(1.0, 1.0, 1.0);
                GL.Vertex3(1.0, -1.0, 1.0);
                GL.Vertex3(-1.0, -1.0, 1.0);
            }
            GL.End();

            GL.Begin(PrimitiveType.LineLoop);
            {
                GL.Color3(1.0, 1.0, 1.0);

                GL.Vertex3(-1.0, 1.0, -1.0);
                GL.Vertex3(1.0, 1.0, -1.0);
                GL.Vertex3(1.0, -1.0, -1.0);
                GL.Vertex3(-1.0, -1.0, -1.0);
            }
            GL.End();

            GL.Begin(PrimitiveType.Lines);
            {
                GL.Color3(1.0, 1.0, 1.0);

                GL.Vertex3(-1.0, 1.0, 1.0);
                GL.Vertex3(-1.0, 1.0, -1.0);
            }
            GL.End();
            
            GL.Begin(PrimitiveType.Lines);
            {
                GL.Color3(1.0, 1.0, 1.0);

                GL.Vertex3(1.0, 1.0, 1.0);
                GL.Vertex3(1.0, 1.0, -1.0);
            }
            GL.End();
            
            GL.Begin(PrimitiveType.Lines);
            {
                GL.Color3(1.0, 1.0, 1.0);

                GL.Vertex3(1.0, -1.0, 1.0);
                GL.Vertex3(1.0, -1.0, -1.0);
            }
            GL.End();
            
            GL.Begin(PrimitiveType.Lines);
            {
                GL.Color3(1.0, 1.0, 1.0);

                GL.Vertex3(-1.0, -1.0, 1.0);
                GL.Vertex3(-1.0, -1.0, -1.0);
            }
            GL.End();
            #endregion
            
            SwapBuffers();

        }
    }
}
