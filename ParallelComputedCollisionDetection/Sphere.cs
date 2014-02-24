using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;

#region Assembly Collisions
using GL = OpenTK.Graphics.OpenGL.GL;
using PolygonMode = OpenTK.Graphics.OpenGL.PolygonMode;
using MaterialFace = OpenTK.Graphics.OpenGL.MaterialFace;
#endregion

namespace ParallelComputedCollisionDetection
{
    public class Sphere
    {
        public IntPtr quad;
        public Vector3 pos;
        public Vector3 cellPos;
        public float radius; 
        public int slices;
        public int stacks;
        public uint ctrl_bits;
        public int bodyIndex;
        public uint[] cellArray;
        public List<Body> cells;

        public const int XSHIFT = 0;
        public const int YSHIFT = 4;
        public const int ZSHIFT = 8;
        public const uint ICType1 = 1;
        public const uint ICType2 = 2;
        public const uint ICType3 = 4;
        public const uint ICType4 = 8;
        public const uint ICType5 = 16;
        public const uint ICType6 = 32;
        public const uint ICType7 = 64;
        public const uint ICType8 = 128;

        public Sphere(Vector3 pos, float radius, int slices, int stacks, int bodyIndex)
        {
            this.pos = pos;
            this.radius = radius;
            this.slices = slices;
            this.stacks = stacks;
            quad = Glu.NewQuadric();
            this.bodyIndex = bodyIndex;
            this.cellPos = Vector3.Zero;
            cells = new List<Body>(8);
            cellArray = new uint[8];

            /*for (int i = 0; i < 8; i++)
                cellArray[i] |= (uint)bodyIndex << 5;*/

            foreach (Body cell in cells)
            {
                checkCellType(cell.getPos().X, cell.getPos().Y, cell.getPos().Z, false);
            }
        }

        public void Draw()
        {
            GL.Translate(pos);
            Glu.Sphere(quad, radius, slices, stacks);
            GL.Translate(-pos);
        }

