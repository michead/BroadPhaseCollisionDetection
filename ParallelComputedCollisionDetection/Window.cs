#define ORTHO

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;
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
using GL = OpenTK.Graphics.OpenGL.GL;

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
        float scale_factor = 1;
        float rotation_speed = 2;
        float fov;
        public int sphere_precision = 30;
        float wp_scale_factor = 3;
        KeyboardState old_key;
        bool xRot;
        bool yRot;
        bool zRot;
        float[] mat_specular = { 1.0f, 1.0f, 1.0f, 1.0f };
        float[] mat_shininess = { 50.0f };
        float[] light_position = { 1.0f, 1.0f, 1.0f, 0.0f };
        float[] light_ambient = { 0.5f, 0.5f, 0.5f, 1.0f };
        float[][] colors = new float[][]{   new float[]{1f, 0f, 0f, 0.0f}, new float[]{0f, 1f, 0f, 0.0f}, new float[]{0f, 0f, 1f, 0.0f},
                                            new float[]{1f, 1f, 1f, 0.0f}, new float[]{1f, 1f, 0f, 0.0f}, new float[]{1f, 0f, 1f, 0.0f},
                                            new float[]{1f, 1f, 0f, 0.0f}, new float[]{0.4f, 0.6f, 0.4f, 0.0f}, new float[]{0f, 1f, 1f, 0.0f},
                                            new float[]{1f, 0f, 1f, 0.0f}, new float[]{0f, 1f, 1f, 0.0f}, new float[]{0.4f, 0.4f, 0.6f, 0.0f}};
        float width;
        float height;
        char view = '0';
        double gizmosOffsetX = 12;
        double gizmosOffsetY = 11.5;
        double gizmosOffsetZ = 12;
        //uint selectedPolyhedron;

        List<Parallelepiped> paras;

        float aspect_ratio;
        Matrix4 perspective;
        #endregion

        public Window()
            : base(1366, 768, new GraphicsMode(32, 0, 0, 4), "Parallel Computed Collision Detection")
        {
            WindowState = WindowState.Fullscreen;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            GL.ClearColor(0.0f, 0.0f, 0.0f, 0.0f);

            GL.Material(TK.MaterialFace.Front, TK.MaterialParameter.Specular, mat_specular);
            GL.Material(TK.MaterialFace.Front, TK.MaterialParameter.Shininess, mat_shininess);
            GL.Light(TK.LightName.Light0, TK.LightParameter.Position, light_position);
            GL.Light(TK.LightName.Light0, TK.LightParameter.Ambient, light_ambient);
            GL.Light(TK.LightName.Light0, TK.LightParameter.Diffuse, mat_specular);

            GL.Enable(TK.EnableCap.CullFace);
            GL.Enable(TK.EnableCap.DepthTest);
            GL.ShadeModel(TK.ShadingModel.Smooth);
            GL.Enable(TK.EnableCap.ColorMaterial);
            //GL.Enable(TK.EnableCap.AlphaTest);
            GL.Enable(TK.EnableCap.Blend);
            GL.BlendFunc(TK.BlendingFactorSrc.SrcAlpha, TK.BlendingFactorDest.OneMinusSrcAlpha);
            //GL.BlendFunc(TK.BlendingFactorSrc.SrcAlpha, TK.BlendingFactorDest.OneMinusSrcAlpha);

            old_mouse = OpenTK.Input.Mouse.GetState();
            old_key = OpenTK.Input.Keyboard.GetState();

            paras = new List<Parallelepiped>();
            paras.Add(new Parallelepiped(new Vector3(5, 5, 5), 3, 3, 3, 90));
            paras.Add(new Parallelepiped(new Vector3(-5, -5, -5), 5, 5, 5, 90));
            paras.Add(new Parallelepiped(new Vector3(-3, 3, 0), 4, 4, 4, 90));
            paras.Add(new Parallelepiped(new Vector3(6, -4, 3), 2, 3, 4, 70));
            paras.Add(new Parallelepiped(new Vector3(-7, 7, 4), 1.5, 1.5, 1.5, 120));

            GL.Viewport(0, 0, Width, Height);
            aspect_ratio = Width / (float)Height;
            checkAspectRatio();
            width *= wp_scale_factor;
            height *= wp_scale_factor;
            fov = height * 0.75f;

            VSync = VSyncMode.On;
            eye = new Vector3(0, 0, height * 1.5f);
            target = new Vector3(0, 0, 0);
            up = new Vector3(0, 1, 0);          
        }

        protected override void OnUnload(EventArgs e)
        {
        }

        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(0, 0, Width, Height);

            aspect_ratio = Width / (float)Height;
            perspective = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver4, aspect_ratio, 1, 64);
            GL.MatrixMode(TK.MatrixMode.Projection);
            GL.LoadMatrix(ref perspective);
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
            foreach (Parallelepiped para in paras)
                para.calculateBoundingSphere();
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
            scale_factor += (mouse.WheelPrecise - old_mouse.WheelPrecise) * 0.5f;
            if(scale_factor > 50f)
                scale_factor = 50f;
            else if(scale_factor < 0.5f)
                scale_factor = 0.5f;
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
            }

            if (offsetX == -90f && offsetY == 0f)
            {
                xRot = false;
                view = 'x';
            }

            else if (offsetX == 0f && offsetY == 90f)
            {
                yRot = false;
                view = 'y';
            }

            else if (offsetX == 0f && offsetY == 0f)
            {
                zRot = false;
                view = 'z';
            }

            else
                view = '0';
            #endregion
            //old_key = OpenTK.Input.Keyboard.GetState();
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(TK.ClearBufferMask.DepthBufferBit | TK.ClearBufferMask.ColorBufferBit);

