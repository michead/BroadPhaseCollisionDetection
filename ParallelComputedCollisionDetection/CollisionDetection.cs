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

namespace ParallelComputedCollisionDetection
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ObjectProperties
    {
        public uint ID;
        public uint control_bits;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public uint[] cellIDs;
        public double radius;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public float[] pos;
    }

    public static class CollisionDetection
    {
        static ComputePlatform platform = ComputePlatform.Platforms[0];
        static ComputeContextPropertyList properties = new ComputeContextPropertyList(platform);
        static ComputeContext context;
        static IList<ComputeDevice> devices;
        public static ObjectProperties[] array = new ObjectProperties[Program.window.number_of_bodies];
        static ComputeProgram program;
        static string Arvo;
        public static string deviceInfo = "";

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
                    + "\tMax Work Item dimensions: " + device.MaxWorkItemDimensions + "\n"
                    + "\tMax Work Item sizes: " + device.MaxWorkItemSizes.Count + "\n"
                    + "\tCompute Units: " + device.MaxComputeUnits + "\n"
                    + "\tGlobal Memory : " + device.GlobalMemorySize + "bytes\n";
                    /*+ "\tImage Support: " + device.ImageSupport + "\n"
                    + "\tExtensions: ";
                foreach (string extension in device.Extensions)
                    s += "\n\t + " + extension;*/
            }
        }

        public unsafe static void CollisionDetectionSetUp()
        {
            int nob = Program.window.number_of_bodies;
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
            //Console.Write(Arvo);
            int* addrNumOfBodies = &nob;
            IntPtr anob = (IntPtr)addrNumOfBodies;
            double ge = Program.window.grid_edge;
            double* addr_grid_edge = &ge;
            IntPtr age = (IntPtr)addr_grid_edge;

            //setup
            int structSize = Marshal.SizeOf(array[0]);
            IntPtr ptr = Marshal.AllocHGlobal(structSize*nob);
            for(int i=0; i < nob; i++)
                Marshal.StructureToPtr(array[i], ptr + i*structSize, false);
            byte[] dataOut = new byte[structSize*nob];
            Marshal.Copy(ptr, dataOut, 0, structSize*nob);
            Marshal.FreeHGlobal(ptr);
            ComputeBuffer<byte> objArray = new ComputeBuffer<byte>
                (context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.UseHostPointer, dataOut);
            ComputeBuffer<int> numOfBodies = new ComputeBuffer<int>
                (context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.UseHostPointer, sizeof(int), anob);
            ComputeBuffer<double> gridEdge = new ComputeBuffer<double>
                (context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.UseHostPointer, sizeof(double), age);
            program = new ComputeProgram(context, Arvo);
            program.Build(devices, "", null, IntPtr.Zero);
            ComputeKernel kernelArvo = program.CreateKernel("Arvo");
            kernelArvo.SetMemoryArgument(0, objArray);
            kernelArvo.SetMemoryArgument(1, numOfBodies);
            kernelArvo.SetMemoryArgument(2, gridEdge);

            //execution
            ComputeCommandQueue queue = 
                new ComputeCommandQueue(context, ComputePlatform.Platforms[0].Devices[0], ComputeCommandQueueFlags.None);
            queue.Execute(kernelArvo, null, new long[] { nob }, null, null);

            //read from buffer
            byte[] result = new byte[structSize * nob];
            queue.ReadFromBuffer<byte>(objArray, ref result, false, null);
            IntPtr intPtr = Marshal.AllocHGlobal(structSize*nob);
            Marshal.Copy(result, 0, intPtr, structSize * nob);
            for(int i=0; i < nob; i++)
                array[i] = (ObjectProperties)Marshal.PtrToStructure(intPtr + i*structSize, typeof(ObjectProperties));
            Marshal.FreeHGlobal(intPtr);
        }
    }
}
