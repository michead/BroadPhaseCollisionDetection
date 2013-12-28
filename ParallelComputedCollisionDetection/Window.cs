#define ORTHO

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
using TK = OpenTK.Graphics.OpenGL;

namespace ParallelComputedCollisionDetection
{
    public class Window : GameWindow
    {
        #region TK.GLobal Members
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
        float[] lightAmb = { 1.0f, 1.0f, 0.0f, 1.0f };
        float[] lightSpec = { 1.0f, 1.0f, 1.0f, 1.0f };
        float[] lightDiff = { 1.0f, 1.0f, 0.0f, 1.0f };
        float[] lightPos0 = { 0.0f, 0.0f, 5.0f, 1.0f };
        IntPtr quad;
        #endregion

        public Window()
            : base(1366, 768, new GraphicsMode(32, 0, 0, 4), "Parallel Computed Collision Detection")
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

            TK.GL.ClearColor(0.0f, 0.0f, 0.0f, 0.0f);
            TK.GL.Enable(TK.EnableCap.DepthTest);
            TK.GL.Enable(TK.EnableCap.CullFace);

            TK.GL.Light(TK.LightName.Light0, TK.LightParameter.Ambient, lightAmb);
            TK.GL.Light(TK.LightName.Light0, TK.LightParameter.Specular, lightSpec);
            TK.GL.Light(TK.LightName.Light0, TK.LightParameter.Diffuse, lightDiff);
            TK.GL.Light(TK.LightName.Light0, TK.LightParameter.Position, lightPos0);
            TK.GL.Enable(TK.EnableCap.Lighting);

            TK.GL.ShadeModel(TK.ShadingModel.Smooth);

            old_mouse = OpenTK.Input.Mouse.GetState();
            old_key = OpenTK.Input.Keyboard.GetState();

            quad = Glu.NewQuadric();
        }

        protected override void OnUnload(EventArgs e)
        {
        }

        protected override void OnResize(EventArgs e)
        {
            TK.GL.Viewport(0, 0, Width, Height);

            float aspect_ratio = Width / (float)Height;
            Matrix4 perpective = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver4, aspect_ratio, 1, 64);
            TK.GL.MatrixMode(TK.MatrixMode.Projection);
            TK.GL.LoadMatrix(ref perpective);
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
            scale_factor = mouse.WheelPrecise + 7f;
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
                    offsetX -= rotation_speed;
                else if (offsetX < 0f)
                    offsetX += rotation_speed;
                if (offsetY > 0f)
                    offsetY -= rotation_speed;
                else if (offsetY < 0f)
                    offsetY += rotation_speed;
                if (offsetX  > -2f && offsetX < 2f)
                    offsetX = 0f;
                if (offsetY > -2f && offsetY < 2f)
                    offsetY = 0f;
                if (offsetX == 0f && offsetY == 0f)
                    zRot = false;
            }
            if (Keyboard[Key.Y] || yRot)
            {
                yRot = true;
                if (offsetX > 0f)
                    offsetX -= rotation_speed;
                else if (offsetX < 0f)
                    offsetX += rotation_speed;
                if (offsetY > 90f)
                    offsetY -= rotation_speed;
                else if (offsetY < 90f)
                    offsetY += rotation_speed;
                if (offsetX > -2f && offsetX < 2f)
                    offsetX = 0f;
                if (offsetY > 88f && offsetY < 92f)
                    offsetY = 90f;
                if (offsetX == 0f && offsetY == 90f)
                    yRot = false;
            }
            if (Keyboard[Key.X] || xRot)
            {
                xRot = true;
                if (offsetX > -90f)
                    offsetX -= rotation_speed;
                else if (offsetX < -90f)
                    offsetX += rotation_speed;
                if (offsetY > 0f)
                    offsetY -= rotation_speed;
                else if (offsetY < 0f)
                    offsetY += rotation_speed;
                if (offsetX < -88f && offsetX > -92f)
                    offsetX = -90f;
                if (offsetY > -2f && offsetY < 2f)
                    offsetY = 0f;
                if (offsetX == -90f && offsetY == 0f)
                    xRot = false;
            }
            #endregion
            //old_key = OpenTK.Input.Keyboard.GetState();

        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            TK.GL.Clear(TK.ClearBufferMask.DepthBufferBit | TK.ClearBufferMask.ColorBufferBit);

#if ORTHO
            modelView = Matrix4.CreateOrthographic(16, 9, 6, -6);
            TK.GL.MatrixMode(TK.MatrixMode.Projection);
            TK.GL.LoadMatrix(ref modelView);
            TK.GL.Ortho(-6, 6, -6, 6, 6, -6);
            TK.GL.Scale(scale_factor, scale_factor, scale_factor);