#if ORTHO
            modelView = Matrix4.CreateOrthographic(width, height, -height, height);
            GL.MatrixMode(TK.MatrixMode.Projection);
            GL.LoadMatrix(ref modelView);
            GL.Scale(scale_factor, scale_factor, scale_factor);
#else
            modelView = Matrix4.LookAt(eye, target, up);
            GL.MatrixMode(TK.MatrixMode.Modelview);
            GL.LoadMatrix(ref modelView);
#endif
            GL.Rotate(offsetX, 0.0f, 1.0f, 0.0f);
            GL.Rotate(offsetY, 1.0f, 0.0f, 0.0f);

            DrawGrid(5);

            #region Draw 3D Polyhedrons
            
            GL.Color3(0.0, 0.5, 1.0);

            GL.Enable(TK.EnableCap.Light0);
            GL.Enable(TK.EnableCap.Lighting);

            int i = 0;
            foreach (Parallelepiped para in paras)
            {
                colors[i][3] = 1f;
                GL.Color4(colors[i]);
                GL.PolygonMode(TK.MaterialFace.FrontAndBack, TK.PolygonMode.Fill);
                para.Draw();
                GL.Color3(0.2f, 0.5f, 1f);
                GL.PolygonMode(TK.MaterialFace.FrontAndBack, TK.PolygonMode.Line);
                para.bsphere.Draw();
                i++;
            }

            GL.Disable(TK.EnableCap.Lighting);
            GL.Disable(TK.EnableCap.Light0);
            #endregion

            //GL.BlendFunc(TK.BlendingFactorSrc.One, TK.BlendingFactorDest.OneMinusSrcAlpha);
            #region Draw Gizmos
            drawCollisionVectors();
            #endregion

            SwapBuffers();
        }

        void DrawGrid3x3()
        {
            GL.Begin(TK.PrimitiveType.LineLoop);
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

            GL.Begin(PrimitiveType.Lines);
            {
                GL.Color3(1.0, 1.0, 1.0);

                GL.Vertex3(-1.0, 3.0, 1.0);
                GL.Vertex3(-1.0, -3.0, 1.0);
            }
            GL.End();

            GL.Begin(PrimitiveType.Lines);
            {
                GL.Color3(1.0, 1.0, 1.0);

                GL.Vertex3(1.0, 3.0, 1.0);
                GL.Vertex3(1.0, -3.0, 1.0);
            }
            GL.End();

            GL.Begin(PrimitiveType.Lines);
            {
                GL.Color3(1.0, 1.0, 1.0);

                GL.Vertex3(-1.0, 3.0, -1.0);
                GL.Vertex3(-1.0, -3.0, -1.0);
            }
            GL.End();

            GL.Begin(PrimitiveType.Lines);
            {
                GL.Color3(1.0, 1.0, 1.0);

                GL.Vertex3(1.0, 3.0, -1.0);
                GL.Vertex3(1.0, -3.0, -1.0);
            }
            GL.End();
        }

        void DrawGrid(int x)
        {
            float edge = fov / x;
            float half_fov = fov * 0.5f;
            float offset;

            GL.Color4(1f, 0f, 0f, 1f);

            //grid y
            GL.Begin(PrimitiveType.LineLoop);
            {
                GL.Vertex3(-half_fov, -half_fov, half_fov);
                GL.Vertex3(half_fov, -half_fov, half_fov);
                GL.Vertex3(half_fov, -half_fov, -half_fov);
                GL.Vertex3(-half_fov, -half_fov, -half_fov);
            }
            GL.End();

            for (int i = 1; i < x; i++)
            {
                offset = edge * i;

                GL.Begin(PrimitiveType.Lines);
                {
                    GL.Vertex3(offset - half_fov, -half_fov, half_fov);
                    GL.Vertex3(offset - half_fov, -half_fov, -half_fov);

                    GL.Vertex3(-half_fov, -half_fov, offset - half_fov);
                    GL.Vertex3(half_fov, -half_fov, offset - half_fov);
                }
                GL.End();
            }

            GL.Color4(0f, 1f, 0f, 1f);

            //grid x
            GL.Begin(PrimitiveType.LineLoop);
            {
                GL.Vertex3(-half_fov, half_fov, half_fov);
                GL.Vertex3(-half_fov, half_fov, -half_fov);
                GL.Vertex3(-half_fov, -half_fov, -half_fov);
                GL.Vertex3(-half_fov, -half_fov, half_fov);
            }
            GL.End();

            for (int i = 1; i < x; i++)
            {
                offset = edge * i;

                GL.Begin(PrimitiveType.Lines);
                {
                    GL.Vertex3(-half_fov, -half_fov, offset - half_fov);
                    GL.Vertex3(-half_fov, half_fov, offset - half_fov);

                    GL.Vertex3(-half_fov, offset - half_fov, half_fov);
                    GL.Vertex3(-half_fov, offset - half_fov, -half_fov);
                }
                GL.End();
            }

            GL.Color4(0f, 0f, 1f, 1f);

            //grid z
            GL.Begin(PrimitiveType.LineLoop);
            {
                GL.Vertex3(-half_fov, -half_fov, -half_fov);
                GL.Vertex3(half_fov, -half_fov, -half_fov);
                GL.Vertex3(half_fov, half_fov, -half_fov);
                GL.Vertex3(-half_fov, half_fov, -half_fov);
            }
            GL.End();

            for (int i = 1; i < x; i++)
            {
                offset = edge * i;

                GL.Begin(PrimitiveType.Lines);
                {
                    GL.Vertex3(offset - half_fov, -half_fov, -half_fov);
                    GL.Vertex3(offset - half_fov, half_fov, -half_fov);

                    GL.Vertex3(-half_fov, offset - half_fov, -half_fov);
                    GL.Vertex3(half_fov, offset - half_fov, -half_fov);
                }
                GL.End();
            }

        }

        void checkAspectRatio()
        {
            if (aspect_ratio == 4 / 3f)
            {
                width = 4f;
                height = 3f;
                return;
            }
            if (aspect_ratio == 16 / 10f)
            {
                width = 16f;
                height = 10f;
                return;
            }
            else
            {
                width = 16f;
                height = 9f;
            }
        }

        void drawCollisionVectors()
        {
            if (view == '0')
                return;

            GL.LineWidth(5.0f);
            int i = 0;
            float j = 0;

            foreach (Parallelepiped para in paras)
            {
                colors[i][3] = 0.5f;
                GL.Color4(colors[i]);
                double paraOffset = Math.Abs(para.offsetX) + para.length * 0.5;

                if (view == 'z')
                {
                    GL.Begin(PrimitiveType.Lines);
                    {
                        /*GL.Vertex3(para.pos.X - paraOffset, - (gizmosOffsetY + j), para.pos.Z);
                        GL.Vertex3(para.pos.X + paraOffset, - (gizmosOffsetY + j), para.pos.Z);

                        GL.Vertex3(- (gizmosOffsetX + j), para.pos.Y + para.height * 0.5, para.pos.Z);
                        GL.Vertex3(- (gizmosOffsetX + j), para.pos.Y - para.height * 0.5, para.pos.Z);*/

                        GL.Vertex3(para.pos.X - para.bsphere.radius, -(gizmosOffsetY + j), para.pos.Z);
                        GL.Vertex3(para.pos.X + para.bsphere.radius, -(gizmosOffsetY + j), para.pos.Z);

                        GL.Vertex3(-(gizmosOffsetX + j), para.pos.Y + para.bsphere.radius, para.pos.Z);
                        GL.Vertex3(-(gizmosOffsetX + j), para.pos.Y - para.bsphere.radius, para.pos.Z);
                    }
                    GL.End();
                }

                else if (view == 'x')
                {
                    GL.Begin(PrimitiveType.Lines);
                    {
                        /*GL.Vertex3(para.pos.X, -(gizmosOffsetY + j), para.pos.Z - para.width * 0.5);
                        GL.Vertex3(para.pos.X, -(gizmosOffsetY + j), para.pos.Z + para.width * 0.5);

                        GL.Vertex3(para.pos.X, para.pos.Y + para.height * 0.5, gizmosOffsetZ + j);
                        GL.Vertex3(para.pos.X, para.pos.Y - para.height * 0.5, gizmosOffsetZ + j);*/

                        GL.Vertex3(para.pos.X, -(gizmosOffsetY + j), para.pos.Z - para.bsphere.radius);
                        GL.Vertex3(para.pos.X, -(gizmosOffsetY + j), para.pos.Z + para.bsphere.radius);

                        GL.Vertex3(para.pos.X, para.pos.Y + para.bsphere.radius, gizmosOffsetZ + j);
                        GL.Vertex3(para.pos.X, para.pos.Y - para.bsphere.radius, gizmosOffsetZ + j);
                    }
                    GL.End();
                }

                else
                {
                    GL.Begin(PrimitiveType.Lines);
                    {
                        /*GL.Vertex3(-(gizmosOffsetX + j), para.pos.Y, para.pos.Z + para.width *0.5);
                        GL.Vertex3(-(gizmosOffsetX + j), para.pos.Y, para.pos.Z - para.width * 0.5);

                        GL.Vertex3(para.pos.X - paraOffset, para.pos.Y, gizmosOffsetZ + j);
                        GL.Vertex3(para.pos.X + paraOffset, para.pos.Y, gizmosOffsetZ + j);*/

                        GL.Vertex3(-(gizmosOffsetX + j), para.pos.Y, para.pos.Z + para.bsphere.radius);
                        GL.Vertex3(-(gizmosOffsetX + j), para.pos.Y, para.pos.Z - para.bsphere.radius);

                        GL.Vertex3(para.pos.X - para.bsphere.radius, para.pos.Y, gizmosOffsetZ + j);
                        GL.Vertex3(para.pos.X + para.bsphere.radius, para.pos.Y, gizmosOffsetZ + j);
                    }
                    GL.End();
                }
                i++;
                j += 0.2f;
            }
            GL.LineWidth(1.0f);
        }
    }
}
