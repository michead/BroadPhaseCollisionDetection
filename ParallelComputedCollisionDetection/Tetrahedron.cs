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
    class Tetrahedron : Body
    {
        double[][] vertices = new double [4][];
        double Pi = Math.PI;
        double phiaa = -19.471220333;
        double radius;
        double phia;
        double the120;
        double the;
        Vector3 pos;
        Sphere bsphere;
        double edge;
        int sphere_precision = 25;

        public Tetrahedron(Vector3 pos, double radius)
        {
            this.radius = radius;
            phia = Pi * phiaa / 180.0;
            the120 = Pi * 120.0 / 180.0;
            vertices[0] = new double[3];
            vertices[0][0] = 0.0;
            vertices[0][1] = 0.0;
            vertices[0][2] = radius;
            the = 0.0;
            edge = 1.6329932 * radius;
            pos = new Vector3((float)vertices[0][0], (float)vertices[0][1], (float)(vertices[0][2] - ((Math.Sqrt(6) * edge) / 4)));
            setPos(pos);
        }

        public void Draw()
        {
            GL.Normal3(0f, 1f, 0f);
            GL.Begin(PrimitiveType.Polygon);
            {
                GL.Vertex3(vertices[0]);
                GL.Vertex3(vertices[1]);
                GL.Vertex3(vertices[2]);
            }
            GL.End();

            GL.Normal3(-1f, 0f, 0f);
            GL.Begin(PrimitiveType.Polygon);
            {
                GL.Vertex3(vertices[0]);
                GL.Vertex3(vertices[2]);
                GL.Vertex3(vertices[3]);
            }
            GL.End();

            GL.Normal3(0.5f, -0.5f, 0.5f);
            GL.Begin(PrimitiveType.Polygon);
            {
                GL.Vertex3(vertices[0]);
                GL.Vertex3(vertices[3]);
                GL.Vertex3(vertices[1]);
            }
            GL.End();

            GL.Normal3(0f, 0f, -1f);
            GL.Begin(PrimitiveType.Polygon);
            {
                GL.Vertex3(vertices[3]);
                GL.Vertex3(vertices[2]);
                GL.Vertex3(vertices[1]);
            }
            GL.End();
        }

        public double getRadius()
        {
            return radius;
        }
        public Vector3 getPos()
        {
            return pos;
        }

        public void setPos(Vector3 pos) { 
            for (int i = 1; i < 4; i++)
            {
                vertices[i] = new double[3];
                vertices[i][0] = radius * Math.Cos(the) * Math.Cos(phia);
                vertices[i][1] = radius * Math.Sin(the) * Math.Cos(phia);
                vertices[i][2] = radius * Math.Sin(phia);
                the = the + the120;
            }

            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 3; j++)
                    vertices[i][j] += (pos[j] - this.pos[j]);
        }

        public void calculateBoundingSphere()
        {
            bsphere = new Sphere(pos, radius, sphere_precision, sphere_precision);
        }

        public Sphere getBSphere()
        {
            return bsphere;
        }
    }
}
