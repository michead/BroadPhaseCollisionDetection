using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;

#region Assembly Collision
using GL = OpenTK.Graphics.OpenGL.GL;
using PolygonMode = OpenTK.Graphics.OpenGL.PolygonMode;
using MaterialFace = OpenTK.Graphics.OpenGL.MaterialFace;
#endregion

namespace ParallelComputedCollisionDetection
{
    public class Sphere
    {
        [Flags]
        public enum HomeCellType
        {
            One = 1,
            Two = 2,
            Three = 3,
            Four = 4,
            Five = 5,
            Six = 6,
            Seven = 7,
            Eight = 8
        }

        public IntPtr quad;
        public Vector3 pos;
        public double radius; 
        public int slices;
        public int stacks;
        public float left;
        public float right;
        public float top;
        public float bottom;
        public float front;
        public float back;
        HomeCellType homeCellType;
        uint cellTypesIntersected;

        public Sphere(Vector3 pos, double radius, int slices, int stacks)
        {
            this.pos = pos;
            this.radius = radius;
            this.slices = slices;
            this.stacks = stacks;
            quad = Glu.NewQuadric();
            this.left = pos.X - (float)radius;
            this.right = pos.X + (float)radius;
            this.top = pos.Y + (float)radius;
            this.bottom = pos.Y - (float)radius;
            this.front = pos.Z + (float)radius;
            this.back = pos.Z - (float)radius;
        }

        public void Draw()
        {
            GL.Translate(pos);
            Glu.Sphere(quad, radius, slices, stacks);
            GL.Translate(-pos);
        }
    }
}
