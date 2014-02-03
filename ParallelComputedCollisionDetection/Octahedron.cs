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
    class Octahedron : Body
    {
        double[][] vertices = new double[6][];
        int sphere_precision = 25;
        double Pi = Math.PI;
        double radius;
        double phiaa;
        double phia;
        double the90;
        double the;
        Sphere bsphere;

        public Octahedron(Vector3 pos, double radius)
        {
            phiaa = 0.0;
            radius = 1.0;
            phia = Pi * phiaa / 180.0;
            the90 = Pi * 90.0 / 180;

            vertices[0] = new double[3];
            vertices[0][0] = 0.0;
            vertices[0][1] = 0.0;
            vertices[0][2] = radius;

            vertices[5] = new double[3];
            vertices[5][0] = 0.0;
            vertices[5][1] = 0.0;
            vertices[5][2] = -radius;

            the = 0.0;
            for (int i = 1; i < 5; i++)
            {
                vertices[i] = new double[3];
                vertices[i][0] = radius * Math.Cos(the) * Math.Cos(phia);
                vertices[i][1] = radius * Math.Sin(the) * Math.Cos(phia);
                vertices[i][2] = radius * Math.Sin(phia);
                the = the + the90;
            }
        }

        public void Draw()
        {
            GL.Begin(PrimitiveType.Polygon);
            {
                GL.Vertex3(vertices[0]);
                GL.Vertex3(vertices[1]);
                GL.Vertex3(vertices[2]);
            }
            GL.End();

            GL.Begin(PrimitiveType.Polygon);
            {
                GL.Vertex3(vertices[0]);
                GL.Vertex3(vertices[2]);
                GL.Vertex3(vertices[3]);
            }
            GL.End();

            GL.Begin(PrimitiveType.Polygon);
            {
                GL.Vertex3(vertices[0]);
                GL.Vertex3(vertices[3]);
                GL.Vertex3(vertices[4]);
            }
            GL.End();

            GL.Begin(PrimitiveType.Polygon);
            {
                GL.Vertex3(vertices[0]);
                GL.Vertex3(vertices[4]);
                GL.Vertex3(vertices[1]);
            }
            GL.End();

            GL.Begin(PrimitiveType.Polygon);
            {
                GL.Vertex3(vertices[5]);
                GL.Vertex3(vertices[1]);
                GL.Vertex3(vertices[2]);
            }
            GL.End();

            GL.Begin(PrimitiveType.Polygon);
            {
                GL.Vertex3(vertices[5]);
                GL.Vertex3(vertices[2]);
                GL.Vertex3(vertices[3]);
            }
            GL.End();

            GL.Begin(PrimitiveType.Polygon);
            {
                GL.Vertex3(vertices[5]);
                GL.Vertex3(vertices[3]);
                GL.Vertex3(vertices[4]);
            }
            GL.End();

            GL.Begin(PrimitiveType.Polygon);
            {
                GL.Vertex3(vertices[5]);
                GL.Vertex3(vertices[4]);
                GL.Vertex3(vertices[1]);
            }
            GL.End();
        }

        public double getRadius()
        {
            return radius;
        }
        public Vector3 getPos() { return Vector3.Zero; }

        public void setPos(Vector3 pos) { }

        public void calculateBoundingSphere()
        {
            bsphere = new Sphere(Vector3.Zero, radius, sphere_precision, sphere_precision, 0);
        }

        public Sphere getBSphere()
        {
            return bsphere;
        }

        public void updateBoundingSphere() { }
    }
}
