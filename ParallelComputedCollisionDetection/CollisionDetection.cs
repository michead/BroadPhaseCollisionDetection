#define ALL

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
using System.IO;
using System.Diagnostics;

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

    public class CollisionDetection
    {
        #region GLOBAL MEMBERS
        //ComputeEventList ceb;
        ComputePlatform platform;
        public ComputeContext context;
        ComputeCommandQueue queue;
        ComputeContextPropertyList properties;
        IList<ComputeDevice> devices;
        int structSize;
        public string deviceInfo;
        public string log;
        public const int DIGITS_PER_ROUND = 4;
        public const int BLOCK_SIZE = 32;
        public const uint bits_to_sort = 4294967295;
        public BodyData[] array;
        System.IO.StreamReader kernelStream;
        Stopwatch time;
        //long exe_time;
        
        #region PROGRAMS
        ComputeProgram p_dataInitialization;
        ComputeProgram p_reorder;
        ComputeProgram p_elementCount;
        ComputeProgram p_prefixSum;
        ComputeProgram p_ccArrayCreation;
        ComputeProgram p_radixSort;
        #endregion
        #region KERNELS
        ComputeKernel k_dataInitialization;
        ComputeKernel k_reorder;
        ComputeKernel k_elementCount;
        ComputeKernel k_prefixSum;
        ComputeKernel k_ccArrayCreation;
        ComputeKernel kernel_block_sort;
        ComputeKernel kernel_block_scan;
        ComputeKernel kernel_block_prefix;
        ComputeKernel kernel_reorder;
        string s_dataInitialization;
        string s_radixSort;
        string s_reorder;
        string s_elementCount;
        string s_prefixSum;
        string s_ccArrayCreation;
        #endregion
        #region BUFFERS
        //ARVO
        ComputeBuffer<byte> b_objectData;
        ComputeBuffer<ulong> b_objectIDArray;
        ComputeBuffer<uint> b_cellIDArray;
        ComputeBuffer<float> b_gridEdge;
        //RADIX SORT
        ComputeBuffer<uint> b_blockScan;
        ComputeBuffer<uint> b_blockOffset;
        ComputeBuffer<uint> b_blockSum;
        ComputeBuffer<uint> b_temp;
        ComputeBuffer<uint> b_iArrayIn;
        ComputeBuffer<uint> b_iArrayOut;
        ComputeBuffer<uint> b_scanCount;
        ComputeBuffer<uint> b_numberOfElems;
        //OBJECT ID ARRAY REORDER
        ComputeBuffer<ulong> b_reorder;
        //COLLISION CELL ARRAY SETUP
        ComputeBuffer<uint> b_occPerRad;
        ComputeBuffer<uint> b_temp2;
        ComputeBuffer<uint> b_flags;
        ComputeBuffer<uint> b_numOfCC;
        //COLLISION CELL ARRAY CREATION
        ComputeBuffer<ulong> b_ccArray;
        ComputeBuffer<ulong> b_temp3;
        ComputeBuffer<uint> b_ccIndexes;
        #endregion
        #endregion

        public void deviceSetUp()
        {
            platform = ComputePlatform.Platforms[0];
            properties = new ComputeContextPropertyList(platform);
            devices = new List<ComputeDevice>();
            foreach (ComputeDevice device in platform.Devices)
                devices.Add(device);
            context = new ComputeContext(devices, properties, null, IntPtr.Zero);
            deviceInfo = "[HOST]\n\t" + Environment.OSVersion + "\n"
                        + "[OPENCL Platform]\n"
                        + "\tName: " + platform.Name + "\n"
                        + "\tVendor: " + platform.Vendor + "\n";
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
                    + "\tGlobal Memory : " + device.GlobalMemorySize + " bytes\n"
                    + "\tShared Memory: " + device.LocalMemorySize + " bytes";
            }
            InitializeComponents();
            time = new Stopwatch();
        }

        public unsafe void CreateCollisionCellArray()
        {
            time.Reset();

            int num_of_bodies = Program.window.number_of_bodies;
            uint num_of_elements = (uint)num_of_bodies * 8;
            float ge = Program.window.grid_edge;

            time.Start();

            uint[] sortedCellIDArray = new uint[num_of_elements];
            uint[] indexArrayIn = new uint[num_of_elements];
            for (int j = 0; j < num_of_elements; j++)
                indexArrayIn[j] = (uint)j;
            uint[] indexArrayOut = new uint[num_of_elements];
            
            GCHandle gch_iai = GCHandle.Alloc(indexArrayIn, GCHandleType.Pinned);
            IntPtr ptr_i = gch_iai.AddrOfPinnedObject();
            GCHandle gch_ge = GCHandle.Alloc(ge, GCHandleType.Pinned);
            IntPtr ptr_ge = gch_ge.AddrOfPinnedObject();

            array = new BodyData[num_of_bodies];
            for (int i = 0; i < num_of_bodies; i++)
            {
                Body body = Program.window.bodies.ElementAt(i);
                array[i].pos = new float[3];
                array[i].cellIDs = new uint[8];
                //array[i].ctrl_bits = 0;
                array[i].pos[0] = body.getPos().X;
                array[i].pos[1] = body.getPos().Y;
                array[i].pos[2] = body.getPos().Z;
                array[i].ID = (uint)i;
                array[i].radius = body.getBSphere().radius;
            }

            structSize = Marshal.SizeOf(array[0]);

            #region INITIALIZATION AND POPULATION OF DEVICE ARRAYS

            IntPtr ptr = Marshal.AllocHGlobal(structSize * num_of_bodies);
            for (int i = 0; i < num_of_bodies; i++)
                Marshal.StructureToPtr(array[i], ptr + i * structSize, false);
            byte[] input = new byte[structSize * num_of_bodies];
            Marshal.Copy(ptr, input, 0, structSize * num_of_bodies);

            b_objectData = new ComputeBuffer<byte>
                (context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.CopyHostPointer, input);
            b_objectIDArray = new ComputeBuffer<ulong>
                (context, ComputeMemoryFlags.ReadWrite, num_of_elements);
            b_cellIDArray = new ComputeBuffer<uint>
                (context, ComputeMemoryFlags.ReadWrite, num_of_elements);
            b_gridEdge = new ComputeBuffer<float>
                (context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.CopyHostPointer, 1, ptr_ge);
            
            k_dataInitialization.SetMemoryArgument(0, b_objectData);
            k_dataInitialization.SetMemoryArgument(1, b_gridEdge);
            k_dataInitialization.SetMemoryArgument(2, b_cellIDArray);
            k_dataInitialization.SetMemoryArgument(3, b_objectIDArray);
            queue.Execute(k_dataInitialization, null, new long[] { num_of_bodies }, null, null);
            queue.Finish();

            #endregion

            uint[] rs_array = new uint[num_of_bodies * 8];
            queue.ReadFromBuffer<uint>(b_cellIDArray, ref rs_array, false, null);
            queue.Finish();

            #region READ DATA INITIALIZED IN DEVICE
            array = new BodyData[num_of_bodies];
            byte[] result = new byte[structSize * num_of_bodies];
            IntPtr intPtr = Marshal.AllocHGlobal(structSize * num_of_bodies);
            queue.ReadFromBuffer<byte>(b_objectData, ref result, false, null);
            queue.Finish();
            #endregion

            #region CHECK CORRECTNESS

            Marshal.Copy(result, 0, intPtr, structSize * num_of_bodies);
            for (int i = 0; i < num_of_bodies; i++)
                array[i] = (BodyData)Marshal.PtrToStructure(intPtr + i * structSize, typeof(BodyData));
            checkCorrectness();
            Marshal.FreeHGlobal(intPtr);
            #endregion

            #region CHECK OBJECT ID AND CELL ID ARRAYS

            uint[] unsortedCellIDArray = new uint[num_of_elements];
            ulong[] unsortedObjectIDArray = new ulong[num_of_elements];

            queue.ReadFromBuffer<uint>(b_cellIDArray, ref unsortedCellIDArray, false, null);
            queue.Finish();
            
            string cellIDArrayLog = "";

            for (int i = 0; i < num_of_elements; i++)
            {
                cellIDArrayLog += "[" + i + "]" + unsortedCellIDArray[i] + "\n";
            }
            File.WriteAllText(Application.StartupPath + @"\unsortedCellIDArrayLog.txt", cellIDArrayLog);

            string objectIDArrayLog = "";

            queue.ReadFromBuffer<ulong>(b_objectIDArray, ref unsortedObjectIDArray, false, null);
            queue.Finish();
            for (int i = 0; i < num_of_elements; i++)
            {
                objectIDArrayLog += "[" + i + "]" + (uint)unsortedObjectIDArray[i] + "\n";
            }
            File.WriteAllText(Application.StartupPath + @"\unsortedObjectIDArrayLog.txt", objectIDArrayLog);

            #endregion
#if ALL

            #region RADIX SORT
            #region INITIALIZING RADIX SORT MEMBERS
            uint block_count = (uint)Math.Ceiling((float)num_of_elements / BLOCK_SIZE);
            uint lScanCount = block_count * (1 << DIGITS_PER_ROUND) / 4;
            uint lScanSize = (uint)Math.Ceiling(((float)lScanCount / BLOCK_SIZE)) * BLOCK_SIZE;
            uint globalSize = block_count * BLOCK_SIZE;
            #endregion

            #region POPULATING RADIX SORT BUFFERS
            b_blockScan = new ComputeBuffer<uint>(context, ComputeMemoryFlags.ReadWrite, block_count * (1 << DIGITS_PER_ROUND));
            b_blockOffset = new ComputeBuffer<uint>(context, ComputeMemoryFlags.ReadWrite, block_count * (1 << DIGITS_PER_ROUND));
            b_blockSum = new ComputeBuffer<uint>(context, ComputeMemoryFlags.ReadWrite, BLOCK_SIZE);
            b_temp = new ComputeBuffer<uint>(context, ComputeMemoryFlags.ReadWrite, num_of_elements);
            b_iArrayIn = new ComputeBuffer<uint>(context, ComputeMemoryFlags.ReadWrite
                | ComputeMemoryFlags.CopyHostPointer, num_of_elements, ptr_i);
            b_iArrayOut = new ComputeBuffer<uint>(context, ComputeMemoryFlags.ReadWrite, num_of_elements);
            #endregion

            GCHandle gch_sc = GCHandle.Alloc(lScanCount, GCHandleType.Pinned);
            IntPtr ptr_sc = gch_sc.AddrOfPinnedObject();
            GCHandle gch_ec = GCHandle.Alloc(num_of_elements, GCHandleType.Pinned);
            IntPtr ptr_ec = gch_ec.AddrOfPinnedObject();

            b_scanCount =
                    new ComputeBuffer<uint>(context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.CopyHostPointer, 1, ptr_sc);
            b_numberOfElems =
                    new ComputeBuffer<uint>(context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.CopyHostPointer, 1, ptr_ec);
            
            #region libCL RADIX SORT ITERATION
            for (uint j = 0; j < 32; j += DIGITS_PER_ROUND)
            {
                GCHandle gch_iter = GCHandle.Alloc(j, GCHandleType.Pinned);
                IntPtr ptr_j = gch_iter.AddrOfPinnedObject();

                ComputeBuffer<uint> b_iteration =
                    new ComputeBuffer<uint>(context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.CopyHostPointer, 1, ptr_j);

            #region BLOCK SORT
                kernel_block_sort.SetMemoryArgument(0, b_cellIDArray);
                kernel_block_sort.SetMemoryArgument(1, b_temp);
                kernel_block_sort.SetMemoryArgument(2, b_iArrayIn);
                kernel_block_sort.SetMemoryArgument(3, b_iArrayOut);
                kernel_block_sort.SetMemoryArgument(4, b_iteration);
                kernel_block_sort.SetMemoryArgument(5, b_blockScan);
                kernel_block_sort.SetMemoryArgument(6, b_blockOffset);
                kernel_block_sort.SetMemoryArgument(7, b_numberOfElems);
                queue.Execute(kernel_block_sort, null, new long[] { globalSize }, new long[] { BLOCK_SIZE }, null);
                queue.Finish();
                #endregion

            #region BLOCK SCAN
                kernel_block_scan.SetMemoryArgument(0, b_blockScan);
                kernel_block_scan.SetMemoryArgument(1, b_blockSum);
                kernel_block_scan.SetMemoryArgument(2, b_scanCount);
                queue.Execute(kernel_block_scan, null, new long[] { lScanSize }, new long[] { BLOCK_SIZE }, null);
                queue.Finish();
                #endregion

            #region BLOCK PREFIX
                kernel_block_prefix.SetMemoryArgument(0, b_blockScan);
                kernel_block_prefix.SetMemoryArgument(1, b_blockSum);
                kernel_block_prefix.SetMemoryArgument(2, b_scanCount);
                queue.Execute(kernel_block_prefix, null, new long[] { lScanSize }, new long[] { BLOCK_SIZE }, null);
                queue.Finish();
                #endregion

            #region REORDER
                kernel_reorder.SetMemoryArgument(0, b_temp);
                kernel_reorder.SetMemoryArgument(1, b_cellIDArray);
                kernel_reorder.SetMemoryArgument(2, b_iArrayOut);
                kernel_reorder.SetMemoryArgument(3, b_iArrayIn);
                kernel_reorder.SetMemoryArgument(4, b_blockScan);
                kernel_reorder.SetMemoryArgument(5, b_blockOffset);
                kernel_reorder.SetMemoryArgument(6, b_iteration);
                kernel_reorder.SetMemoryArgument(7, b_numberOfElems);
                queue.Execute(kernel_reorder, null, new long[] { globalSize }, new long[] { BLOCK_SIZE }, null);
                queue.Finish();
                #endregion

                b_iteration.Dispose();
                gch_iter.Free();
            }
            #endregion
            #endregion

            queue.ReadFromBuffer<uint>(b_cellIDArray, ref sortedCellIDArray, false, null);
            queue.Finish();
            
            #region CHECK CELL ID ARRAY ORDER
            string orderedCellIDArrayLog = "";

            for (int i = 0; i < num_of_elements; i++)
            {
                orderedCellIDArrayLog += "[" + i + "]" + sortedCellIDArray[i] + "\n";
            }
            File.WriteAllText(Application.StartupPath + @"\radixSortLog.txt", orderedCellIDArrayLog);
            #endregion

            #region REORDER OBJECT ID ARRAY
            ulong[] o_array = new ulong[num_of_elements];
            b_reorder = new ComputeBuffer<ulong>(context, ComputeMemoryFlags.ReadWrite, num_of_elements);

            k_reorder.SetMemoryArgument(0, b_iArrayIn);
            k_reorder.SetMemoryArgument(1, b_objectIDArray);
            k_reorder.SetMemoryArgument(2, b_reorder);
            queue.Execute(k_reorder, null, new long[] { num_of_elements }, null, null);
            queue.Finish();

            queue.ReadFromBuffer<ulong>(b_reorder, ref o_array, false, null);
            queue.Finish();

            indexArrayOut = new uint[num_of_elements];
            queue.ReadFromBuffer<uint>(b_iArrayIn, ref indexArrayOut, false, null);
            #endregion
            
            #region CHECK OBJECT ID ORDER
            string orderedObjIDArray = "";
            for (int i = 0; i < num_of_elements; i++)
            {
                orderedObjIDArray += "[" + i + "]" + (uint)o_array[i] + "\n";
            }
            string indexOut = "";
            for (int i = 0; i < num_of_elements; i++)
            {
                indexOut += "[" + i + "]" + indexArrayOut[i] + "\n";
            }
            File.WriteAllText(Application.StartupPath + @"\reorderLog.txt", orderedObjIDArray);
            File.WriteAllText(Application.StartupPath + @"\indexLog.txt", indexOut);
            #endregion

            #region ELEMENT COUNT

            //281 -> max value a cell hash can be
            uint[] occurrences = new uint[282];
            uint[] n_occurrences = new uint[282];
            uint[] temp_array = new uint[num_of_elements];
            uint[] temp_array2 = new uint[num_of_elements];
            ulong[] out_array = new ulong[num_of_elements];
            uint[] nocc = new uint[1];

            b_occPerRad =
                new ComputeBuffer<uint>(context, ComputeMemoryFlags.ReadWrite, 282);
            b_temp2 =
                new ComputeBuffer<uint>(context, ComputeMemoryFlags.ReadWrite, num_of_elements);
            b_flags =
                new ComputeBuffer<uint>(context, ComputeMemoryFlags.ReadWrite, num_of_elements);
            b_numOfCC =
                new ComputeBuffer<uint>(context, ComputeMemoryFlags.ReadWrite, 1);

            k_elementCount.SetMemoryArgument(0, b_reorder);
            k_elementCount.SetMemoryArgument(1, b_temp2);
            k_elementCount.SetMemoryArgument(2, b_numOfCC);
            k_elementCount.SetMemoryArgument(3, b_occPerRad);
            k_elementCount.SetMemoryArgument(4, b_flags);
            
            try{
                queue.Execute(k_elementCount, null, new long[] { num_of_elements }, null, null);
            }
            catch (Exception e)
            {
                File.WriteAllText(Application.StartupPath + @"\exeLog.txt", e.Message);
            }
            queue.Finish();

            queue.ReadFromBuffer<uint>(b_temp2, ref temp_array, false, null);
            queue.ReadFromBuffer<uint>(b_occPerRad, ref n_occurrences, false, null);
            queue.ReadFromBuffer<uint>(b_numOfCC, ref nocc, false, null);
            queue.Finish();

            //Console.WriteLine(sum);
            //Console.WriteLine(nocc[0]);

            string hs = "";
            for (int h = 0; h < 282; h++)
            {
                hs += "[" + h + "] " +
                    //temp_array[h] + "\n\t" + 
                    n_occurrences[h] + "\n";
            }

            File.WriteAllText(Application.StartupPath + @"\elementCountLog.txt", hs);

            Array.Sort<uint>(rs_array); //sort by .NET
            
            string s = "";
            for (int h = 0; h < num_of_elements; h++)
            {
                s += "[" + h + "] " +
                //s += sortedCellIDArray[h] + "\n"; //OCL
                //s += rs_array[h] + "\n"; //CPU
                //s += unchecked((uint)o_array[h]) + "\n"; //OCL reordered array cell ID
                //s += ((o_array[h] & ((ulong)281470681743360)) >> 32) + "\n"; //OCL reordered array obj ID
                //s += (indexArrayOut[h] / 8) + "\n"; // cell index divided by 8 --> should be equal to the line above;
                    temp_array[h] + "\n";
            }
            
            File.WriteAllText(Application.StartupPath + @"\elementCountLog2.txt", s);

            #endregion

            #region PREFIX SUM

            int maxIter = (int)Math.Log(num_of_elements, 2);
            for (uint d = 0; d < maxIter; d++)
            {
                GCHandle gch_iteration = GCHandle.Alloc(d, GCHandleType.Pinned);
                IntPtr ptr_iteration = gch_iteration.AddrOfPinnedObject();
                
                ComputeBuffer<uint> b_iteration2 =
                    new ComputeBuffer<uint>(context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.CopyHostPointer, 1, ptr_iteration);

                k_prefixSum.SetMemoryArgument(0, b_temp2);
                k_prefixSum.SetMemoryArgument(1, b_iteration2);
                try
                {
                    queue.Execute(k_prefixSum, null, new long[] { num_of_elements }, new long[] { num_of_elements }, null);
                }
                catch (Exception e)
                {
                    File.WriteAllText(Application.StartupPath + @"\exeLog.txt", e.Message);
                }
                queue.Finish();
                b_iteration2.Dispose();
                gch_iteration.Free();
            }

            string sscan = "";

            queue.ReadFromBuffer<uint>(b_temp2, ref temp_array2, false, null);
            queue.ReadFromBuffer<uint>(b_numOfCC, ref nocc, false, null);

            for (int h = 0; h < num_of_elements; h++)
            {
                //TO BE FIXED!
                sscan += temp_array2[h] + "\n";
            }
            File.WriteAllText(Application.StartupPath + @"\scanLog.txt", sscan);
            
            #region CHECK SUM 

            uint sum = 0;
            for (int p = 0; p < num_of_elements; p++) //TODO CHANGE TO nocc[0] -- but why??
                sum += temp_array[p];
            Console.WriteLine(".NET sum: " + sum);
            Console.WriteLine("OCL sum: " + nocc[0]);
            
            #endregion
            
            #endregion

            #region COLLISION CELL ARRAY CREATION

            uint[] outArray = new uint[nocc[0]];
            b_ccArray = new ComputeBuffer<ulong>(context, ComputeMemoryFlags.ReadWrite, nocc[0]);//TODO !!!!! CHANGE TO nocc[0]

            GCHandle gch_ta = GCHandle.Alloc(temp_array, GCHandleType.Pinned);
            IntPtr ptr_ta = gch_ta.AddrOfPinnedObject();

            b_temp3 =
                new ComputeBuffer<ulong>(context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.CopyHostPointer, num_of_elements, ptr_ta);
            b_ccIndexes =
                new ComputeBuffer<uint>(context, ComputeMemoryFlags.ReadWrite, nocc[0]);//TODO!!! CHANGE TO nocc[0]

            k_ccArrayCreation.SetMemoryArgument(0, b_reorder);
            k_ccArrayCreation.SetMemoryArgument(1, b_ccArray);
            k_ccArrayCreation.SetMemoryArgument(2, b_temp2);
            k_ccArrayCreation.SetMemoryArgument(3, b_occPerRad);
            k_ccArrayCreation.SetMemoryArgument(4, b_temp3);
            k_ccArrayCreation.SetMemoryArgument(5, b_ccIndexes);
            k_ccArrayCreation.SetMemoryArgument(6, b_flags);
            try
            {
                queue.Execute(k_ccArrayCreation, null, new long[] { num_of_elements }, null, null);
            }
            catch (Exception e)
            {
                File.WriteAllText(Application.StartupPath + @"\exeLog.txt", e.Message);
            }
            queue.Finish();

            queue.ReadFromBuffer(b_ccArray, ref out_array, false, null);
            queue.Finish();

            #endregion

            Console.WriteLine("ELAPSED TIME: " + time.ElapsedMilliseconds);

            #region CHECK RESULT
            
            string output = "";
            for (int t = 0; t < num_of_elements; t++)//CHANGE TO nocc[0]
            {
                output += "___INDEX___ " + t + "\n\t";
                if ((out_array[t] & ((ulong)1 << 63)) != (ulong)0)
                    output += "[H] ";
                output += (uint)out_array[t] + "\t\n";
            }

            File.WriteAllText(Application.StartupPath + @"\outputLog.txt", output);
            
            #endregion

            #region POINTERS DISPOSAL
            Marshal.FreeHGlobal(ptr);
            gch_ta.Free();
            gch_ec.Free();
            gch_sc.Free();
            gch_ge.Free();
            #endregion

            try
            {
                DisposeBuffers();
            }
            catch (Exception e){
                Console.WriteLine("Error encountered while releasing buffers - " + e.Message);
            }
#else
            Marshal.FreeHGlobal(ptr);
            gch_iai.Free();
            gch_ge.Free();
            b_objectData.Dispose();
            b_objectIDArray.Dispose();
            b_cellIDArray.Dispose();
            b_gridEdge.Dispose();
#endif
        }

        void checkCorrectness()
        {
            log = "";
            List<Body> bodies = Program.window.bodies;
            int nob = Program.window.number_of_bodies;
            for (int i = 0; i < nob; i++)
            {
                if (array[i].ID != bodies[i].getBSphere().bodyIndex
                    || array[i].ctrl_bits != bodies[i].getBSphere().ctrl_bits
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
                        + "\n\n";
                }
            }
        }

        void stopThere()
        {
            try
            {
                DisposeBuffers();
            }
            catch (Exception e) { }
            return;
        }

        public void InitializeComponents()
        {
            queue = new ComputeCommandQueue(context, ComputePlatform.Platforms[0].Devices[0], ComputeCommandQueueFlags.Profiling);
            //ceb = new ComputeEventList();

            kernelStream = new System.IO.StreamReader("Kernels/dataInitialization.cl");
            s_dataInitialization = kernelStream.ReadToEnd();
            kernelStream.Close();

            kernelStream = new System.IO.StreamReader("Kernels/oclRadixSort.cl");
            s_radixSort = kernelStream.ReadToEnd();
            kernelStream.Close();

            kernelStream = new System.IO.StreamReader("Kernels/reorder.cl");
            s_reorder = kernelStream.ReadToEnd();
            kernelStream.Close();

            kernelStream = new System.IO.StreamReader("Kernels/prefixSum.cl");
            s_prefixSum = kernelStream.ReadToEnd();
            kernelStream.Close();

            kernelStream = new System.IO.StreamReader("Kernels/ccArrayCreation.cl");
            s_ccArrayCreation = kernelStream.ReadToEnd();
            kernelStream.Close();

            kernelStream = new System.IO.StreamReader("Kernels/elementCount.cl");
            s_elementCount = kernelStream.ReadToEnd();
            kernelStream.Close();

            p_dataInitialization = new ComputeProgram(context, s_dataInitialization);
            p_dataInitialization.Build(devices, "-g", null, IntPtr.Zero);
            k_dataInitialization = p_dataInitialization.CreateKernel("dataInitialization");

            p_reorder = new ComputeProgram(context, s_reorder);
            p_reorder.Build(devices, "-g", null, IntPtr.Zero);
            k_reorder = p_reorder.CreateKernel("reorder");

            p_elementCount = new ComputeProgram(context, s_elementCount);
            p_elementCount.Build(devices, "-g", null, IntPtr.Zero);
            k_elementCount = p_elementCount.CreateKernel("elementCount");

            p_prefixSum = new ComputeProgram(context, s_prefixSum);
            p_prefixSum.Build(devices, "-g", null, IntPtr.Zero);
            k_prefixSum = p_prefixSum.CreateKernel("prefixSum");

            p_ccArrayCreation = new ComputeProgram(context, s_ccArrayCreation);
            p_ccArrayCreation.Build(devices, "-g", null, IntPtr.Zero);
            k_ccArrayCreation = p_ccArrayCreation.CreateKernel("ccArrayCreation");

            p_radixSort = new ComputeProgram(context, s_radixSort);
            p_radixSort.Build(devices, "-g", null, IntPtr.Zero);
            kernel_block_sort = p_radixSort.CreateKernel("clBlockSort");
            kernel_block_scan = p_radixSort.CreateKernel("clBlockScan");
            kernel_block_prefix = p_radixSort.CreateKernel("clBlockPrefix");
            kernel_reorder = p_radixSort.CreateKernel("clReorder");
        }

        public void DisposeComponents()
        {
            //RELEASE KERNELS
            kernel_block_prefix.Dispose();
            kernel_block_scan.Dispose();
            kernel_block_sort.Dispose();
            kernel_reorder.Dispose();
            k_reorder.Dispose();
            k_elementCount.Dispose();
            k_prefixSum.Dispose();
            k_ccArrayCreation.Dispose();
            k_dataInitialization.Dispose();

            //RELEASE PROGRAMS
            p_elementCount.Dispose();
            p_reorder.Dispose();
            p_radixSort.Dispose();
            p_prefixSum.Dispose();
            p_dataInitialization.Dispose();
            p_ccArrayCreation.Dispose();

            queue.Dispose();
            context.Dispose();
        }

        public void DisposeBuffers()
        {
            //DATA INITIALIZATION
            b_objectData.Dispose();
            b_objectIDArray.Dispose();
            b_cellIDArray.Dispose();
            b_gridEdge.Dispose();
            //RADIX SORT
            b_blockScan.Dispose();
            b_blockOffset.Dispose();
            b_blockSum.Dispose();
            b_temp.Dispose();
            b_iArrayIn.Dispose();
            b_iArrayOut.Dispose();
            b_scanCount.Dispose();
            b_numberOfElems.Dispose();
            //OBJECT ID ARRAY REORDER
            b_reorder.Dispose();
            //ELEMENT COUNT
            b_occPerRad.Dispose();
            b_temp2.Dispose();
            b_flags.Dispose();
            b_numOfCC.Dispose();
            //COLLISION CELL ARRAY CREATION
            b_ccArray.Dispose();
            b_temp3.Dispose();
            b_ccIndexes.Dispose();
        }
    }
}