        public void checkForCellIntersection()
        {
            cells.Clear();
            ctrl_bits = 0;

            float grid_edge = (float)Program.window.grid_edge;
            if(pos.X>=0)
                cellPos.X = ((int)((this.pos.X + grid_edge * 0.5) / grid_edge)) * grid_edge;
            else
                cellPos.X = ((int)((this.pos.X - grid_edge * 0.5) / grid_edge)) * grid_edge;
            if (pos.Y >= 0)
                cellPos.Y = ((int)((this.pos.Y + grid_edge * 0.5) / grid_edge)) * grid_edge;
            else
                cellPos.Y = ((int)((this.pos.Y - grid_edge * 0.5) / grid_edge)) * grid_edge;
            if (pos.Z >= 0)
                cellPos.Z = ((int)((this.pos.Z + grid_edge * 0.5) / grid_edge)) * grid_edge;
            else
                cellPos.Z = ((int)((this.pos.Z - grid_edge * 0.5) / grid_edge)) * grid_edge;

            //hCell
            cells.Add(new Parallelepiped(cellPos, grid_edge, -1));
            checkCellType(cellPos.X, cellPos.Y, cellPos.Z, true);
            //hashCell(cellPos, 0);
            
            #region Check For Collisions
            //right
            if (checkForSphereBoxIntersection(
                                new Vector3(cellPos.X + grid_edge * 0.5f, cellPos.Y - grid_edge * 0.5f, cellPos.Z - grid_edge * 0.5f),
                                new Vector3(cellPos.X + grid_edge * 1.5f, cellPos.Y + grid_edge * 0.5f, cellPos.Z + grid_edge * 0.5f),
                                radius))
            {
                cells.Add(new Parallelepiped(Vector3.Add(cellPos, new Vector3(grid_edge, 0f, 0f)), grid_edge, -1));
            }

            //left
            if (checkForSphereBoxIntersection(
                                new Vector3(cellPos.X - grid_edge * 1.5f, cellPos.Y - grid_edge * 0.5f, cellPos.Z - grid_edge * 0.5f),
                                new Vector3(cellPos.X - grid_edge * 0.5f, cellPos.Y + grid_edge * 0.5f, cellPos.Z + grid_edge * 0.5f),
                                radius))
            {
                cells.Add(new Parallelepiped(Vector3.Add(cellPos, new Vector3(-grid_edge, 0f, 0f)), grid_edge, -1));
            }

            //top
            if (checkForSphereBoxIntersection(
                                new Vector3(cellPos.X - grid_edge * 0.5f, cellPos.Y + grid_edge * 0.5f, cellPos.Z - grid_edge * 0.5f),
                                new Vector3(cellPos.X + grid_edge * 0.5f, cellPos.Y + grid_edge * 1.5f, cellPos.Z + grid_edge * 0.5f),
                                radius))
            {
                cells.Add(new Parallelepiped(Vector3.Add(cellPos, new Vector3(0f, grid_edge, 0f)), grid_edge, -1));
            }

            //bottom
            if (checkForSphereBoxIntersection(
                                new Vector3(cellPos.X - grid_edge * 0.5f, cellPos.Y - grid_edge * 1.5f, cellPos.Z - grid_edge * 0.5f),
                                new Vector3(cellPos.X + grid_edge * 0.5f, cellPos.Y - grid_edge * 0.5f, cellPos.Z + grid_edge * 0.5f),
                                radius))
            {
                cells.Add(new Parallelepiped(Vector3.Add(cellPos, new Vector3(0f, -grid_edge, 0f)), grid_edge, -1));
            }

            //near
            if (checkForSphereBoxIntersection(
                                new Vector3(cellPos.X - grid_edge * 0.5f, cellPos.Y - grid_edge * 0.5f, cellPos.Z + grid_edge * 0.5f),
                                new Vector3(cellPos.X + grid_edge * 0.5f, cellPos.Y + grid_edge * 0.5f, cellPos.Z + grid_edge * 1.5f),
                                radius))
            {
                cells.Add(new Parallelepiped(Vector3.Add(cellPos, new Vector3(0f, 0f, grid_edge)), grid_edge, -1));
            }

            //far
            if (checkForSphereBoxIntersection(
                                new Vector3(cellPos.X - grid_edge * 0.5f, cellPos.Y - grid_edge * 0.5f, cellPos.Z - grid_edge * 1.5f),
                                new Vector3(cellPos.X + grid_edge * 0.5f, cellPos.Y + grid_edge * 0.5f, cellPos.Z - grid_edge * 0.5f),
                                radius))
            {
                cells.Add(new Parallelepiped(Vector3.Add(cellPos, new Vector3(0f, 0f, -grid_edge)), -grid_edge, -1));
            }

            //bottom_left
            if (checkForSphereBoxIntersection(
                                new Vector3(cellPos.X - grid_edge * 1.5f, cellPos.Y - grid_edge * 1.5f, cellPos.Z - grid_edge * 0.5f),
                                new Vector3(cellPos.X - grid_edge * 0.5f, cellPos.Y - grid_edge * 0.5f, cellPos.Z + grid_edge * 0.5f),
                                radius))
            {
                cells.Add(new Parallelepiped(Vector3.Add(cellPos, new Vector3(-grid_edge, -grid_edge, 0f)), grid_edge, -1));
            }
            //bottom_left_near
            if (checkForSphereBoxIntersection(
                                new Vector3(cellPos.X - grid_edge * 1.5f, cellPos.Y - grid_edge * 1.5f, cellPos.Z + grid_edge * 0.5f),
                                new Vector3(cellPos.X - grid_edge * 0.5f, cellPos.Y - grid_edge * 0.5f, cellPos.Z + grid_edge * 1.5f),
                                radius))
            {
                cells.Add(new Parallelepiped(Vector3.Add(cellPos, new Vector3(-grid_edge, -grid_edge, grid_edge)), grid_edge, -1));
            }

            //bottom_left_far
            if (checkForSphereBoxIntersection(
                                new Vector3(cellPos.X - grid_edge * 1.5f, cellPos.Y - grid_edge * 1.5f, cellPos.Z - grid_edge * 1.5f),
                                new Vector3(cellPos.X - grid_edge * 0.5f, cellPos.Y - grid_edge * 0.5f, cellPos.Z - grid_edge * 0.5f),
                                radius))
            {
                cells.Add(new Parallelepiped(Vector3.Add(cellPos, new Vector3(-grid_edge, -grid_edge, -grid_edge)), grid_edge, -1));
            }

            //bottom_right
            if (checkForSphereBoxIntersection(
                                new Vector3(cellPos.X + grid_edge * 0.5f, cellPos.Y - grid_edge * 1.5f, cellPos.Z - grid_edge * 0.5f),
                                new Vector3(cellPos.X + grid_edge * 1.5f, cellPos.Y - grid_edge * 0.5f, cellPos.Z + grid_edge * 0.5f),
                                radius))
            {
                cells.Add(new Parallelepiped(Vector3.Add(cellPos, new Vector3(grid_edge, -grid_edge, 0f)), grid_edge, -1));
            }

            //bottom_right_near
            if (checkForSphereBoxIntersection(
                                new Vector3(cellPos.X + grid_edge * 0.5f, cellPos.Y - grid_edge * 1.5f, cellPos.Z + grid_edge * 0.5f),
                                new Vector3(cellPos.X + grid_edge * 1.5f, cellPos.Y - grid_edge * 0.5f, cellPos.Z + grid_edge * 1.5f),
                                radius))
            {
                cells.Add(new Parallelepiped(Vector3.Add(cellPos, new Vector3(grid_edge, -grid_edge, grid_edge)), grid_edge, -1));
            }

            //bottom_right_far
            if (checkForSphereBoxIntersection(
                                new Vector3(cellPos.X + grid_edge * 0.5f, cellPos.Y - grid_edge * 1.5f, cellPos.Z - grid_edge * 1.5f),
                                new Vector3(cellPos.X + grid_edge * 1.5f, cellPos.Y - grid_edge * 0.5f, cellPos.Z - grid_edge * 0.5f),
                                radius))
            {
                cells.Add(new Parallelepiped(Vector3.Add(cellPos, new Vector3(grid_edge, -grid_edge, -grid_edge)), grid_edge, -1));
            }

            //top_left
            if (checkForSphereBoxIntersection(
                                new Vector3(cellPos.X - grid_edge * 1.5f, cellPos.Y + grid_edge * 0.5f, cellPos.Z - grid_edge * 0.5f),
                                new Vector3(cellPos.X - grid_edge * 0.5f, cellPos.Y + grid_edge * 1.5f, cellPos.Z + grid_edge * 0.5f),
                                radius))
            {
                cells.Add(new Parallelepiped(Vector3.Add(cellPos, new Vector3(-grid_edge, grid_edge, 0f)), grid_edge, -1));
            }

            //top_left_near
            if (checkForSphereBoxIntersection(
                                new Vector3(cellPos.X - grid_edge * 1.5f, cellPos.Y + grid_edge * 0.5f, cellPos.Z + grid_edge * 0.5f),
                                new Vector3(cellPos.X - grid_edge * 0.5f, cellPos.Y + grid_edge * 1.5f, cellPos.Z + grid_edge * 1.5f),
                                radius))
            {
                cells.Add(new Parallelepiped(Vector3.Add(cellPos, new Vector3(-grid_edge, grid_edge, grid_edge)), grid_edge, -1));
            }

            //top_left_far
            if (checkForSphereBoxIntersection(
                                new Vector3(cellPos.X - grid_edge * 1.5f, cellPos.Y + grid_edge * 0.5f, cellPos.Z - grid_edge * 1.5f),
                                new Vector3(cellPos.X - grid_edge * 0.5f, cellPos.Y + grid_edge * 1.5f, cellPos.Z - grid_edge * 0.5f),
                                radius))
            {
                cells.Add(new Parallelepiped(Vector3.Add(cellPos, new Vector3(-grid_edge, grid_edge, -grid_edge)), grid_edge, -1));
            }

            //top_right
            if (checkForSphereBoxIntersection(
                                new Vector3(cellPos.X + grid_edge * 0.5f, cellPos.Y + grid_edge * 0.5f, cellPos.Z - grid_edge * 0.5f),
                                new Vector3(cellPos.X + grid_edge * 1.5f, cellPos.Y + grid_edge * 1.5f, cellPos.Z + grid_edge * 0.5f),
                                radius))
            {
                cells.Add(new Parallelepiped(Vector3.Add(cellPos, new Vector3(grid_edge, grid_edge, 0f)), grid_edge, -1));
            }

            //top_right_near
            if (checkForSphereBoxIntersection(
                                new Vector3(cellPos.X + grid_edge * 0.5f, cellPos.Y + grid_edge * 0.5f, cellPos.Z + grid_edge * 0.5f),
                                new Vector3(cellPos.X + grid_edge * 1.5f, cellPos.Y + grid_edge * 1.5f, cellPos.Z + grid_edge * 1.5f),
                                radius))
            {
                cells.Add(new Parallelepiped(Vector3.Add(cellPos, new Vector3(grid_edge, grid_edge, grid_edge)), grid_edge, -1));
            }

            //top_right_far
            if (checkForSphereBoxIntersection(
                                new Vector3(cellPos.X + grid_edge * 0.5f, cellPos.Y + grid_edge * 0.5f, cellPos.Z - grid_edge * 1.5f),
                                new Vector3(cellPos.X + grid_edge * 1.5f, cellPos.Y + grid_edge * 1.5f, cellPos.Z - grid_edge * 0.5f),
                                radius))
            {
                cells.Add(new Parallelepiped(Vector3.Add(cellPos, new Vector3(grid_edge, grid_edge, -grid_edge)), grid_edge, -1));
            }

            //top_near
            if (checkForSphereBoxIntersection(
                                new Vector3(cellPos.X - grid_edge * 0.5f, cellPos.Y + grid_edge * 0.5f, cellPos.Z + grid_edge * 0.5f),
                                new Vector3(cellPos.X + grid_edge * 0.5f, cellPos.Y + grid_edge * 1.5f, cellPos.Z + grid_edge * 1.5f),
                                radius))
            {
                cells.Add(new Parallelepiped(Vector3.Add(cellPos, new Vector3(0f, grid_edge, grid_edge)), grid_edge, -1));
            }

            //bottom_near
            if (checkForSphereBoxIntersection(
                                new Vector3(cellPos.X - grid_edge * 0.5f, cellPos.Y - grid_edge * 1.5f, cellPos.Z + grid_edge * 0.5f),
                                new Vector3(cellPos.X + grid_edge * 0.5f, cellPos.Y - grid_edge * 0.5f, cellPos.Z + grid_edge * 1.5f),
                                radius))
            {
                cells.Add(new Parallelepiped(Vector3.Add(cellPos, new Vector3(0f, -grid_edge, grid_edge)), grid_edge, -1));
            }

            //top_far
            if (checkForSphereBoxIntersection(
                                new Vector3(cellPos.X - grid_edge * 0.5f, cellPos.Y + grid_edge * 0.5f, cellPos.Z - grid_edge * 1.5f),
                                new Vector3(cellPos.X + grid_edge * 0.5f, cellPos.Y + grid_edge * 1.5f, cellPos.Z - grid_edge * 0.5f),
                                radius))
            {
                cells.Add(new Parallelepiped(Vector3.Add(cellPos, new Vector3(0f, grid_edge, -grid_edge)), grid_edge, -1));
            }

            //bottom_far
            if (checkForSphereBoxIntersection(
                                new Vector3(cellPos.X - grid_edge * 0.5f, cellPos.Y - grid_edge * 1.5f, cellPos.Z - grid_edge * 1.5f),
                                new Vector3(cellPos.X + grid_edge * 0.5f, cellPos.Y - grid_edge * 0.5f, cellPos.Z - grid_edge * 0.5f),
                                radius))
            {
                cells.Add(new Parallelepiped(Vector3.Add(cellPos, new Vector3(0f, -grid_edge, -grid_edge)), grid_edge, -1));
            }

            //left_far
            if (checkForSphereBoxIntersection(
                                new Vector3(cellPos.X - grid_edge * 1.5f, cellPos.Y - grid_edge * 0.5f, cellPos.Z - grid_edge * 1.5f),
                                new Vector3(cellPos.X - grid_edge * 0.5f, cellPos.Y + grid_edge * 0.5f, cellPos.Z - grid_edge * 0.5f),
                                radius))
            {
                cells.Add(new Parallelepiped(Vector3.Add(cellPos, new Vector3(-grid_edge, 0f, -grid_edge)), grid_edge, -1));
            }

            //right_far
            if (checkForSphereBoxIntersection(
                                new Vector3(cellPos.X + grid_edge * 0.5f, cellPos.Y - grid_edge * 0.5f, cellPos.Z - grid_edge * 1.5f),
                                new Vector3(cellPos.X + grid_edge * 1.5f, cellPos.Y + grid_edge * 0.5f, cellPos.Z - grid_edge * 0.5f),
                                radius))
            {
                cells.Add(new Parallelepiped(Vector3.Add(cellPos, new Vector3(grid_edge, 0f, -grid_edge)), grid_edge, -1));
            }

            //left_near
            if (checkForSphereBoxIntersection(
                                new Vector3(cellPos.X - grid_edge * 1.5f, cellPos.Y - grid_edge * 0.5f, cellPos.Z + grid_edge * 0.5f),
                                new Vector3(cellPos.X - grid_edge * 0.5f, cellPos.Y + grid_edge * 0.5f, cellPos.Z + grid_edge * 1.5f),
                                radius))
            {
                cells.Add(new Parallelepiped(Vector3.Add(cellPos, new Vector3(-grid_edge, 0f, grid_edge)), grid_edge, -1));
            }

            //right_near
            if (checkForSphereBoxIntersection(
                                new Vector3(cellPos.X + grid_edge * 0.5f, cellPos.Y - grid_edge * 0.5f, cellPos.Z + grid_edge * 0.5f),
                                new Vector3(cellPos.X + grid_edge * 1.5f, cellPos.Y + grid_edge * 0.5f, cellPos.Z + grid_edge * 1.5f),
                                radius))
            {
                cells.Add(new Parallelepiped(Vector3.Add(cellPos, new Vector3(grid_edge, 0f, grid_edge)), grid_edge, -1));
            }
#endregion

            checkCellType(cellPos.X, cellPos.Y, cellPos.Z, true);
            hashCell(cellPos, 0);
            for (int i = 1; i < 8; i++)
            {
                if (i >= cells.Count)
                {
                    cellArray[i] = 0;
                    continue;
                }
                Body cell = cells.ElementAt(i);
                checkCellType(cell.getPos().X, cell.getPos().Y, cell.getPos().Z, false);
                hashCell(cell.getPos(), i);
            }
        }

