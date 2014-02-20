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

namespace ParallelComputedCollisionDetection
{
    struct CellArrayElem
    {
        int[] cellIDs;
    }

    struct ObjectArrayElem
    {
        int objectID;
        int controlBits;
    }

    struct ObjectProperties
    {
        public int ID;
        public int control_bits;
        //CHECK THIS
        public int[] cellIDs;
        public double radius;
        public float[] pos;
    }

    static class CollisionDetection
    {
        static ComputePlatform platform = ComputePlatform.Platforms[0];
        static ComputeContextPropertyList properties = new ComputeContextPropertyList(platform);
        static ComputeContext context;
        static IList<ComputeDevice> devices;
        static ObjectProperties[] array = new ObjectProperties[Program.window.number_of_bodies];
        static ComputeProgram program;
        static string Arvo;

        public static string deviceInfo(){
            devices = new List<ComputeDevice>();
            foreach(ComputeDevice device in platform.Devices)
                devices.Add(device);
            context = new ComputeContext(devices, properties, null, IntPtr.Zero);
            string s = "[HOST]\n\t" + Environment.OSVersion + "\n"
                        + "[OPENCL Platform]\n"
                        + "\tName: " + platform.Name + "\n"
                        + "\tVendor: " + platform.Vendor + "\n";
                        /*+ "\tProfile: " + platform.Profile + "\n"
                        + "\tExtensions: \n";
            foreach (string extension in platform.Extensions)
                s +="\t\t" + extension + "\n";*/
            s += "[DEVICES]\n";
            foreach (ComputeDevice device in context.Devices)
            {
                s += "\tName: " + device.Name + "\n"
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
            return s;
        }

        public unsafe static void CollisionDetectionSetUp()
        {
            /*ComputeBuffer<CellArrayElem> cellArray = new ComputeBuffer<CellArrayElem>
                (context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.UseHostPointer, null);
            ComputeBuffer<ObjectArrayElem> objectArray = new ComputeBuffer<ObjectArrayElem>
                (context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.UseHostPointer, null);*/
            for (int i = 0; i < Program.window.number_of_bodies; i++)
            {
                Body body = Program.window.bodies.ElementAt(i);
                array[i].pos[0] = body.getPos().X;
                array[i].pos[1] = body.getPos().Y;
                array[i].pos[2] = body.getPos().Z;
                array[i].ID = i;
                array[i].radius = body.getBSphere().radius;
                array[i].pos = new float[3];
                array[i].cellIDs = new int[8];
            }
            System.IO.StreamReader kernelStream = new System.IO.StreamReader
                (@"C:\Users\simone\Documents\Visual Studio 2013\Projects\ParallelComputedCollisionDetection\ParallelComputedCollisionDetection");
            Arvo = kernelStream.ReadToEnd();
            kernelStream.Close();
            int nob = Program.window.number_of_bodies;
            int* addrNumOfBodies = &nob;
            IntPtr anob = (IntPtr)addrNumOfBodies;
            double ge = Program.window.grid_edge;
            double* addr_grid_edge = &ge;
            IntPtr age = (IntPtr)addr_grid_edge;
            ComputeBuffer<ObjectProperties> objArray = new ComputeBuffer<ObjectProperties>
                (context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.UseHostPointer, array);
            ComputeBuffer<int> numOfBodies = new ComputeBuffer<int>
                (context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.UseHostPointer, sizeof(int), anob);
            ComputeBuffer<double> gridEdge = new ComputeBuffer<double>
                (context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.UseHostPointer, sizeof(double), age);
            program = new ComputeProgram(context, Arvo);
            program.Build(devices, "", null, IntPtr.Zero);
            ComputeKernel kernelArvo = program.CreateKernel("Arvo");
            kernelArvo.SetMemoryArgument(0, objArray);
            kernelArvo.SetMemoryArgument(1, numOfBodies);
        }

    }
}
