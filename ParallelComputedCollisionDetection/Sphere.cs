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
        public Vector3 cellPos;
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
        public uint cTypesIntersected;
        public int bodyIndex;
        public uint[] cellArray = new uint[8];
        //public bool[] cellsIntersected = new bool[8];
        public List<Body> cells;

        public const int XSHIFT = 0;
        public const int YSHIFT = 3;
        public const int ZSHIFT = 6;
        public const uint intersectCType1 = 1;
        public const uint intersectCType2 = 2;
        public const uint intersectCType3 = 4;
        public const uint intersectCType4 = 8;
        public const uint intersectCType5 = 16;
        public const uint intersectCType6 = 32;
        public const uint intersectCType7 = 64;
        public const uint intersectCType8 = 128;

        public Sphere(Vector3 pos, double radius, int slices, int stacks, int bodyIndex)
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
            this.cellPos = Vector3.Zero;
            cells = new List<Body>();

            for (int i = 0; i < 8; i++)
                cellArray[i] |= (uint)bodyIndex << 5;
            checkCellTypes();
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

        /*public void checkHomeCellType()
        {
            double half_fov = Window.fov * 0.5;
            cellArray[0] |= 1;

            if ((int)(-(pos.Z - half_fov) / Program.window.grid_edge) % 2 == 0)
            {
                if ((int)(-(pos.X - half_fov) / Program.window.grid_edge) % 2 == 0)
                {
                    if ((int)(-(pos.Y - half_fov) / Program.window.grid_edge) % 2 == 0)
                    {
                        hCell = 0;
                        cellArray[0] |= intersectCType1;
                    }
                    else
                    {
                        hCell = 2;
                        cellArray[0] |= intersectCType3;
                    }
                }
                else
                {
                    if ((int)(-(pos.Y - half_fov) / Program.window.grid_edge) % 2 == 0)
                    {
                        hCell = 1;
                        cellArray[0] |= intersectCType2;
                    }
                    else
                    {
                        hCell = 3;
                        cellArray[0] |= intersectCType4;
                    }
                }
            }
            else
            {
                if ((int)(-(pos.X - half_fov) / Program.window.grid_edge) % 2 == 0)
                {
                    if ((int)(-(pos.Y - half_fov) / Program.window.grid_edge) % 2 == 0)
                    {
                        hCell = 4;
                        cellArray[0] |= intersectCType5;
                    }
                    else
                    {
                        hCell = 6;
                        cellArray[0] |= intersectCType7;
                    }
                }
                else
                {
                    if ((int)(-(pos.Y - half_fov) / Program.window.grid_edge) % 2 == 0)
                    {
                        hCell = 5;
                        cellArray[0] |= intersectCType6;
                    }
                    else
                    {
                        hCell = 7;
                        cellArray[0] |= intersectCType8;
                    }
                }
            }
            cellsIntersected[hCell] = true;
        }*/

        public void checkForCellIntersection()
        {
            cells.Clear();
            cTypesIntersected = 0;

            float grid_edge = (float)Program.window.grid_edge;
            if(pos.X>=0)
                cellPos.X = ((int)((this.pos.X + grid_edge * 0.5f) / grid_edge)) * grid_edge;
            else
                cellPos.X = ((int)((this.pos.X - grid_edge * 0.5f) / grid_edge)) * grid_edge;
            if (pos.Y >= 0)
                cellPos.Y = ((int)((this.pos.Y + grid_edge * 0.5f) / grid_edge)) * grid_edge;
            else
                cellPos.Y = ((int)((this.pos.Y - grid_edge * 0.5f) / grid_edge)) * grid_edge;
            if (pos.Z >= 0)
                cellPos.Z = ((int)((this.pos.Z + grid_edge * 0.5f) / grid_edge)) * grid_edge;
            else
                cellPos.Z = ((int)((this.pos.Z - grid_edge * 0.5f) / grid_edge)) * grid_edge;

            //hCell
            cells.Add(new Parallelepiped(cellPos, grid_edge, -1));
            checkCellType(cellPos.X, cellPos.Y, cellPos.Z, true);
            hashHCell(cellPos);


            /*Vector3 pos = Window.bodies.ElementAt<Body>((int)bodyIndex).getPos();
            cellPos.X = (int)(pos.X / grid_edge) * grid_edge + grid_edge * 0.5f;
            cellPos.Y = (int)(pos.Y / grid_edge) * grid_edge + grid_edge * 0.5f;
            cellPos.Z = (int)(pos.Z / grid_edge) * grid_edge + grid_edge * 0.5f;*/
            
            #region Check For Collisions
            //right
            if (checkForSphereBoxIntersection(
                                new Vector3(cellPos.X + grid_edge * 0.5f, cellPos.Y - grid_edge * 0.5f, cellPos.Z - grid_edge * 0.5f),
                                new Vector3(cellPos.X + grid_edge * 1.5f, cellPos.Y + grid_edge * 0.5f, cellPos.Z + grid_edge * 0.5f),
                                pos, (float)radius))
            {
                cells.Add(new Parallelepiped(Vector3.Add(cellPos, new Vector3(grid_edge, 0f, 0f)), grid_edge, -1));
            }

            //left
            if (checkForSphereBoxIntersection(
                                new Vector3(cellPos.X - grid_edge * 1.5f, cellPos.Y - grid_edge * 0.5f, cellPos.Z - grid_edge * 0.5f),
                                new Vector3(cellPos.X - grid_edge * 0.5f, cellPos.Y + grid_edge * 0.5f, cellPos.Z + grid_edge * 0.5f),
                                pos, (float)radius))
            {
                cells.Add(new Parallelepiped(Vector3.Add(cellPos, new Vector3(-grid_edge, 0f, 0f)), grid_edge, -1));
            }

            //top
            if (checkForSphereBoxIntersection(
                                new Vector3(cellPos.X - grid_edge * 0.5f, cellPos.Y + grid_edge * 0.5f, cellPos.Z - grid_edge * 0.5f),
                                new Vector3(cellPos.X + grid_edge * 0.5f, cellPos.Y + grid_edge * 1.5f, cellPos.Z + grid_edge * 0.5f),
                                pos, (float)radius))
            {
                cells.Add(new Parallelepiped(Vector3.Add(cellPos, new Vector3(0f, grid_edge, 0f)), grid_edge, -1));
            }

            //bottom
            if (checkForSphereBoxIntersection(
                                new Vector3(cellPos.X - grid_edge * 0.5f, cellPos.Y - grid_edge * 1.5f, cellPos.Z - grid_edge * 0.5f),
                                new Vector3(cellPos.X + grid_edge * 0.5f, cellPos.Y - grid_edge * 0.5f, cellPos.Z + grid_edge * 0.5f),
                                pos, (float)radius))
            {
                cells.Add(new Parallelepiped(Vector3.Add(cellPos, new Vector3(0f, -grid_edge, 0f)), grid_edge, -1));
            }

            //near
            if (checkForSphereBoxIntersection(
                                new Vector3(cellPos.X - grid_edge * 0.5f, cellPos.Y - grid_edge * 0.5f, cellPos.Z + grid_edge * 0.5f),
                                new Vector3(cellPos.X + grid_edge * 0.5f, cellPos.Y + grid_edge * 0.5f, cellPos.Z + grid_edge * 1.5f),
                                pos, (float)radius))
            {
                cells.Add(new Parallelepiped(Vector3.Add(cellPos, new Vector3(0f, 0f, grid_edge)), grid_edge, -1));
            }

            //far
            if (checkForSphereBoxIntersection(
                                new Vector3(cellPos.X - grid_edge * 0.5f, cellPos.Y - grid_edge * 0.5f, cellPos.Z - grid_edge * 1.5f),
                                new Vector3(cellPos.X + grid_edge * 0.5f, cellPos.Y + grid_edge * 0.5f, cellPos.Z - grid_edge * 0.5f),
                                pos, (float)radius))
            {
                cells.Add(new Parallelepiped(Vector3.Add(cellPos, new Vector3(0f, 0f, -grid_edge)), -grid_edge, -1));
            }

            //bottom_left
            if (checkForSphereBoxIntersection(
                                new Vector3(cellPos.X - grid_edge * 1.5f, cellPos.Y - grid_edge * 1.5f, cellPos.Z - grid_edge * 0.5f),
                                new Vector3(cellPos.X - grid_edge * 0.5f, cellPos.Y - grid_edge * 0.5f, cellPos.Z + grid_edge * 0.5f),
                                pos, (float)radius))
            {
                cells.Add(new Parallelepiped(Vector3.Add(cellPos, new Vector3(-grid_edge, -grid_edge, 0f)), grid_edge, -1));
            }
            //bottom_left_near
            if (checkForSphereBoxIntersection(
                                new Vector3(cellPos.X - grid_edge * 1.5f, cellPos.Y - grid_edge * 1.5f, cellPos.Z + grid_edge * 0.5f),
                                new Vector3(cellPos.X - grid_edge * 0.5f, cellPos.Y - grid_edge * 0.5f, cellPos.Z + grid_edge * 1.5f),
                                pos, (float)radius))
            {
                cells.Add(new Parallelepiped(Vector3.Add(cellPos, new Vector3(-grid_edge, -grid_edge, grid_edge)), grid_edge, -1));
            }

            //bottom_left_far
            if (checkForSphereBoxIntersection(
                                new Vector3(cellPos.X - grid_edge * 1.5f, cellPos.Y - grid_edge * 1.5f, cellPos.Z - grid_edge * 1.5f),
                                new Vector3(cellPos.X - grid_edge * 0.5f, cellPos.Y - grid_edge * 0.5f, cellPos.Z - grid_edge * 0.5f),
                                pos, (float)radius))
            {
                cells.Add(new Parallelepiped(Vector3.Add(cellPos, new Vector3(-grid_edge, -grid_edge, -grid_edge)), grid_edge, -1));
            }

            //bottom_right
            if (checkForSphereBoxIntersection(
                                new Vector3(cellPos.X + grid_edge * 0.5f, cellPos.Y - grid_edge * 1.5f, cellPos.Z - grid_edge * 0.5f),
                                new Vector3(cellPos.X + grid_edge * 1.5f, cellPos.Y - grid_edge * 0.5f, cellPos.Z + grid_edge * 0.5f),
                                pos, (float)radius))
            {
                cells.Add(new Parallelepiped(Vector3.Add(cellPos, new Vector3(grid_edge, -grid_edge, 0f)), grid_edge, -1));
            }

            //bottom_right_near
            if (checkForSphereBoxIntersection(
                                new Vector3(cellPos.X + grid_edge * 0.5f, cellPos.Y - grid_edge * 1.5f, cellPos.Z + grid_edge * 0.5f),
                                new Vector3(cellPos.X + grid_edge * 1.5f, cellPos.Y - grid_edge * 0.5f, cellPos.Z + grid_edge * 1.5f),
                                pos, (float)radius))
            {
                cells.Add(new Parallelepiped(Vector3.Add(cellPos, new Vector3(grid_edge, -grid_edge, grid_edge)), grid_edge, -1));
            }

            //bottom_right_far
            if (checkForSphereBoxIntersection(
                                new Vector3(cellPos.X + grid_edge * 0.5f, cellPos.Y - grid_edge * 1.5f, cellPos.Z - grid_edge * 1.5f),
                                new Vector3(cellPos.X + grid_edge * 1.5f, cellPos.Y - grid_edge * 0.5f, cellPos.Z - grid_edge * 0.5f),
                                pos, (float)radius))
            {
                cells.Add(new Parallelepiped(Vector3.Add(cellPos, new Vector3(grid_edge, -grid_edge, -grid_edge)), grid_edge, -1));
            }

            //top_left
            if (checkForSphereBoxIntersection(
                                new Vector3(cellPos.X - grid_edge * 1.5f, cellPos.Y + grid_edge * 0.5f, cellPos.Z - grid_edge * 0.5f),
                                new Vector3(cellPos.X - grid_edge * 0.5f, cellPos.Y + grid_edge * 1.5f, cellPos.Z + grid_edge * 0.5f),
                                pos, (float)radius))
            {
                cells.Add(new Parallelepiped(Vector3.Add(cellPos, new Vector3(-grid_edge, grid_edge, 0f)), grid_edge, -1));
            }

            //top_left_near
            if (checkForSphereBoxIntersection(
                                new Vector3(cellPos.X - grid_edge * 1.5f, cellPos.Y + grid_edge * 0.5f, cellPos.Z + grid_edge * 0.5f),
                                new Vector3(cellPos.X - grid_edge * 0.5f, cellPos.Y + grid_edge * 1.5f, cellPos.Z + grid_edge * 1.5f),
                                pos, (float)radius))
            {
                cells.Add(new Parallelepiped(Vector3.Add(cellPos, new Vector3(-grid_edge, grid_edge, grid_edge)), grid_edge, -1));
            }

            //top_left_far
            if (checkForSphereBoxIntersection(
                                new Vector3(cellPos.X - grid_edge * 1.5f, cellPos.Y + grid_edge * 0.5f, cellPos.Z - grid_edge * 1.5f),
                                new Vector3(cellPos.X - grid_edge * 0.5f, cellPos.Y + grid_edge * 1.5f, cellPos.Z - grid_edge * 0.5f),
                                pos, (float)radius))
            {
                cells.Add(new Parallelepiped(Vector3.Add(cellPos, new Vector3(-grid_edge, grid_edge, -grid_edge)), grid_edge, -1));
            }

            //top_right
            if (checkForSphereBoxIntersection(
                                new Vector3(cellPos.X + grid_edge * 0.5f, cellPos.Y + grid_edge * 0.5f, cellPos.Z - grid_edge * 0.5f),
                                new Vector3(cellPos.X + grid_edge * 1.5f, cellPos.Y + grid_edge * 1.5f, cellPos.Z + grid_edge * 0.5f),
                                pos, (float)radius))
            {
                cells.Add(new Parallelepiped(Vector3.Add(cellPos, new Vector3(grid_edge, grid_edge, 0f)), grid_edge, -1));
            }

            //top_right_near
            if (checkForSphereBoxIntersection(
                                new Vector3(cellPos.X + grid_edge * 0.5f, cellPos.Y + grid_edge * 0.5f, cellPos.Z + grid_edge * 0.5f),
                                new Vector3(cellPos.X + grid_edge * 1.5f, cellPos.Y + grid_edge * 1.5f, cellPos.Z + grid_edge * 1.5f),
                                pos, (float)radius))
            {
                cells.Add(new Parallelepiped(Vector3.Add(cellPos, new Vector3(grid_edge, grid_edge, grid_edge)), grid_edge, -1));
            }

            //top_right_far
            if (checkForSphereBoxIntersection(
                                new Vector3(cellPos.X + grid_edge * 0.5f, cellPos.Y + grid_edge * 0.5f, cellPos.Z - grid_edge * 1.5f),
                                new Vector3(cellPos.X + grid_edge * 1.5f, cellPos.Y + grid_edge * 1.5f, cellPos.Z - grid_edge * 0.5f),
                                pos, (float)radius))
            {
                cells.Add(new Parallelepiped(Vector3.Add(cellPos, new Vector3(grid_edge, grid_edge, -grid_edge)), grid_edge, -1));
            }

            //top_near
            if (checkForSphereBoxIntersection(
                                new Vector3(cellPos.X - grid_edge * 0.5f, cellPos.Y + grid_edge * 0.5f, cellPos.Z + grid_edge * 0.5f),
                                new Vector3(cellPos.X + grid_edge * 0.5f, cellPos.Y + grid_edge * 1.5f, cellPos.Z + grid_edge * 1.5f),
                                pos, (float)radius))
            {
                cells.Add(new Parallelepiped(Vector3.Add(cellPos, new Vector3(0f, grid_edge, grid_edge)), grid_edge, -1));
            }

            //bottom_near
            if (checkForSphereBoxIntersection(
                                new Vector3(cellPos.X - grid_edge * 0.5f, cellPos.Y - grid_edge * 1.5f, cellPos.Z + grid_edge * 0.5f),
                                new Vector3(cellPos.X + grid_edge * 0.5f, cellPos.Y - grid_edge * 0.5f, cellPos.Z + grid_edge * 1.5f),
                                pos, (float)radius))
            {
                cells.Add(new Parallelepiped(Vector3.Add(cellPos, new Vector3(0f, -grid_edge, grid_edge)), grid_edge, -1));
            }

            //top_far
            if (checkForSphereBoxIntersection(
                                new Vector3(cellPos.X - grid_edge * 0.5f, cellPos.Y + grid_edge * 0.5f, cellPos.Z - grid_edge * 1.5f),
                                new Vector3(cellPos.X + grid_edge * 0.5f, cellPos.Y + grid_edge * 1.5f, cellPos.Z - grid_edge * 0.5f),
                                pos, (float)radius))
            {
                cells.Add(new Parallelepiped(Vector3.Add(cellPos, new Vector3(0f, grid_edge, -grid_edge)), grid_edge, -1));
            }

            //bottom_far
            if (checkForSphereBoxIntersection(
                                new Vector3(cellPos.X - grid_edge * 0.5f, cellPos.Y - grid_edge * 1.5f, cellPos.Z - grid_edge * 1.5f),
                                new Vector3(cellPos.X + grid_edge * 0.5f, cellPos.Y - grid_edge * 0.5f, cellPos.Z - grid_edge * 0.5f),
                                pos, (float)radius))
            {
                cells.Add(new Parallelepiped(Vector3.Add(cellPos, new Vector3(0f, -grid_edge, -grid_edge)), grid_edge, -1));
            }

            //left_far
            if (checkForSphereBoxIntersection(
                                new Vector3(cellPos.X - grid_edge * 1.5f, cellPos.Y - grid_edge * 0.5f, cellPos.Z - grid_edge * 1.5f),
                                new Vector3(cellPos.X - grid_edge * 0.5f, cellPos.Y + grid_edge * 0.5f, cellPos.Z - grid_edge * 0.5f),
                                pos, (float)radius))
            {
                cells.Add(new Parallelepiped(Vector3.Add(cellPos, new Vector3(-grid_edge, 0f, -grid_edge)), grid_edge, -1));
            }

            //right_far
            if (checkForSphereBoxIntersection(
                                new Vector3(cellPos.X + grid_edge * 0.5f, cellPos.Y - grid_edge * 0.5f, cellPos.Z - grid_edge * 1.5f),
                                new Vector3(cellPos.X + grid_edge * 1.5f, cellPos.Y + grid_edge * 0.5f, cellPos.Z - grid_edge * 0.5f),
                                pos, (float)radius))
            {
                cells.Add(new Parallelepiped(Vector3.Add(cellPos, new Vector3(grid_edge, 0f, -grid_edge)), grid_edge, -1));
            }

            //left_near
            if (checkForSphereBoxIntersection(
                                new Vector3(cellPos.X - grid_edge * 1.5f, cellPos.Y - grid_edge * 0.5f, cellPos.Z + grid_edge * 0.5f),
                                new Vector3(cellPos.X - grid_edge * 0.5f, cellPos.Y + grid_edge * 0.5f, cellPos.Z + grid_edge * 1.5f),
                                pos, (float)radius))
            {
                cells.Add(new Parallelepiped(Vector3.Add(cellPos, new Vector3(-grid_edge, 0f, grid_edge)), grid_edge, -1));
            }

            //right_near
            if (checkForSphereBoxIntersection(
                                new Vector3(cellPos.X + grid_edge * 0.5f, cellPos.Y - grid_edge * 0.5f, cellPos.Z + grid_edge * 0.5f),
                                new Vector3(cellPos.X + grid_edge * 1.5f, cellPos.Y + grid_edge * 0.5f, cellPos.Z + grid_edge * 1.5f),
                                pos, (float)radius))
            {
                cells.Add(new Parallelepiped(Vector3.Add(cellPos, new Vector3(grid_edge, 0f, grid_edge)), grid_edge, -1));
            }
#endregion

            checkCellTypes();
        }

        public void hashHCell(Vector3 hcp)
        {
            double ge = Program.window.grid_edge;
            hCell = ((uint)(hcp.X / ge) << XSHIFT) |
                    ((uint)(hcp.Y /ge) << YSHIFT) |
                    ((uint)(hcp.Z / ge) << ZSHIFT);
        }

        bool checkForSphereBoxIntersection(Vector3 c1,  Vector3 c2, Vector3 sPos, float radius)
        {
            float dist_squared = radius * radius;
            if (sPos.X < c1.X) dist_squared -= (float)Math.Pow(sPos.X - c1.X, 2);
            else if (sPos.X > c2.X) dist_squared -= (float)Math.Pow(sPos.X - c2.X, 2);
            if (sPos.Y < c1.Y) dist_squared -= (float)Math.Pow(sPos.Y - c1.Y, 2);
            else if (sPos.Y > c2.Y) dist_squared -= (float)Math.Pow(sPos.Y - c2.Y, 2);
            if (sPos.Z < c1.Z) dist_squared -= (float)Math.Pow(sPos.Z - c1.Z, 2);
            else if (sPos.Z > c2.Z) dist_squared -= (float)Math.Pow(sPos.Z - c2.Z, 2);
            return dist_squared > 0;
        }

        /*public void checkCellType()
        {
            cellsIntersected = new bool[8];
            double half_fov = Window.fov * 0.5;
            foreach (Body cell in cells)
            {
                Vector3 pos = cell.getPos();
                double half_fov_ = Window.fov * 0.5;
                double grid_edge = Program.window.grid_edge;
                if ((int)(-(pos.Z - half_fov_) / grid_edge) % 2 == 0)
                {
                    if ((int)(-(pos.X - half_fov_) / grid_edge) % 2 == 0)
                    {
                        if ((int)(-(pos.Y - half_fov_) / grid_edge) % 2 == 0)
                        {
                            cellsIntersected[0] = true;
                        }
                        else
                        {
                            cellsIntersected[2] = true;
                        }
                    }
                    else
                    {
                        if ((int)(-(pos.Y - half_fov_) / grid_edge) % 2 == 0)
                        {
                            cellsIntersected[1] = true;
                        }
                        else
                        {
                            cellsIntersected[3] = true;
                        }
                    }
                }
                else
                {
                    if ((int)(-(pos.X - half_fov_) / grid_edge) % 2 == 0)
                    {
                        if ((int)(-(pos.Y - half_fov_) / grid_edge) % 2 == 0)
                        {
                            cellsIntersected[4] = true;
                        }
                        else
                        {
                            cellsIntersected[6] = true;
                        }
                    }
                    else
                    {
                        if ((int)(-(pos.Y - half_fov_) / grid_edge) % 2 == 0)
                        {
                            cellsIntersected[5] = true;
                        }
                        else
                        {
                            cellsIntersected[7] = true;
                        }
                    }
                }
            }
        }*/

        public void checkCellTypes()
        {
            foreach (Body cell in cells)
            {
                checkCellType(cell.getPos().X, cell.getPos().Y, cell.getPos().Z, false);
            }
        }

        public void checkCellType(float posX, float posY, float posZ, bool homeCell)
        {
            double grid_edge = Program.window.grid_edge;
            uint index;
            posX += 10;
            posY = -(posY - 10);
            posZ = -(posZ - 10);

            //case 1
            if (posX % (2 * grid_edge) <= grid_edge && posY % (2 * grid_edge) <= grid_edge && posZ % (2 * grid_edge) <= grid_edge)
            {
                cTypesIntersected |= intersectCType1;
                index = 1;
            }
            //case 2
            else if (posX % (2 * grid_edge) > grid_edge && posY % (2 * grid_edge) <= grid_edge && posZ % (2 * grid_edge) <= grid_edge)
            {
                cTypesIntersected |= intersectCType2;
                index = 2;
            }
            //case 3
            else if (posX % (2 * grid_edge) <= grid_edge && posY % (2 * grid_edge) > grid_edge && posZ % (2 * grid_edge) <= grid_edge)
            {
                cTypesIntersected |= intersectCType3;
                index = 3;
            }
            //case 4
            else if (posX % (2 * grid_edge) > grid_edge && posY % (2 * grid_edge) > grid_edge && posZ % (2 * grid_edge) <= grid_edge)
            {
                cTypesIntersected |= intersectCType4;
                index = 4;
            }
            //case 5
            else if (posX % (2 * grid_edge) <= grid_edge && posY % (2 * grid_edge) <= grid_edge && posZ % (2 * grid_edge) > grid_edge)
            {
                cTypesIntersected |= intersectCType5;
                index = 5;
            }
            //case 6
            else if (posX % (2 * grid_edge) > grid_edge && posY % (2 * grid_edge) <= grid_edge && posZ % (2 * grid_edge) > grid_edge)
            {
                cTypesIntersected |= intersectCType6;
                index = 6;
            }
            //case 7
            else if (posX % (2 * grid_edge) <= grid_edge && posY % (2 * grid_edge) > grid_edge && posZ % (2 * grid_edge) > grid_edge)
            {
                cTypesIntersected |= intersectCType7;
                index = 7;
            }
            //case 8
            else
            {
                cTypesIntersected |= intersectCType8;
                index = 8;
            }
            if (homeCell)
                hCell = index;
        }
    }
}
