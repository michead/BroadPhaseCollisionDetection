using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cloo;
using Cloo.Bindings;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using OpenTK.Math;
using OpenCLTemplate;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Clpp.Core;

namespace ParallelComputedCollisionDetection
{
    [StructLayout(LayoutKind.Sequential)]
    public struct BodyData
    {
        public uint ID;
        public uint ctrl_bits;
        public float radius;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public float[] pos;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public uint[] cellIDs;
    }

    public static class CollisionDetection
    {
        static ComputePlatform platform = ComputePlatform.Platforms[0];
        static ComputeContextPropertyList properties = new ComputeContextPropertyList(platform);
        static ComputeContext context;
        static IList<ComputeDevice> devices;
        public static BodyData[] array;
        static ComputeProgram program;
        static string Arvo;
        public static string log;
        public static string deviceInfo;
        public const int WORK_GROUP_SIZE = 192;
        public const int R = 16;
        public const uint bits_to_sort = 4294967295;
        public static uint[] rs_array;

        public static void deviceSetUp(){
            devices = new List<ComputeDevice>();
            foreach(ComputeDevice device in platform.Devices)
                devices.Add(device);
            context = new ComputeContext(devices, properties, null, IntPtr.Zero);
            deviceInfo = "[HOST]\n\t" + Environment.OSVersion + "\n"
                        + "[OPENCL Platform]\n"
                        + "\tName: " + platform.Name + "\n"
                        + "\tVendor: " + platform.Vendor + "\n";
                        /*+ "\tProfile: " + platform.Profile + "\n"
                        + "\tExtensions: \n";
            foreach (string extension in platform.Extensions)
                s +="\t\t" + extension + "\n";*/
            deviceInfo += "[DEVICES]\n";
            foreach (ComputeDevice device in context.Devices)
            {
                deviceInfo += "\tName: " + device.Name + "\n"
                    + "\tVendor: " + device.Vendor + "\n"
                    + "\tDriver Version: " + device.DriverVersion + "\n"
                    + "\tOpenCL version: " + device.OpenCLCVersion + "\n"
                    + "\tMax Work Group size: " + device.MaxWorkGroupSize + "\n"
                    + "\tNativeVectorWidthFloat: " + device.NativeVectorWidthFloat + "\n"
                    + "\tNativeVectorWidthDouble: " + device.NativeVectorWidthDouble + "\n"
                    + "\tNativeVectorWidthInt: " + device.NativeVectorWidthInt + "\n"
                    + "\tMax Work Item dimensions: " + device.MaxWorkItemDimensions + "\n"
                    + "\tMax Work Item sizes: " + device.MaxWorkItemSizes.Count + "\n"
                    + "\tCompute Units: " + device.MaxComputeUnits + "\n"
                    + "\tGlobal Memory : " + device.GlobalMemorySize + "bytes\n"
                    + "\tShared Memory: " + device.LocalMemorySize + "bytes";
                    /*+ "\tImage Support: " + device.ImageSupport + "\n"
                    + "\tExtensions: ";
                foreach (string extension in device.Extensions)
                    s += "\n\t + " + extension;*/
            }
        }