        public void hashCell(Vector3 hcp, int index)
        {
            float ge = (float)Program.window.grid_edge;
            cellArray[index] = (((uint)((hcp.X + 10) / ge) << XSHIFT) |
                                ((uint)((hcp.Y + 10) / ge) << YSHIFT) |
                                ((uint)((hcp.Z + 10) / ge) << ZSHIFT)) + (uint)1;
        }

        bool checkForSphereBoxIntersection(Vector3 c1,  Vector3 c2, float radius)
        {
            float dist_squared = radius * radius;
            if (pos.X < c1.X) dist_squared -= (pos.X - c1.X)*(pos.X - c1.X);
            else if (pos.X > c2.X) dist_squared -= (pos.X - c2.X)*(pos.X - c2.X);
            if (pos.Y < c1.Y) dist_squared -= (pos.Y - c1.Y)*(pos.Y - c1.Y);
            else if (pos.Y > c2.Y) dist_squared -= (pos.Y - c2.Y)*(pos.Y - c2.Y);
            if (pos.Z < c1.Z) dist_squared -= (pos.Z - c1.Z)*(pos.Z - c1.Z);
            else if (pos.Z > c2.Z) dist_squared -= (pos.Z - c2.Z)*(pos.Z - c2.Z);
            return dist_squared > 0;
        }

