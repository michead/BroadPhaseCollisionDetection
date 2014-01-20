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
    class Dodecahedron : Body
    {
        double[][] vertices = new double[20][];
        double Pi = MathHelper.Pi;
        double phiaa = 52.62263590; 
        double phibb = 10.81231754;
        double radius;
        double phia;
        double phib;
        double phic;
        double phid;
        double the72;
        double theb;
        double the;
        Sphere bsphere;
        int sphere_precision = 25;
        double edge;

        public Dodecahedron(Vector3 pos, double radius){
            this.radius = radius;
            phia = Pi * phiaa / 180.0;
            phib = Pi*phibb/180.0;
            phic = Pi*(-phibb)/180.0;
            phid = Pi*(-phiaa)/180.0;
            the72 = Pi*72.0/180;
            theb = the72/2.0;
            the = 0.0;
            edge = 0.713644 * radius;

            for (int i = 0; i < 20; i++)
                vertices[i] = new double[3];

            for (int i = 0; i < 5; i++)
            {
                vertices[i][0] = radius * Math.Cos(the) * Math.Cos(phia);
                vertices[i][1] = radius * Math.Sin(the) * Math.Cos(phia);
                vertices[i][2] = radius * Math.Sin(phia);
                the = the + the72;
            }
            the = 0.0;
            for (int i = 5; i < 10; i++)
            {
                vertices[i][0] = radius * Math.Cos(the) * Math.Cos(phib);
                vertices[i][1] = radius * Math.Sin(the) * Math.Cos(phib);
                vertices[i][2] = radius * Math.Sin(phib);
                the = the + the72;
            }
            the = theb;
            for (int i = 10; i < 15; i++)
            {
                vertices[i][0] = radius * Math.Cos(the) * Math.Cos(phic);
                vertices[i][1] = radius * Math.Sin(the) * Math.Cos(phic);
                vertices[i][2] = radius * Math.Sin(phic);
                the = the + the72;
            }
            the = theb;
            for (int i = 15; i < 20; i++)
            {
                vertices[i][0] = radius * Math.Cos(the) * Math.Cos(phid);
                vertices[i][1] = radius * Math.Sin(the) * Math.Cos(phid);
                vertices[i][2] = radius * Math.Sin(phid);
                the = the + the72;
            }
        }

        public void Draw()
        {
            GL.PolygonMode(TK.MaterialFace.FrontAndBack, TK.PolygonMode.Fill);
            GL.Begin(PrimitiveType.Polygon);
            {
                GL.Vertex3(vertices[0]);
                GL.Vertex3(vertices[1]);
                GL.Vertex3(vertices[2]);
                GL.Vertex3(vertices[3]);
                GL.Vertex3(vertices[4]);
            }
            GL.End();

            GL.Begin(PrimitiveType.Polygon);
            {
                GL.Vertex3(vertices[0]);
                GL.Vertex3(vertices[1]);
                GL.Vertex3(vertices[6]);
                GL.Vertex3(vertices[10]);
                GL.Vertex3(vertices[5]);
            }
            GL.End();

            GL.Begin(PrimitiveType.Polygon);
            {
                GL.Vertex3(vertices[1]);
                GL.Vertex3(vertices[2]);
                GL.Vertex3(vertices[7]);
                GL.Vertex3(vertices[11]);
                GL.Vertex3(vertices[6]);
            }
            GL.End();

            GL.Begin(PrimitiveType.Polygon);
            {
                GL.Vertex3(vertices[2]);
                GL.Vertex3(vertices[3]);
                GL.Vertex3(vertices[8]);
                GL.Vertex3(vertices[12]);
                GL.Vertex3(vertices[7]);
            }
            GL.End();

            GL.Begin(PrimitiveType.Polygon);
            {
                GL.Vertex3(vertices[3]);
                GL.Vertex3(vertices[4]);
                GL.Vertex3(vertices[9]);
                GL.Vertex3(vertices[13]);
                GL.Vertex3(vertices[8]);
            }
            GL.End();

            GL.Begin(PrimitiveType.Polygon);
            {
                GL.Vertex3(vertices[4]);
                GL.Vertex3(vertices[0]);
                GL.Vertex3(vertices[5]);
                GL.Vertex3(vertices[14]);
                GL.Vertex3(vertices[9]);
            }
            GL.End();

            GL.Begin(PrimitiveType.Polygon);
            {
                GL.Vertex3(vertices[15]);
                GL.Vertex3(vertices[16]);
                GL.Vertex3(vertices[11]);
                GL.Vertex3(vertices[6]);
                GL.Vertex3(vertices[10]);
            }
            GL.End();

            GL.Begin(PrimitiveType.Polygon);
            {
                GL.Vertex3(vertices[16]);
                GL.Vertex3(vertices[17]);
                GL.Vertex3(vertices[12]);
                GL.Vertex3(vertices[7]);
                GL.Vertex3(vertices[11]);
            }
            GL.End();

            GL.Begin(PrimitiveType.Polygon);
            {
                GL.Vertex3(vertices[17]);
                GL.Vertex3(vertices[18]);
                GL.Vertex3(vertices[13]);
                GL.Vertex3(vertices[8]);
                GL.Vertex3(vertices[12]);
            }
            GL.End();

            GL.Begin(PrimitiveType.Polygon);
            {
                GL.Vertex3(vertices[18]);
                GL.Vertex3(vertices[19]);
                GL.Vertex3(vertices[14]);
                GL.Vertex3(vertices[9]);
                GL.Vertex3(vertices[13]);
            }
            GL.End();

            GL.Begin(PrimitiveType.Polygon);
            {
                GL.Vertex3(vertices[19]);
                GL.Vertex3(vertices[15]);
                GL.Vertex3(vertices[10]);
                GL.Vertex3(vertices[5]);
                GL.Vertex3(vertices[14]);
            }
            GL.End();

            GL.Begin(PrimitiveType.Polygon);
            {
                GL.Vertex3(vertices[15]);
                GL.Vertex3(vertices[16]);
                GL.Vertex3(vertices[17]);
                GL.Vertex3(vertices[18]);
                GL.Vertex3(vertices[19]);
            }
            GL.End();
        }

        public double getRadius()
        {
            return radius;
        }
        public Vector3 getPos()
        {
            return Vector3.Zero;
        }

        public void setPos(Vector3 pos) { }

        public void calculateBoundingSphere()
        {
            bsphere = new Sphere(Vector3.Zero, radius, sphere_precision, sphere_precision, 0);
        }

        public Sphere getBSphere()
        {
            return bsphere;
        }
    }
}