        public unsafe static void CollisionDetectionSetUp()
        {
            int nob = Program.window.number_of_bodies;
            array = new BodyData[nob];
            for (int i = 0; i < nob; i++)
            {
                Body body = Program.window.bodies.ElementAt(i);
                array[i].pos = new float[3];
                array[i].cellIDs = new uint[8];
                array[i].pos[0] = body.getPos().X;
                array[i].pos[1] = body.getPos().Y;
                array[i].pos[2] = body.getPos().Z;
                array[i].ID = (uint)i;
                array[i].radius = body.getBSphere().radius;
            }
            System.IO.StreamReader kernelStream = new System.IO.StreamReader("Arvo.cl");
            Arvo = kernelStream.ReadToEnd();
            kernelStream.Close();
            int* addrNumOfBodies = &nob;
            IntPtr anob = (IntPtr)addrNumOfBodies;
            float ge = Program.window.grid_edge;
            float* addr_grid_edge = &ge;
            IntPtr age = (IntPtr)addr_grid_edge;

            //setup
            int structSize = Marshal.SizeOf(array[0]);
            IntPtr ptr = Marshal.AllocHGlobal(structSize*nob);
            for(int i=0; i < nob; i++)
                Marshal.StructureToPtr(array[i], ptr + i*structSize, false);
            byte[] input = new byte[structSize*nob];
            Marshal.Copy(ptr, input, 0, structSize * nob);

#region USELESS DEBUG
            /*Console.WriteLine("FLOAT: " + sizeof(float));
            Console.WriteLine("DOUBLE: " + sizeof(double));
            Console.WriteLine("UINT: " + sizeof(uint));
            Console.WriteLine("INT: " + sizeof(int));
            IntPtr ptr2 = Marshal.AllocHGlobal(structSize * nob);
            byte[] prova = new byte[structSize * nob];
            BodyData[] prova2 = new BodyData[nob];

            Marshal.Copy(input, 0, ptr2, structSize * nob);
            for (int i = 0; i < nob; i++)
                prova2[i] = (BodyData)Marshal.PtrToStructure(ptr2 + i * structSize, typeof(BodyData));

            Console.WriteLine("------PROVA!------");
            List<Body> bodies = Program.window.bodies;
            for(int i = 0; i< nob; i++) {
                Console.Write("Index: " + i
                        + "\nOCL vs CPU"
                        + "\nradius: " + prova2[i].radius.ToString() + "  |  " + bodies[i].getBSphere().radius.ToString()
                        + "\nX: " + prova2[i].pos[0].ToString() + "  |  " + bodies[i].getPos().X.ToString()
                        + "\nY: " + prova2[i].pos[1].ToString() + "  |  " + bodies[i].getPos().Y.ToString()
                        + "\nZ: " + prova2[i].pos[2].ToString() + "  |  " + bodies[i].getPos().Z.ToString() + "\n");
            }*/
#endregion

            ComputeBuffer<byte> objArray = new ComputeBuffer<byte>
                (context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.CopyHostPointer, input);
            ComputeBuffer<uint> cellArray = new ComputeBuffer<uint>
                (context, ComputeMemoryFlags.ReadWrite, nob * 8);
            /*ComputeBuffer<uint> cellArray2 = new ComputeBuffer<uint>
                (context, ComputeMemoryFlags.ReadWrite, nob * 8);
            ComputeBuffer<uint> support = new ComputeBuffer<uint>
                (context, ComputeMemoryFlags.ReadWrite, 256 * 24);*/
            Marshal.FreeHGlobal(ptr);
            ComputeBuffer<int> numOfBodies = new ComputeBuffer<int>
                (context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.CopyHostPointer, sizeof(int), anob);
            ComputeBuffer<float> gridEdge = new ComputeBuffer<float>
                (context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.CopyHostPointer, sizeof(float), age);
            program = new ComputeProgram(context, Arvo);
            program.Build(devices, "-g", null, IntPtr.Zero);
            ComputeKernel kernelArvo = program.CreateKernel("Arvo");
            kernelArvo.SetMemoryArgument(0, objArray);
            kernelArvo.SetMemoryArgument(1, numOfBodies);
            kernelArvo.SetMemoryArgument(2, gridEdge);
            kernelArvo.SetMemoryArgument(3, cellArray);
            /*kernelArvo.SetMemoryArgument(4, cellArray2);
            kernelArvo.SetMemoryArgument(5, support);*/

            //execution
            ComputeCommandQueue queue = 
                new ComputeCommandQueue(context, ComputePlatform.Platforms[0].Devices[0], ComputeCommandQueueFlags.Profiling);
            queue.Execute(kernelArvo, null, new long[] { nob }, null, null);

            //RADIX SORT!
            /*rs_array = new uint[nob * 8];
            IntPtr rs = Marshal.AllocHGlobal(nob * 8);
            ClppContext clppContext = new ClppContext(ComputeDeviceTypes.Gpu);
            clppContext.PrintInformation();
            Clpp.Core.Sort.ClppSortRadixSortGPU sort = new Clpp.Core.Sort.ClppSortRadixSortGPU(clppContext, nob * 8, (long)bits_to_sort, true);
            queue.ReadFromBuffer<uint>(cellArray, ref rs_array, false, null);
            uint[] copy = new uint[nob * 8];
            for (int h = 0; h < nob * 8; h++)
                copy[h] = rs_array[h];
            Array.Sort(copy);
            GCHandle ah = GCHandle.Alloc(rs_array, GCHandleType.Pinned);
            IntPtr a_ah = ah.AddrOfPinnedObject();

            //TODO: modify source to use old compute buffer!  
            sort.PushDatas(a_ah, nob * 8);
            sort.Sort();
            sort.PopDatas();
            ah.Free();

            for (int h = 0; h < nob * 8; h++)
            {
                Console.WriteLine("INDEX: " + h);
                Console.WriteLine(copy[h]);
                Console.WriteLine(rs_array[h]);
            }*/
            
            //read from buffer
            array = new BodyData[nob];
            byte[] result = new byte[structSize * nob];
            IntPtr intPtr = Marshal.AllocHGlobal(structSize*nob);
            queue.ReadFromBuffer<byte>(objArray, ref result, false, null);
            queue.Finish();
            Marshal.Copy(result, 0, intPtr, structSize * nob); 
            for(int i=0; i < nob; i++)
                array[i] = (BodyData)Marshal.PtrToStructure(intPtr + i*structSize, typeof(BodyData));
            checkCorrectness();
            Marshal.FreeHGlobal(intPtr);

            //disposal
            cellArray.Dispose();
            objArray.Dispose();
            numOfBodies.Dispose();
            gridEdge.Dispose();
            kernelArvo.Dispose();
            queue.Dispose();
            program.Dispose();
            //context.Dispose();
        }