        public void checkCellType(float posX, float posY, float posZ, bool homeCell)
        {
            float grid_edge = Program.window.grid_edge;
            uint index;
            float pos_X = posX + 10f;
            float pos_Y = -(posY - 10f);
            float pos_Z = -(posZ - 10f);
            float dge = grid_edge * 2f;

            //case 1
            if (pos_X % (dge) <= grid_edge && pos_Y % (dge) <= grid_edge && pos_Z % (dge) <= grid_edge)
            {
                ctrl_bits |= ICType1;
                index = 1;
            }
            //case 2
            else if (pos_X % (dge) > grid_edge && pos_Y % (dge) <= grid_edge && pos_Z % (dge) <= grid_edge)
            {
                ctrl_bits |= ICType2;
                index = 2;
            }
            //case 3
            else if (pos_X % (dge) <= grid_edge && pos_Y % (dge) > grid_edge && pos_Z % (dge) <= grid_edge)
            {
                ctrl_bits |= ICType3;
                index = 3;
            }
            //case 4
            else if (pos_X % (dge) > grid_edge && pos_Y % (dge) > grid_edge && pos_Z % (dge) <= grid_edge)
            {
                ctrl_bits |= ICType4;
                index = 4;
            }
            //case 5
            else if (pos_X % (dge) <= grid_edge && pos_Y % (dge) <= grid_edge && pos_Z % (dge) > grid_edge)
            {
                ctrl_bits |= ICType5;
                index = 5;
            }
            //case 6
            else if (pos_X % (dge) > grid_edge && pos_Y % (dge) <= grid_edge && pos_Z % (dge) > grid_edge)
            {
                ctrl_bits |= ICType6;
                index = 6;
            }
            //case 7
            else if (pos_X % (dge) <= grid_edge && pos_Y % (dge) > grid_edge && pos_Z % (dge) > grid_edge)
            {
                ctrl_bits |= ICType7;
                index = 7;
            }
            //case 8
            else
            {
                ctrl_bits |= ICType8;
                index = 8;
            }

            if (homeCell)
                ctrl_bits |= index << 8;
        }
    }
}
