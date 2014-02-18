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
        public uint cellTypesIntersected;
        public int bodyIndex;
        public uint[] cellArray = new uint[8];
        public bool[] cellsIntersected = new bool[8];
        public List<Body> cells;

        public const uint XSHIFT = 0;
        public const uint YSHIFT = 3;
        public const uint ZSHIFT = 6;
        public const uint intersectCType1 = 1 << 1;
        public const uint intersectCType2 = 2 << 1;
        public const uint intersectCType3 = 4 << 1;
        public const uint intersectCType4 = 8 << 1;
        public const uint intersectCType5 = 16 << 1;
        public const uint intersectCType6 = 32 << 1;
        public const uint intersectCType7 = 64 << 1;
        public const uint intersectCType8 = 128 << 1;

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
            checkHomeCellType();
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
                    if ((int)(-(pos.Y - half_fov) / Window.grid_edge) % 2 == 0)
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
                if ((int)(-(pos.X - half_fov) / Window.grid_edge) % 2 == 0)
                {
                    if ((int)(-(pos.Y - half_fov) / Window.grid_edge) % 2 == 0)
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
                    if ((int)(-(pos.Y - half_fov) / Window.grid_edge) % 2 == 0)
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
        }

        public void checkForCellIntersection()
        {
            cells.Clear();

            float grid_edge = (float)Window.grid_edge;
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

            /*Vector3 pos = Window.bodies.ElementAt<Body>((int)bodyIndex).getPos();
            cellPos.X = (int)(pos.X / grid_edge) * grid_edge + grid_edge * 0.5f;
            cellPos.Y = (int)(pos.Y / grid_edge) * grid_edge + grid_edge * 0.5f;
            cellPos.Z = (int)(pos.Z / grid_edge) * grid_edge + grid_edge * 0.5f;*/

            int count = 0;
            
            #region Check For Collisions
            //right
            if (cellPos.X < (10 - grid_edge) && /*(cellPos.X + grid_edge * 0.5 < pos.X + radius)*/checkForSphereCubeIntersection(
                                new Vector3(cellPos.X + grid_edge * 0.5f, cellPos.Y - grid_edge * 0.5f, cellPos.Z - grid_edge * 0.5f),
                                new Vector3(cellPos.X + grid_edge * 1.5f, cellPos.Y + grid_edge * 0.5f, cellPos.Z + grid_edge * 0.5f),
                                pos, (float)radius))
            {
                cells.Add(new Parallelepiped(Vector3.Add(cellPos, new Vector3(grid_edge, 0f, 0f)), grid_edge, -1));
                count++;
            }

            //left
            if (cellPos.X > (-10 + grid_edge) && /*(cellPos.X - grid_edge * 0.5 > pos.X - radius)*/checkForSphereCubeIntersection(
                                new Vector3(cellPos.X - grid_edge * 1.5f, cellPos.Y - grid_edge * 0.5f, cellPos.Z - grid_edge * 0.5f),
                                new Vector3(cellPos.X - grid_edge * 0.5f, cellPos.Y + grid_edge * 0.5f, cellPos.Z + grid_edge * 0.5f),
                                pos, (float)radius))
            {
                cells.Add(new Parallelepiped(Vector3.Add(cellPos, new Vector3(-grid_edge, 0f, 0f)), grid_edge, -1));
                count++;
            }

            //top
            if (cellPos.Y < (10 - grid_edge) && /*(cellPos.Y + grid_edge * 0.5 < pos.Y + radius)*/checkForSphereCubeIntersection(
                                new Vector3(cellPos.X - grid_edge * 0.5f, cellPos.Y + grid_edge * 0.5f, cellPos.Z - grid_edge * 0.5f),
                                new Vector3(cellPos.X + grid_edge * 0.5f, cellPos.Y + grid_edge * 1.5f, cellPos.Z + grid_edge * 0.5f),
                                pos, (float)radius))
            {
                cells.Add(new Parallelepiped(Vector3.Add(cellPos, new Vector3(0f, grid_edge, 0f)), grid_edge, -1));
                count++;
            }

            //bottom
            if (cellPos.Y > (-10 + grid_edge) && /*(cellPos.Y - grid_edge * 0.5 > pos.Y - radius)*/checkForSphereCubeIntersection(
                                new Vector3(cellPos.X - grid_edge * 0.5f, cellPos.Y - grid_edge * 1.5f, cellPos.Z - grid_edge * 0.5f),
                                new Vector3(cellPos.X + grid_edge * 0.5f, cellPos.Y - grid_edge * 0.5f, cellPos.Z + grid_edge * 0.5f),
                                pos, (float)radius))
            {
                cells.Add(new Parallelepiped(Vector3.Add(cellPos, new Vector3(0f, -grid_edge, 0f)), grid_edge, -1));
                count++;
            }

            //near
            if (cellPos.Z < (10 - grid_edge) && /*(cellPos.Z - grid_edge * 0.5 > pos.Z - radius)*/checkForSphereCubeIntersection(
                                new Vector3(cellPos.X - grid_edge * 0.5f, cellPos.Y - grid_edge * 0.5f, cellPos.Z + grid_edge * 0.5f),
                                new Vector3(cellPos.X + grid_edge * 0.5f, cellPos.Y + grid_edge * 0.5f, cellPos.Z + grid_edge * 1.5f),
                                pos, (float)radius))
            {
                cells.Add(new Parallelepiped(Vector3.Add(cellPos, new Vector3(0f, 0f, grid_edge)), grid_edge, -1));
                count++;
            }

            //far
            if (cellPos.Z > (-10 + grid_edge) && /*(cellPos.Z + grid_edge * 0.5 < pos.Z + radius)*/checkForSphereCubeIntersection(
                                new Vector3(cellPos.X - grid_edge * 0.5f, cellPos.Y - grid_edge * 0.5f, cellPos.Z - grid_edge * 1.5f),
                                new Vector3(cellPos.X + grid_edge * 0.5f, cellPos.Y + grid_edge * 0.5f, cellPos.Z - grid_edge * 0.5f),
                                pos, (float)radius))
            {
                cells.Add(new Parallelepiped(Vector3.Add(cellPos, new Vector3(0f, 0f, -grid_edge)), -grid_edge, -1));
                count++;
            }

            //bottom_left
            if (cellPos.X > (-10 + grid_edge) && cellPos.Y > (-10 + grid_edge) && checkForSphereCubeIntersection(
                                new Vector3(cellPos.X - grid_edge * 1.5f, cellPos.Y - grid_edge * 1.5f, cellPos.Z - grid_edge * 0.5f),
                                new Vector3(cellPos.X - grid_edge * 0.5f, cellPos.Y - grid_edge * 0.5f, cellPos.Z + grid_edge * 0.5f),
                                pos, (float)radius))
            {
                cells.Add(new Parallelepiped(Vector3.Add(cellPos, new Vector3(-grid_edge, -grid_edge, 0f)), grid_edge, -1));
                count++;
            }
            //bottom_left_near
            if (cellPos.X > (-10 + grid_edge) && cellPos.Y > (-10 + grid_edge) && cellPos.Z < (10 - grid_edge) && checkForSphereCubeIntersection(
                                new Vector3(cellPos.X - grid_edge * 1.5f, cellPos.Y - grid_edge * 1.5f, cellPos.Z + grid_edge * 0.5f),
                                new Vector3(cellPos.X - grid_edge * 0.5f, cellPos.Y - grid_edge * 0.5f, cellPos.Z + grid_edge * 1.5f),
                                pos, (float)radius))
            {
                cells.Add(new Parallelepiped(Vector3.Add(cellPos, new Vector3(-grid_edge, -grid_edge, grid_edge)), grid_edge, -1));
                count++;
            }

            //bottom_left_far
            if (cellPos.X > (-10 + grid_edge) && cellPos.Y > (-10 + grid_edge) && cellPos.Z > (-10 + grid_edge) && checkForSphereCubeIntersection(
                                new Vector3(cellPos.X - grid_edge * 1.5f, cellPos.Y - grid_edge * 1.5f, cellPos.Z - grid_edge * 1.5f),
                                new Vector3(cellPos.X - grid_edge * 0.5f, cellPos.Y - grid_edge * 0.5f, cellPos.Z - grid_edge * 0.5f),
                                pos, (float)radius))
            {
                cells.Add(new Parallelepiped(Vector3.Add(cellPos, new Vector3(-grid_edge, -grid_edge, -grid_edge)), grid_edge, -1));
                count++;
            }

            //bottom_right
            if (cellPos.X > (-10 + grid_edge) && cellPos.Y > (-10 + grid_edge) && checkForSphereCubeIntersection(
                                new Vector3(cellPos.X + grid_edge * 0.5f, cellPos.Y - grid_edge * 1.5f, cellPos.Z - grid_edge * 0.5f),
                                new Vector3(cellPos.X + grid_edge * 1.5f, cellPos.Y - grid_edge * 0.5f, cellPos.Z + grid_edge * 0.5f),
                                pos, (float)radius))
            {
                cells.Add(new Parallelepiped(Vector3.Add(cellPos, new Vector3(grid_edge, -grid_edge, 0f)), grid_edge, -1));
                count++;
            }

            //bottom_right_near
            if (cellPos.X > (-10 + grid_edge) && cellPos.Y > (-10 + grid_edge) && cellPos.Z < (10 - grid_edge) && checkForSphereCubeIntersection(
                                new Vector3(cellPos.X + grid_edge * 0.5f, cellPos.Y - grid_edge * 1.5f, cellPos.Z + grid_edge * 0.5f),
                                new Vector3(cellPos.X + grid_edge * 1.5f, cellPos.Y - grid_edge * 0.5f, cellPos.Z + grid_edge * 1.5f),
                                pos, (float)radius))
                cells.Add(new Parallelepiped(Vector3.Add(cellPos, new Vector3(grid_edge, -grid_edge, grid_edge)), grid_edge, -1));

            //bottom_right_far
            if (cellPos.X < (10 - grid_edge) && cellPos.Y > (-10 + grid_edge) && cellPos.Z > (-10 + grid_edge) && checkForSphereCubeIntersection(
                                new Vector3(cellPos.X + grid_edge * 0.5f, cellPos.Y - grid_edge * 1.5f, cellPos.Z - grid_edge * 1.5f),
                                new Vector3(cellPos.X + grid_edge * 1.5f, cellPos.Y - grid_edge * 0.5f, cellPos.Z - grid_edge * 0.5f),
                                pos, (float)radius))
            {
                cells.Add(new Parallelepiped(Vector3.Add(cellPos, new Vector3(grid_edge, -grid_edge, -grid_edge)), grid_edge, -1));
                count++;
            }

            //top_left
            if (cellPos.X > (-10 + grid_edge) && cellPos.Y < (10 - grid_edge) && checkForSphereCubeIntersection(
                                new Vector3(cellPos.X - grid_edge * 1.5f, cellPos.Y + grid_edge * 0.5f, cellPos.Z - grid_edge * 0.5f),
                                new Vector3(cellPos.X - grid_edge * 0.5f, cellPos.Y + grid_edge * 1.5f, cellPos.Z + grid_edge * 0.5f),
                                pos, (float)radius))
            {
                cells.Add(new Parallelepiped(Vector3.Add(cellPos, new Vector3(-grid_edge, grid_edge, 0f)), grid_edge, -1));
                count++;
            }

            //top_left_near
            if (cellPos.X > (-10 + grid_edge) && cellPos.Y > (-10 + grid_edge) && cellPos.Z < (10 - grid_edge) && checkForSphereCubeIntersection(
                                new Vector3(cellPos.X - grid_edge * 1.5f, cellPos.Y + grid_edge * 0.5f, cellPos.Z + grid_edge * 0.5f),
                                new Vector3(cellPos.X - grid_edge * 0.5f, cellPos.Y + grid_edge * 1.5f, cellPos.Z + grid_edge * 1.5f),
                                pos, (float)radius))
            {
                cells.Add(new Parallelepiped(Vector3.Add(cellPos, new Vector3(-grid_edge, grid_edge, grid_edge)), grid_edge, -1));
                count++;
            }

            //top_left_far
            if (cellPos.X > (-10 + grid_edge) && cellPos.Y < (10 - grid_edge) && cellPos.Z > (-10 + grid_edge) && checkForSphereCubeIntersection(
                                new Vector3(cellPos.X - grid_edge * 1.5f, cellPos.Y + grid_edge * 0.5f, cellPos.Z - grid_edge * 1.5f),
                                new Vector3(cellPos.X - grid_edge * 0.5f, cellPos.Y + grid_edge * 1.5f, cellPos.Z - grid_edge * 0.5f),
                                pos, (float)radius))
            {
                cells.Add(new Parallelepiped(Vector3.Add(cellPos, new Vector3(-grid_edge, grid_edge, -grid_edge)), grid_edge, -1));
                count++;
            }

            //top_right
            if (cellPos.X < (10 - grid_edge) && cellPos.Y < (10 - grid_edge) && checkForSphereCubeIntersection(
                                new Vector3(cellPos.X + grid_edge * 0.5f, cellPos.Y + grid_edge * 0.5f, cellPos.Z - grid_edge * 0.5f),
                                new Vector3(cellPos.X + grid_edge * 1.5f, cellPos.Y + grid_edge * 1.5f, cellPos.Z + grid_edge * 0.5f),
                                pos, (float)radius))
            {
                cells.Add(new Parallelepiped(Vector3.Add(cellPos, new Vector3(grid_edge, grid_edge, 0f)), grid_edge, -1));
                count++;
            }

            //top_right_near
            if (cellPos.X < (10 - grid_edge) && cellPos.Y < (10 - grid_edge) && cellPos.Z < (10 - grid_edge) && checkForSphereCubeIntersection(
                                new Vector3(cellPos.X + grid_edge * 0.5f, cellPos.Y + grid_edge * 0.5f, cellPos.Z + grid_edge * 0.5f),
                                new Vector3(cellPos.X + grid_edge * 1.5f, cellPos.Y + grid_edge * 1.5f, cellPos.Z + grid_edge * 1.5f),
                                pos, (float)radius))
            {
                cells.Add(new Parallelepiped(Vector3.Add(cellPos, new Vector3(grid_edge, grid_edge, grid_edge)), grid_edge, -1));
                count++;
            }

            //top_right_far
            if (cellPos.X < (10 - grid_edge) && cellPos.Y < (10 - grid_edge) && cellPos.Z > (-10 + grid_edge) && checkForSphereCubeIntersection(
                                new Vector3(cellPos.X + grid_edge * 0.5f, cellPos.Y + grid_edge * 0.5f, cellPos.Z - grid_edge * 1.5f),
                                new Vector3(cellPos.X + grid_edge * 1.5f, cellPos.Y + grid_edge * 1.5f, cellPos.Z - grid_edge * 0.5f),
                                pos, (float)radius))
            {
                cells.Add(new Parallelepiped(Vector3.Add(cellPos, new Vector3(grid_edge, grid_edge, -grid_edge)), grid_edge, -1));
                count++;
            }

            //top_near
            if (cellPos.Y < (10 - grid_edge) && cellPos.Z < (10 - grid_edge) && checkForSphereCubeIntersection(
                                new Vector3(cellPos.X - grid_edge * 0.5f, cellPos.Y + grid_edge * 0.5f, cellPos.Z + grid_edge * 0.5f),
                                new Vector3(cellPos.X + grid_edge * 0.5f, cellPos.Y + grid_edge * 1.5f, cellPos.Z + grid_edge * 1.5f),
                                pos, (float)radius))
            {
                cells.Add(new Parallelepiped(Vector3.Add(cellPos, new Vector3(0f, grid_edge, grid_edge)), grid_edge, -1));
                count++;
                if (count >= 7)
                    return;
            }

            //bottom_near
            if (cellPos.Y > (-10 + grid_edge) && cellPos.Z < (10 - grid_edge) && checkForSphereCubeIntersection(
                                new Vector3(cellPos.X - grid_edge * 0.5f, cellPos.Y - grid_edge * 1.5f, cellPos.Z + grid_edge * 0.5f),
                                new Vector3(cellPos.X + grid_edge * 0.5f, cellPos.Y - grid_edge * 0.5f, cellPos.Z + grid_edge * 1.5f),
                                pos, (float)radius))
            {
                cells.Add(new Parallelepiped(Vector3.Add(cellPos, new Vector3(0f, -grid_edge, grid_edge)), grid_edge, -1));
                count++;
                if (count >= 7)
                    return;
            }

            //top_far
            if (cellPos.Y < (10 - grid_edge) && cellPos.Z > (-10 + grid_edge) && checkForSphereCubeIntersection(
                                new Vector3(cellPos.X - grid_edge * 0.5f, cellPos.Y + grid_edge * 0.5f, cellPos.Z - grid_edge * 1.5f),
                                new Vector3(cellPos.X + grid_edge * 0.5f, cellPos.Y + grid_edge * 1.5f, cellPos.Z - grid_edge * 0.5f),
                                pos, (float)radius))
            {
                cells.Add(new Parallelepiped(Vector3.Add(cellPos, new Vector3(0f, grid_edge, -grid_edge)), grid_edge, -1));
                count++;
                if (count >= 7)
                    return;
            }

            //bottom_far
            if (cellPos.Y > (-10 + grid_edge) && cellPos.Z > (-10 + grid_edge) && checkForSphereCubeIntersection(
                                new Vector3(cellPos.X - grid_edge * 0.5f, cellPos.Y - grid_edge * 1.5f, cellPos.Z - grid_edge * 1.5f),
                                new Vector3(cellPos.X + grid_edge * 0.5f, cellPos.Y - grid_edge * 0.5f, cellPos.Z - grid_edge * 0.5f),
                                pos, (float)radius))
            {
                cells.Add(new Parallelepiped(Vector3.Add(cellPos, new Vector3(0f, -grid_edge, -grid_edge)), grid_edge, -1));
                count++;
                if (count >= 7)
                    return;
            }

            //left_far
            if (cellPos.X > (-10 + grid_edge) && cellPos.Z > (-10 + grid_edge) && checkForSphereCubeIntersection(
                                new Vector3(cellPos.X - grid_edge * 1.5f, cellPos.Y - grid_edge * 0.5f, cellPos.Z - grid_edge * 1.5f),
                                new Vector3(cellPos.X - grid_edge * 0.5f, cellPos.Y + grid_edge * 0.5f, cellPos.Z - grid_edge * 0.5f),
                                pos, (float)radius))
            {
                cells.Add(new Parallelepiped(Vector3.Add(cellPos, new Vector3(-grid_edge, 0f, -grid_edge)), grid_edge, -1));
                count++;
                if (count >= 7)
                    return;
            }

            //right_far
            if (cellPos.X < (10 - grid_edge) && cellPos.Z > (-10 + grid_edge) && checkForSphereCubeIntersection(
                                new Vector3(cellPos.X + grid_edge * 0.5f, cellPos.Y - grid_edge * 0.5f, cellPos.Z - grid_edge * 1.5f),
                                new Vector3(cellPos.X + grid_edge * 1.5f, cellPos.Y + grid_edge * 0.5f, cellPos.Z - grid_edge * 0.5f),
                                pos, (float)radius))
            {
                cells.Add(new Parallelepiped(Vector3.Add(cellPos, new Vector3(grid_edge, 0f, -grid_edge)), grid_edge, -1));
                count++;
                if (count >= 7)
                    return;
            }

            //left_near
            if (cellPos.X > (-10 + grid_edge) && cellPos.Z < (10 - grid_edge) && checkForSphereCubeIntersection(
                                new Vector3(cellPos.X - grid_edge * 1.5f, cellPos.Y - grid_edge * 0.5f, cellPos.Z + grid_edge * 0.5f),
                                new Vector3(cellPos.X - grid_edge * 0.5f, cellPos.Y + grid_edge * 0.5f, cellPos.Z + grid_edge * 1.5f),
                                pos, (float)radius))
            {
                cells.Add(new Parallelepiped(Vector3.Add(cellPos, new Vector3(-grid_edge, 0f, grid_edge)), grid_edge, -1));
                count++;
                if (count >= 7)
                    return;
            }

            //right_near
            if (cellPos.X < (10 - grid_edge) && cellPos.Z < (10 - grid_edge) && checkForSphereCubeIntersection(
                                new Vector3(cellPos.X + grid_edge * 0.5f, cellPos.Y - grid_edge * 0.5f, cellPos.Z + grid_edge * 0.5f),
                                new Vector3(cellPos.X + grid_edge * 1.5f, cellPos.Y + grid_edge * 0.5f, cellPos.Z + grid_edge * 1.5f),
                                pos, (float)radius))
            {
                cells.Add(new Parallelepiped(Vector3.Add(cellPos, new Vector3(grid_edge, 0f, grid_edge)), grid_edge, -1));
                count++;
                if (count >= 7)
                    return;
            }
#endregion

            checkCellType();
        }

        /*public void hashHCell()
        {
            hCell = ((uint)(pos.X / Window.grid_edge) << XSHIFT) |
                    ((uint)(pos.Y / Window.grid_edge) << YSHIFT) |
                    ((uint)(pos.Z / Window.grid_edge) << ZSHIFT);
        }*/

        bool checkForSphereCubeIntersection(Vector3 c1,  Vector3 c2, Vector3 sPos, float radius)
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

        public void checkCellType()
        {
            cellsIntersected = new bool[8];
            double half_fov = Window.fov * 0.5;
            foreach (Body cell in cells)
            {
                Vector3 pos = cell.getPos();
                double half_fov_ = Window.fov * 0.5;

                if ((int)(-(pos.Z - half_fov_) / Window.grid_edge) % 2 == 0)
                {
                    if ((int)(-(pos.X - half_fov_) / Window.grid_edge) % 2 == 0)
                    {
                        if ((int)(-(pos.Y - half_fov_) / Window.grid_edge) % 2 == 0)
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
                        if ((int)(-(pos.Y - half_fov_) / Window.grid_edge) % 2 == 0)
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
                    if ((int)(-(pos.X - half_fov_) / Window.grid_edge) % 2 == 0)
                    {
                        if ((int)(-(pos.Y - half_fov_) / Window.grid_edge) % 2 == 0)
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
                        if ((int)(-(pos.Y - half_fov_) / Window.grid_edge) % 2 == 0)
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
        }
    }
}