#else
            modelView = Matrix4.LookAt(eye, target, up);
            TK.GL.MatrixMode(TK.MatrixMode.Modelview);
            TK.GL.LoadMatrix(ref modelView);
#endif
            TK.GL.Rotate(offsetX, 0.0f, 1.0f, 0.0f);
            TK.GL.Rotate(offsetY, 1.0f, 0.0f, 0.0f);

            #region Draw Grid 3x3x3

            TK.GL.Disable(TK.EnableCap.Lighting);
            TK.GL.Disable(TK.EnableCap.Light0);

            TK.GL.Begin(TK.PrimitiveType.LineLoop);
            {
                TK.GL.Color3(1.0, 1.0, 1.0);

                TK.GL.Vertex3(-3.0, 3.0, 3.0);
                TK.GL.Vertex3(3.0, 3.0, 3.0);
                TK.GL.Vertex3(3.0, -3.0, 3.0);
                TK.GL.Vertex3(-3.0, -3.0, 3.0);
            }
            TK.GL.End();

            TK.GL.Begin(PrimitiveType.LineLoop);
            {
                TK.GL.Color3(1.0, 1.0, 1.0);

                TK.GL.Vertex3(-3.0, 3.0, 1.0);
                TK.GL.Vertex3(3.0, 3.0, 1.0);
                TK.GL.Vertex3(3.0, -3.0, 1.0);
                TK.GL.Vertex3(-3.0, -3.0, 1.0);
            }
            TK.GL.End();

            TK.GL.Begin(PrimitiveType.LineLoop);
            {
                TK.GL.Color3(1.0, 1.0, 1.0);

                TK.GL.Vertex3(-3.0, 3.0, -1.0);
                TK.GL.Vertex3(3.0, 3.0, -1.0);
                TK.GL.Vertex3(3.0, -3.0, -1.0);
                TK.GL.Vertex3(-3.0, -3.0, -1.0);
            }
            TK.GL.End();

            TK.GL.Begin(PrimitiveType.LineLoop);
            {
                TK.GL.Color3(1.0, 1.0, 1.0);

                TK.GL.Vertex3(-3.0, 3.0, -3.0);
                TK.GL.Vertex3(3.0, 3.0, -3.0);
                TK.GL.Vertex3(3.0, -3.0, -3.0);
                TK.GL.Vertex3(-3.0, -3.0, -3.0);
            }
            TK.GL.End();

            TK.GL.Begin(PrimitiveType.Lines);
            {
                TK.GL.Color3(1.0, 1.0, 1.0);

                TK.GL.Vertex3(-3.0, 3.0, 3.0);
                TK.GL.Vertex3(-3.0, 3.0, -3.0);
            }
            TK.GL.End();
            
            TK.GL.Begin(PrimitiveType.Lines);
            {
                TK.GL.Color3(1.0, 1.0, 1.0);

                TK.GL.Vertex3(3.0, 3.0, 3.0);
                TK.GL.Vertex3(3.0, 3.0, -3.0);
            }
            TK.GL.End();
            
            TK.GL.Begin(PrimitiveType.Lines);
            {
                TK.GL.Color3(1.0, 1.0, 1.0);

                TK.GL.Vertex3(3.0, -3.0, 3.0);
                TK.GL.Vertex3(3.0, -3.0, -3.0);
            }
            TK.GL.End();
            
            TK.GL.Begin(PrimitiveType.Lines);
            {
                TK.GL.Color3(1.0, 1.0, 1.0);

                TK.GL.Vertex3(-3.0, -3.0, 3.0);
                TK.GL.Vertex3(-3.0, -3.0, -3.0);
            }
            TK.GL.End();

            TK.GL.Begin(PrimitiveType.LineLoop);
            {
                TK.GL.Color3(1.0, 1.0, 1.0);

                TK.GL.Vertex3(-1.0, 3.0, 3.0);
                TK.GL.Vertex3(-1.0, 3.0, -3.0);
                TK.GL.Vertex3(-1.0, -3.0, -3.0);
                TK.GL.Vertex3(-1.0, -3.0, 3.0);
            }
            TK.GL.End();

            TK.GL.Begin(PrimitiveType.LineLoop);
            {
                TK.GL.Color3(1.0, 1.0, 1.0);

                TK.GL.Vertex3(1.0, 3.0, 3.0);
                TK.GL.Vertex3(1.0, 3.0, -3.0);
                TK.GL.Vertex3(1.0, -3.0, -3.0);
                TK.GL.Vertex3(1.0, -3.0, 3.0);
            }
            TK.GL.End();

            TK.GL.Begin(PrimitiveType.LineLoop);
            {
                TK.GL.Color3(1.0, 1.0, 1.0);

                TK.GL.Vertex3(3.0, 1.0, 3.0);
                TK.GL.Vertex3(3.0, 1.0, -3.0);
                TK.GL.Vertex3(-3.0, 1.0, -3.0);
                TK.GL.Vertex3(-3.0, 1.0, 3.0);
            }
            TK.GL.End();

            TK.GL.Begin(PrimitiveType.LineLoop);
            {
                TK.GL.Color3(1.0, 1.0, 1.0);

                TK.GL.Vertex3(3.0, -1.0, 3.0);
                TK.GL.Vertex3(3.0, -1.0, -3.0);
                TK.GL.Vertex3(-3.0, -1.0, -3.0);
                TK.GL.Vertex3(-3.0, -1.0, 3.0);
            }
            TK.GL.End();

            TK.GL.Begin(PrimitiveType.Lines);
            {
                TK.GL.Color3(1.0, 1.0, 1.0);

                TK.GL.Vertex3(-1.0, 1.0, 3.0);
                TK.GL.Vertex3(-1.0, 1.0, -3.0);
            }
            TK.GL.End();

            TK.GL.Begin(PrimitiveType.Lines);
            {
                TK.GL.Color3(1.0, 1.0, 1.0);

                TK.GL.Vertex3(1.0, 1.0, 3.0);
                TK.GL.Vertex3(1.0, 1.0, -3.0);
            }
            TK.GL.End();

            TK.GL.Begin(PrimitiveType.Lines);
            {
                TK.GL.Color3(1.0, 1.0, 1.0);

                TK.GL.Vertex3(-1.0, -1.0, 3.0);
                TK.GL.Vertex3(-1.0, -1.0, -3.0);
            }
            TK.GL.End();

            TK.GL.Begin(PrimitiveType.Lines);
            {
                TK.GL.Color3(1.0, 1.0, 1.0);

                TK.GL.Vertex3(1.0, -1.0, 3.0);
                TK.GL.Vertex3(1.0, -1.0, -3.0);
            }
            TK.GL.End();

            TK.GL.Begin(PrimitiveType.Lines);
            {
                TK.GL.Color3(1.0, 1.0, 1.0);

                TK.GL.Vertex3(-3.0, 1.0, 1.0);
                TK.GL.Vertex3(3.0, 1.0, 1.0);
            }
            TK.GL.End();

            TK.GL.Begin(PrimitiveType.Lines);
            {
                TK.GL.Color3(1.0, 1.0, 1.0);

                TK.GL.Vertex3(-3.0, 1.0, -1.0);
                TK.GL.Vertex3(3.0, 1.0, -1.0);
            }
            TK.GL.End();

            TK.GL.Begin(PrimitiveType.Lines);
            {
                TK.GL.Color3(1.0, 1.0, 1.0);

                TK.GL.Vertex3(-3.0, -1.0, 1.0);
                TK.GL.Vertex3(3.0, -1.0, 1.0);
            }
            TK.GL.End();

            TK.GL.Begin(PrimitiveType.Lines);
            {
                TK.GL.Color3(1.0, 1.0, 1.0);

                TK.GL.Vertex3(-3.0, -1.0, -1.0);
                TK.GL.Vertex3(3.0, -1.0, -1.0);
            }
            TK.GL.End();
            
            TK.GL.Begin(PrimitiveType.Lines);
            {
                TK.GL.Color3(1.0, 1.0, 1.0);

                TK.GL.Vertex3(-1.0, 3.0, 1.0);
                TK.GL.Vertex3(-1.0, -3.0, 1.0);
            }
            TK.GL.End();

            TK.GL.Begin(PrimitiveType.Lines);
            {
                TK.GL.Color3(1.0, 1.0, 1.0);

                TK.GL.Vertex3(1.0, 3.0, 1.0);
                TK.GL.Vertex3(1.0, -3.0, 1.0);
            }
            TK.GL.End();

            TK.GL.Begin(PrimitiveType.Lines);
            {
                TK.GL.Color3(1.0, 1.0, 1.0);

                TK.GL.Vertex3(-1.0, 3.0, -1.0);
                TK.GL.Vertex3(-1.0, -3.0, -1.0);
            }
            TK.GL.End();

            TK.GL.Begin(PrimitiveType.Lines);
            {
                TK.GL.Color3(1.0, 1.0, 1.0);

                TK.GL.Vertex3(1.0, 3.0, -1.0);
                TK.GL.Vertex3(1.0, -3.0, -1.0);
            }
            TK.GL.End();
            #endregion

            TK.GL.PolygonMode(TK.MaterialFace.FrontAndBack, TK.PolygonMode.Fill);

            TK.GL.Enable(TK.EnableCap.Light0);
            TK.GL.Enable(TK.EnableCap.Lighting);
            TK.GL.Translate(2f, 2f, 2f);
            Glu.Sphere(quad, 1, 50, 50);
            
            SwapBuffers();
        }
    }
}