        static void checkCorrectness()
        {
            log = "";
            List<Body> bodies = Program.window.bodies;
            int nob = Program.window.number_of_bodies;
            //float ge = Program.window.grid_edge;
            for (int i = 0; i < nob; i++)
            {
                if (array[i].ID != bodies[i].getBSphere().bodyIndex
                    //|| array[i].ctrl_bits != bodies[i].getBSphere().ctrl_bits
                    || array[i].radius != bodies[i].getBSphere().radius
                    || array[i].pos[0] != bodies[i].getPos().X
                    || array[i].pos[1] != bodies[i].getPos().Y
                    || array[i].pos[2] != bodies[i].getPos().Z)
                {
                    log += "Copy error at index: " + i
                        + "\nID: " + array[i].ID.ToString() + "  |  " + bodies[i].getBSphere().bodyIndex.ToString()
                        + "\nctrl_bits: " + array[i].ctrl_bits.ToString() + "  |  " + bodies[i].getBSphere().ctrl_bits.ToString()
                        + "\nradius: " + array[i].radius.ToString() + "  |  " + bodies[i].getBSphere().radius.ToString()
                        + "\nX: " + array[i].pos[0].ToString() + "  |  " + bodies[i].getPos().X.ToString()
                        + "\nY: " + array[i].pos[1].ToString() + "  |  " + bodies[i].getPos().Y.ToString()
                        + "\nZ: " + array[i].pos[2].ToString() + "  |  " + bodies[i].getPos().Z.ToString()
                        /*+ "\ncID[0]" + array[i].cellIDs[0].ToString() + "  |  " + bodies[i].getBSphere().cellArray[0].ToString()
                        + "\ncID[1]" + array[i].cellIDs[1].ToString() + "  |  " + bodies[i].getBSphere().cellArray[1].ToString()
                        + "\ncID[2]" + array[i].cellIDs[2].ToString() + "  |  " + bodies[i].getBSphere().cellArray[2].ToString()
                        + "\ncID[3]" + array[i].cellIDs[3].ToString() + "  |  " + bodies[i].getBSphere().cellArray[3].ToString()
                        + "\ncID[4]" + array[i].cellIDs[4].ToString() + "  |  " + bodies[i].getBSphere().cellArray[4].ToString()
                        + "\ncID[5]" + array[i].cellIDs[5].ToString() + "  |  " + bodies[i].getBSphere().cellArray[5].ToString()
                        + "\ncID[6]" + array[i].cellIDs[6].ToString() + "  |  " + bodies[i].getBSphere().cellArray[6].ToString()
                        + "\ncID[7]" + array[i].cellIDs[7].ToString() + "  |  " + bodies[i].getBSphere().cellArray[7].ToString()*/
                        /*+ "\nradius: \n\t" + Convert.ToString(BitConverter.DoubleToInt64Bits(array[i].radius), 2)
                        + "\n\t" + Convert.ToString(BitConverter.DoubleToInt64Bits(bodies[i].getBSphere().radius), 2)
                        + "\nX: \n\t" + Convert.ToString(BitConverter.DoubleToInt64Bits(array[i].pos[0]), 2)
                        + "\n\t" + Convert.ToString(BitConverter.DoubleToInt64Bits(bodies[i].getPos().X), 2)
                        + "\nY: \n\t" + Convert.ToString(BitConverter.DoubleToInt64Bits(array[i].pos[1]), 2)
                        + "\n\t" + Convert.ToString(BitConverter.DoubleToInt64Bits(bodies[i].getPos().Y), 2)
                        + "\nZ: \n\t" + Convert.ToString(BitConverter.DoubleToInt64Bits(array[i].pos[2]), 2)
                        + "\n\t" + Convert.ToString(BitConverter.DoubleToInt64Bits(bodies[i].getPos().Z), 2)*/ + "\n\n";
                }
                /*else Console.Write("Copy correctly executed at index: " + i
                        + "\nOCL vs CPU"
                        + "\nradius: " + array[i].radius.ToString() + "  |  " + bodies[i].getBSphere().radius.ToString()
                        + "\nX: " + array[i].pos[0].ToString() + "  |  " + bodies[i].getPos().X.ToString()
                        + "\nY: " + array[i].pos[1].ToString() + "  |  " + bodies[i].getPos().Y.ToString()
                        + "\nZ: " + array[i].pos[2].ToString() + "  |  " + bodies[i].getPos().Z.ToString() + "\n");*/
            }
        }
    }
}
