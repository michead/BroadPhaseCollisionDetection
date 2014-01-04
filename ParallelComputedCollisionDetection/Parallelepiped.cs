using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

#region Assembly Collision
using TK = OpenTK.Graphics.OpenGL;
using GL = OpenTK.Graphics.OpenGL.GL;
using PolygonMode = OpenTK.Graphics.OpenGL.PolygonMode;
using MaterialFace = OpenTK.Graphics.OpenGL.MaterialFace;
#endregion

namespace ParallelComputedCollisionDetection
{
    class Parallelepiped
    {
        public Vector3 pos;
        public double length; 
        public double height; 
        public double width;
        public float angle;
        float angle_;
        public Sphere bsphere;
        int sphere_precision = 30;
        double offsetX;

        public Parallelepiped(Vector3 pos, double length, double height, double width, float angle)
        {
            this.pos = pos;
            this.length = length;
            this.height = height;
            this.width = width;
            this.angle = angle;
            this.angle_ = (float)Math.PI * 0.5f - angle;
            this.offsetX = height / Math.Tan(MathHelper.DegreesToRadians(angle));
            calculateBoundingSphere();
        }

        public void Draw()
        {
            GL.Translate(pos);

            GL.Begin(PrimitiveType.Quads);
            {
                // front face
                GL.Normal3(0, 0, 1.0f);

                GL.Vertex3(length * 0.5, -height * 0.5, width * 0.5);
                GL.Vertex3(length * 0.5 + offsetX, height * 0.5, width * 0.5);
                GL.Vertex3(-length * 0.5 + offsetX, height * 0.5, width * 0.5);
                GL.Vertex3(-length * 0.5, -height * 0.5, width * 0.5);

                // back face
                GL.Normal3(0, 0, -1.0f);

                GL.Vertex3(-length * 0.5, -height * 0.5, -width * 0.5);
                GL.Vertex3(-length * 0.5 + offsetX, height * 0.5, -width * 0.5);
                GL.Vertex3(length * 0.5 + offsetX, height * 0.5, -width * 0.5);
                GL.Vertex3(length * 0.5, -height * 0.5, -width * 0.5);

                // top face
                GL.Normal3(0f, 1.0f, 0);

                GL.Vertex3(length * 0.5 + offsetX, height * 0.5, width * 0.5);
                GL.Vertex3(length * 0.5 + offsetX, height * 0.5, -width * 0.5);
                GL.Vertex3(-length * 0.5 + offsetX, height * 0.5, -width * 0.5);
                GL.Vertex3(-length * 0.5 + offsetX, height * 0.5, width * 0.5);

                // bottom face
                GL.Normal3(0, -1.0f, 0);

                GL.Vertex3(-length * 0.5, -height * 0.5, width * 0.5);
                GL.Vertex3(-length * 0.5, -height * 0.5, -width * 0.5);
                GL.Vertex3(length * 0.5, -height * 0.5, -width * 0.5);
                GL.Vertex3(length * 0.5, -height * 0.5, width * 0.5);

                // right face
                GL.Normal3(Math.Cos(angle_), -Math.Sin(angle_), 0);

                GL.Vertex3(length * 0.5, -height * 0.5, -width * 0.5);
                GL.Vertex3(length * 0.5 + offsetX, height * 0.5, -width * 0.5);
                GL.Vertex3(length * 0.5 + offsetX, height * 0.5, width * 0.5);
                GL.Vertex3(length * 0.5, -height * 0.5, width * 0.5);

                // left face
                GL.Normal3(-Math.Cos(angle_), Math.Sin(angle_), 0);

                GL.Vertex3(-length * 0.5, -height * 0.5, width * 0.5);
                GL.Vertex3(-length * 0.5 + offsetX, height * 0.5, width * 0.5);
                GL.Vertex3(-length * 0.5 + offsetX, height * 0.5, -width * 0.5);
                GL.Vertex3(-length * 0.5, -height * 0.5, -width * 0.5);
            }
            GL.End();

            GL.Translate(-pos);
        }

        public void calculateBoundingSphere()
        {
            double offs = offsetX;
            
            if(angle >= 90)
                offs = height / Math.Tan(MathHelper.DegreesToRadians(90 - (angle - 90)));

            double maxFromOrigin = Math.Sqrt(Math.Pow(offs * 0.5 + length * 0.5, 2) + Math.Pow(height * 0.5, 2) + Math.Pow(width * 0.5, 2));
            bsphere = new Sphere(new Vector3(pos.X + (float)offsetX * 0.5f, pos.Y, pos.Z), maxFromOrigin, sphere_precision, sphere_precision);
        }
    }
}
