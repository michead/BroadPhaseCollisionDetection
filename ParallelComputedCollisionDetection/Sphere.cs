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
        /*[Flags]
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
        }*/

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
        //public HomeCellType homeCellType;
        public uint hCell;
        public uint cellTypesIntersected;
        public uint bodyIndex;
        public uint[] cellArray = new uint[8];

        public const uint XSHIFT = 0;
        public const uint YSHIFT = 3;
        public const uint ZSHIFT = 6;
        public const uint intersectCType0 = 1 << 1;
        public const uint intersectCType1 = 2 << 1;
        public const uint intersectCType2 = 4 << 1;
        public const uint intersectCType3 = 8 << 1;
        public const uint intersectCType4 = 16 << 1;
        public const uint intersectCType5 = 32 << 1;
        public const uint intersectCType6 = 64 << 1;
        public const uint intersectCType7 = 128 << 1;

        public Sphere(Vector3 pos, double radius, int slices, int stacks, uint bodyIndex)
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
            this.bodyIndex = bodyIndex;

            for (int i = 0; i < 8; i++)
                cellArray[i] |= bodyIndex << 5;
        }

        public void Draw()
        {
            GL.Translate(pos);
            Glu.Sphere(quad, radius, slices, stacks);
            GL.Translate(-pos);
        }

        /*public void checkHomeCellType(){
            double half_fov = Window.fov * 0.5;

            if ((int)(-(pos.Z - half_fov) / Window.grid_edge) % 2 == 0)
            {
                if ((int)(-(pos.X - half_fov) / Window.grid_edge) % 2 == 0)
                {
                    if ((int)(-(pos.Y - half_fov) / Window.grid_edge) % 2 == 0)
                    {
                        homeCellType = HomeCellType.Two;
                    }
                    else
                        homeCellType = HomeCellType.Four;
                }
                else
                {
                    if ((int)(-(pos.Y - half_fov) / Window.grid_edge) % 2 == 0)
                    {
                        homeCellType = HomeCellType.One;
                    }
                    else
                        homeCellType = HomeCellType.Three;
                }
            }
            else
            {
                if ((int)(-(pos.X - half_fov) / Window.grid_edge) % 2 == 0)
                {
                    if ((int)(-(pos.Y - half_fov) / Window.grid_edge) % 2 == 0)
                    {
                        homeCellType = HomeCellType.Six;
                    }
                    else
                        homeCellType = HomeCellType.Eight;
                }
                else
                {
                    if ((int)(-(pos.Y - half_fov) / Window.grid_edge) % 2 == 0)
                    {
                        homeCellType = HomeCellType.Five;
                    }
                    else
                        homeCellType = HomeCellType.Seven;
                }
            }
        }*/

        public void checkHomeCellType()
        {
            double half_fov = Window.fov * 0.5;
            cellArray[0] |= 1;

            if ((int)(-(pos.Z - half_fov) / Window.grid_edge) % 2 == 0)
            {
                if ((int)(-(pos.X - half_fov) / Window.grid_edge) % 2 == 0)
                {
                    if ((int)(-(pos.Y - half_fov) / Window.grid_edge) % 2 == 0)
                    {
                        hCell = 0;
                        cellArray[0] |= intersectCType0;
                    }
                    else
                    {
                        hCell = 2;
                        cellArray[0] |= intersectCType2;
                    }
                }
                else
                {
                    if ((int)(-(pos.Y - half_fov) / Window.grid_edge) % 2 == 0)
                    {
                        hCell = 1;
                        cellArray[0] |= intersectCType1;
                    }
                    else
                    {
                        hCell = 3;
                        cellArray[0] |= intersectCType3;
                    }
                }
            }
            else
            {
                if ((int)(-(pos.X - half_fov) / Window.grid_edge) % 2 == 0)
                {
                    if ((int)(-(pos.Y - half_fov) / Window.grid_edge) % 2 == 0)
                    {
                        hCell = 4;
                        cellArray[0] |= intersectCType4;
                    }
                    else
                    {
                        hCell = 6;
                        cellArray[0] |= intersectCType6;
                    }
                }
                else
                {
                    if ((int)(-(pos.Y - half_fov) / Window.grid_edge) % 2 == 0)
                    {
                        hCell = 5;
                        cellArray[0] |= intersectCType5;
                    }
                    else
                    {
                        hCell = 7;
                        cellArray[0] |= intersectCType7;
                    }
                }
            }
        }

        /*public void hashHCell()
        {
            hCell = ((uint)(pos.X / Window.grid_edge) << XSHIFT) |
                    ((uint)(pos.Y / Window.grid_edge) << YSHIFT) |
                    ((uint)(pos.Z / Window.grid_edge) << ZSHIFT);
        }*/
    }
}
