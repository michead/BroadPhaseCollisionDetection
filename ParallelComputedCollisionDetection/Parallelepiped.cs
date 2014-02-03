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
    class Parallelepiped : Body
    {
        Vector3 pos;
        public double length; 
        public double height; 
        public double width;
        public float angle;
        public float angle_;
        Sphere bsphere;
        int sphere_precision = 30;
        public double offsetX;
        double radius;
        public int index;

        public Parallelepiped(Vector3 pos, double edge, int index) {
            this.angle = 0f;
            this.angle_ = MathHelper.PiOver2;
            this.length = edge;
            this.height = edge;
            this.width = edge;
            this.pos = pos;
            this.index = index;
            this.offsetX = 0;
            this.radius =
                Math.Sqrt(Math.Pow(length * 0.5, 2) + Math.Pow(height * 0.5, 2) + Math.Pow(width * 0.5, 2));
            if (index != -1)
            {
                calculateBoundingSphere();
                bsphere.checkForCellIntersection();
            }
        }

        public Parallelepiped(Vector3 pos, double length, double height, double width, float angle, int index)
        {
            this.pos = pos;
            this.length = length;
            this.height = height;
            this.width = width;
            this.angle = angle;
            this.angle_ = MathHelper.DegreesToRadians(90f - angle);
            this.index = index;
            this.offsetX = (this.height / (Math.Sin(this.angle_) * 2))
                    * Math.Cos(this.angle_);
            this.radius = 
                Math.Sqrt(Math.Pow(Math.Abs(offsetX) * 0.5 + length * 0.5, 2) + Math.Pow(height * 0.5, 2) + Math.Pow(width * 0.5, 2));
            if (index != -1)
            {
                calculateBoundingSphere();
                bsphere.checkForCellIntersection();
            }
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
                GL.Normal3(Math.Sin(angle_), -Math.Cos(angle_), 0);

                GL.Vertex3(length * 0.5, -height * 0.5, -width * 0.5);
                GL.Vertex3(length * 0.5 + offsetX, height * 0.5, -width * 0.5);
                GL.Vertex3(length * 0.5 + offsetX, height * 0.5, width * 0.5);
                GL.Vertex3(length * 0.5, -height * 0.5, width * 0.5);

                // left face
                GL.Normal3(-Math.Sin(angle_), Math.Cos(angle_), 0);

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
            bsphere = new Sphere(new Vector3(pos.X + (float)offsetX * 0.5f, pos.Y, pos.Z), radius, sphere_precision, sphere_precision, index);
        }

        public void updateBoundingSphere()
        {
            bsphere.pos = new Vector3(pos.X + (float)offsetX * 0.5f, pos.Y, pos.Z);
            bsphere.radius = radius;
            bsphere.slices = sphere_precision;
            bsphere.stacks = sphere_precision;
            bsphere.bodyIndex = index;
        }

        public double getRadius()
        {
            return radius;
        }

        public Vector3 getPos()
        {
            return pos;
        }

        public Sphere getBSphere()
        {
            return bsphere;
        }

        public void setPos(Vector3 pos)
        {
            this.pos = pos;
            bsphere.checkHomeCellType();
            bsphere.checkForCellIntersection();
            /*string binValue = Convert.ToString(bsphere.cellArray[0], 2);
            char[] bits = binValue.PadLeft(16, '0').ToCharArray();
            binValue = "";
            for (int i = 0; i < bits.Count(); i++)
                binValue += bits[i] + " ";
            binValue += "\n";
            Console.Write(binValue + "\n");*/
        }
    }
}
