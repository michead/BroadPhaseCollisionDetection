#define ALL
#define PRINT

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

        #region PERF
        Stopwatch time;
        Stopwatch time2;
        Stopwatch time3;
        long exe_time;
        long buffer_time;
        long program_time;
        long kernel_time;
        long queue_time;
        long context_time;
        long b_start;
        long b_count;
        long k_start;
        long k_count;
        long p_start;
        long p_count;
        long dataInit_start;
        long radixSort_start;
        long radixSort_count;
        long reorder_start;
        long elemCount_start;
        long prefixSum_start;
        long prefixSum_count;
        long ccArrayC_start;
        #endregion

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

        public void deviceSpecs()
        {
            deviceInfo = "[HOST]\n\t" + Environment.OSVersion + "\n"
                        + "[OPENCL Platform]\n"
                        + "\tName: " + platform.Name + "\n"
                        + "\tVendor: " + platform.Vendor + "\n";
            deviceInfo += "[DEVICES]\n";
            foreach (ComputeDevice device in devices)
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
            time = new Stopwatch();
        }

        public unsafe void CreateCollisionCellArray()
        {
            time.Reset();

            int num_of_bodies = Program.window.number_of_bodies;
            uint num_of_elements = (uint)num_of_bodies * 8;
            float ge = Program.window.grid_edge;

            InitializeQueueAndContext();
            InitializeComponents();

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

            b_start = time.ElapsedMilliseconds;

            b_objectData = new ComputeBuffer<byte>
                (context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.CopyHostPointer, input);
            b_objectIDArray = new ComputeBuffer<ulong>
                (context, ComputeMemoryFlags.ReadWrite, num_of_elements);
            b_cellIDArray = new ComputeBuffer<uint>
                (context, ComputeMemoryFlags.ReadWrite, num_of_elements);
            b_gridEdge = new ComputeBuffer<float>
                (context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.CopyHostPointer, 1, ptr_ge);

            b_count += time.ElapsedMilliseconds - b_start;

            k_dataInitialization.SetMemoryArgument(0, b_objectData);
            k_dataInitialization.SetMemoryArgument(1, b_gridEdge);
            k_dataInitialization.SetMemoryArgument(2, b_cellIDArray);
            k_dataInitialization.SetMemoryArgument(3, b_objectIDArray);

            dataInit_start = time.ElapsedMilliseconds;

            queue.Execute(k_dataInitialization, null, new long[] { num_of_bodies }, null, null);
            queue.Finish();

            Console.WriteLine("TIME SPENT EXECUTIND DATA INITIALIZATION KERNEL: " + (time.ElapsedMilliseconds - dataInit_start) + "ms");

            #endregion
#if PRINT

            uint[] rs_array = new uint[num_of_bodies * 8];
            queue.ReadFromBuffer<uint>(b_cellIDArray, ref rs_array, true, null);
            queue.Finish();

            #region READ DATA INITIALIZED IN DEVICE
            array = new BodyData[num_of_bodies];
            byte[] result = new byte[structSize * num_of_bodies];
            IntPtr intPtr = Marshal.AllocHGlobal(structSize * num_of_bodies);
            queue.ReadFromBuffer<byte>(b_objectData, ref result, true, null);
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

            queue.ReadFromBuffer<uint>(b_cellIDArray, ref unsortedCellIDArray, true, null);
            queue.Finish();
            
            string cellIDArrayLog = "";

            for (int i = 0; i < num_of_elements; i++)
            {
                cellIDArrayLog += "[" + i + "]" + unsortedCellIDArray[i] + "\n";
            }
            File.WriteAllText(Application.StartupPath + @"\unsortedCellIDArrayLog.txt", cellIDArrayLog);

            string objectIDArrayLog = "";

            queue.ReadFromBuffer<ulong>(b_objectIDArray, ref unsortedObjectIDArray, true, null);
            queue.Finish();
            for (int i = 0; i < num_of_elements; i++)
            {
                objectIDArrayLog += "[" + i + "]" + (uint)unsortedObjectIDArray[i] + "\n";
            }
            File.WriteAllText(Application.StartupPath + @"\unsortedObjectIDArrayLog.txt", objectIDArrayLog);

            #endregion
#endif

#if ALL

            #region RADIX SORT
            #region INITIALIZING RADIX SORT MEMBERS
            uint block_count = (uint)Math.Ceiling((float)num_of_elements / BLOCK_SIZE);
            uint lScanCount = block_count * (1 << DIGITS_PER_ROUND) / 4;
            uint lScanSize = (uint)Math.Ceiling(((float)lScanCount / BLOCK_SIZE)) * BLOCK_SIZE;
            uint globalSize = block_count * BLOCK_SIZE;
            #endregion

            #region POPULATING RADIX SORT BUFFERS

            b_start = time.ElapsedMilliseconds;
            
            b_blockScan = new ComputeBuffer<uint>(context, ComputeMemoryFlags.ReadWrite, block_count * (1 << DIGITS_PER_ROUND));
            b_blockOffset = new ComputeBuffer<uint>(context, ComputeMemoryFlags.ReadWrite, block_count * (1 << DIGITS_PER_ROUND));
            b_blockSum = new ComputeBuffer<uint>(context, ComputeMemoryFlags.ReadWrite, BLOCK_SIZE);
            b_temp = new ComputeBuffer<uint>(context, ComputeMemoryFlags.ReadWrite, num_of_elements);
            b_iArrayIn = new ComputeBuffer<uint>(context, ComputeMemoryFlags.ReadWrite
                | ComputeMemoryFlags.CopyHostPointer, num_of_elements, ptr_i);
            b_iArrayOut = new ComputeBuffer<uint>(context, ComputeMemoryFlags.ReadWrite, num_of_elements);

            b_count += time.ElapsedMilliseconds - b_start;

            #endregion

            GCHandle gch_sc = GCHandle.Alloc(lScanCount, GCHandleType.Pinned);
            IntPtr ptr_sc = gch_sc.AddrOfPinnedObject();
            GCHandle gch_ec = GCHandle.Alloc(num_of_elements, GCHandleType.Pinned);
            IntPtr ptr_ec = gch_ec.AddrOfPinnedObject();

            b_start = time.ElapsedMilliseconds;

            b_scanCount =
                    new ComputeBuffer<uint>(context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.CopyHostPointer, 1, ptr_sc);
            b_numberOfElems =
                    new ComputeBuffer<uint>(context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.CopyHostPointer, 1, ptr_ec);

            b_count += time.ElapsedMilliseconds - b_start;
            
            #region libCL RADIX SORT ITERATION
            for (uint j = 0; j < 32; j += DIGITS_PER_ROUND)
            {
                GCHandle gch_iter = GCHandle.Alloc(j, GCHandleType.Pinned);
                IntPtr ptr_j = gch_iter.AddrOfPinnedObject();

                b_start = time.ElapsedMilliseconds;

                ComputeBuffer<uint> b_iteration =
                    new ComputeBuffer<uint>(context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.CopyHostPointer, 1, ptr_j);

                b_count += time.ElapsedMilliseconds - b_start;
            
                #region BLOCK SORT
                kernel_block_sort.SetMemoryArgument(0, b_cellIDArray);
                kernel_block_sort.SetMemoryArgument(1, b_temp);
                kernel_block_sort.SetMemoryArgument(2, b_iArrayIn);
                kernel_block_sort.SetMemoryArgument(3, b_iArrayOut);
                kernel_block_sort.SetMemoryArgument(4, b_iteration);
                kernel_block_sort.SetMemoryArgument(5, b_blockScan);
                kernel_block_sort.SetMemoryArgument(6, b_blockOffset);
                kernel_block_sort.SetMemoryArgument(7, b_numberOfElems);

                radixSort_start = time.ElapsedMilliseconds;

                queue.Execute(kernel_block_sort, null, new long[] { globalSize }, new long[] { BLOCK_SIZE }, null);
                queue.Finish();

                radixSort_count += time.ElapsedMilliseconds - radixSort_start;

                #endregion

            #region BLOCK SCAN
                kernel_block_scan.SetMemoryArgument(0, b_blockScan);
                kernel_block_scan.SetMemoryArgument(1, b_blockSum);
                kernel_block_scan.SetMemoryArgument(2, b_scanCount);

                radixSort_start = time.ElapsedMilliseconds;

                queue.Execute(kernel_block_scan, null, new long[] { lScanSize }, new long[] { BLOCK_SIZE }, null);
                queue.Finish();

                radixSort_count += time.ElapsedMilliseconds - radixSort_start;

                #endregion

            #region BLOCK PREFIX
                kernel_block_prefix.SetMemoryArgument(0, b_blockScan);
                kernel_block_prefix.SetMemoryArgument(1, b_blockSum);
                kernel_block_prefix.SetMemoryArgument(2, b_scanCount);

                radixSort_start = time.ElapsedMilliseconds;

                queue.Execute(kernel_block_prefix, null, new long[] { lScanSize }, new long[] { BLOCK_SIZE }, null);
                queue.Finish();

                radixSort_count += time.ElapsedMilliseconds - radixSort_start;

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

                radixSort_start = time.ElapsedMilliseconds;

                queue.Execute(kernel_reorder, null, new long[] { globalSize }, new long[] { BLOCK_SIZE }, null);
                queue.Finish();

                radixSort_count += time.ElapsedMilliseconds - radixSort_start;

                #endregion

                b_iteration.Dispose();
                gch_iter.Free();
            }
            #endregion

            Console.WriteLine("TIME SPENT EXECUTING RADIX SORT: " + radixSort_count + " ms");

            #endregion

#if PRINT

            #region CHECK CELL ID ARRAY ORDER

            queue.ReadFromBuffer<uint>(b_cellIDArray, ref sortedCellIDArray, true, null);
            queue.Finish();
            
            string orderedCellIDArrayLog = "";

            for (int i = 0; i < num_of_elements; i++)
            {
                orderedCellIDArrayLog += "[" + i + "]" + sortedCellIDArray[i] + "\n";
            }
            File.WriteAllText(Application.StartupPath + @"\radixSortLog.txt", orderedCellIDArrayLog);
            #endregion
#endif
            #region REORDER OBJECT ID ARRAY
            ulong[] o_array = new ulong[num_of_elements];

            b_start = time.ElapsedMilliseconds;

            b_reorder = new ComputeBuffer<ulong>(context, ComputeMemoryFlags.ReadWrite, num_of_elements);

            b_count += time.ElapsedMilliseconds - b_start;

            k_reorder.SetMemoryArgument(0, b_iArrayIn);
            k_reorder.SetMemoryArgument(1, b_objectIDArray);
            k_reorder.SetMemoryArgument(2, b_reorder);

            reorder_start = time.ElapsedMilliseconds;

            queue.Execute(k_reorder, null, new long[] { num_of_elements }, null, null);
            queue.Finish();

            Console.WriteLine("TIME SPENT REORDERING OBJECT ID ARRAY: " + (time.ElapsedMilliseconds - reorder_start) + " ms");

            #endregion
#if PRINT      
            #region CHECK OBJECT ID ORDER

            indexArrayOut = new uint[num_of_elements];
            queue.ReadFromBuffer<uint>(b_iArrayIn, ref indexArrayOut, true, null);
            queue.Finish();

            queue.ReadFromBuffer<ulong>(b_reorder, ref o_array, true, null);
            queue.Finish();

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

#endif
            
            #region ELEMENT COUNT

            //511 -> max value a cell hash can be
            //uint[] occurrences = new uint[512];
            uint[] n_occurrences = new uint[512];
            uint[] temp_array = new uint[num_of_elements];
            uint[] temp_array2 = new uint[num_of_elements];
            uint[] nocc = new uint[1];

            b_start = time.ElapsedMilliseconds;

            b_occPerRad =
                new ComputeBuffer<uint>(context, ComputeMemoryFlags.ReadWrite, 512);
            b_temp2 =
                new ComputeBuffer<uint>(context, ComputeMemoryFlags.ReadWrite, num_of_elements);
            b_flags =
                new ComputeBuffer<uint>(context, ComputeMemoryFlags.ReadWrite, num_of_elements);
            b_numOfCC =
                new ComputeBuffer<uint>(context, ComputeMemoryFlags.ReadWrite, 1);

            b_count = time.ElapsedMilliseconds - b_start;

            k_elementCount.SetMemoryArgument(0, b_reorder);
            k_elementCount.SetMemoryArgument(1, b_temp2);
            k_elementCount.SetMemoryArgument(2, b_numOfCC);
            k_elementCount.SetMemoryArgument(3, b_occPerRad);
            k_elementCount.SetMemoryArgument(4, b_flags);


            elemCount_start = time.ElapsedMilliseconds;

            try{
                queue.Execute(k_elementCount, null, new long[] { num_of_elements }, null, null);
            }
            catch (Exception e)
            {
                File.WriteAllText(Application.StartupPath + @"\exeLog.txt", e.Message);
            }
            queue.Finish();

            Console.WriteLine("TIME SPENT EXECUTING ELEMENT COUNT: " + (time.ElapsedMilliseconds - elemCount_start) + " ms");

            queue.ReadFromBuffer<uint>(b_temp2, ref temp_array, true, null);
            queue.ReadFromBuffer<uint>(b_numOfCC, ref nocc, true, null);
            queue.Finish();

            #endregion

#if PRINT

            #region CHECK ELEMENT COUNT

            queue.ReadFromBuffer<uint>(b_occPerRad, ref n_occurrences, true, null);
            queue.Finish();

            string hs = "";
            for (int h = 0; h < 512; h++)
            {
                hs += "[" + h + "] " +
                    //temp_array[h] + "\n\t" + 
                    n_occurrences[h] + "\n";
            }

            File.WriteAllText(Application.StartupPath + @"\elementCountLog_nOcc.txt", hs);

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
            
            File.WriteAllText(Application.StartupPath + @"\elementCountLog_temp.txt", s);
            #endregion

#endif

            #region PREFIX SUM

            int maxIter = (int)Math.Log(num_of_elements, 2);
            for (uint d = 0; d < maxIter; d++)
            {
                GCHandle gch_iteration = GCHandle.Alloc(d, GCHandleType.Pinned);
                IntPtr ptr_iteration = gch_iteration.AddrOfPinnedObject();

                b_start = time.ElapsedMilliseconds;

                ComputeBuffer<uint> b_iteration2 =
                    new ComputeBuffer<uint>(context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.CopyHostPointer, 1, ptr_iteration);

                b_count += time.ElapsedMilliseconds - b_start;

                k_prefixSum.SetMemoryArgument(0, b_temp2);
                k_prefixSum.SetMemoryArgument(1, b_iteration2);

                prefixSum_start = time.ElapsedMilliseconds;

                try
                {
                    queue.Execute(k_prefixSum, null, new long[] { num_of_elements }, new long[] { num_of_elements }, null);
                }
                catch (Exception e)
                {
                    File.WriteAllText(Application.StartupPath + @"\exeLog.txt", e.Message);
                }
                queue.Finish();

                prefixSum_count += time.ElapsedMilliseconds - prefixSum_start;

                b_iteration2.Dispose();
                gch_iteration.Free();
            }

            Console.WriteLine("TIME SPENT EXECUTING PREFIX SUM: " + prefixSum_count + " ms");

            queue.ReadFromBuffer<uint>(b_temp2, ref temp_array2, true, null);

            #endregion

#if PRINT

            #region CHECK SUM

            string sscan = "";

            for (int h = 0; h < num_of_elements; h++)
            {
                //TO BE FIXED! -- fixed?!
                sscan += temp_array2[h] + "\n";
            }
            File.WriteAllText(Application.StartupPath + @"\scanLog.txt", sscan);

            uint sum = 0;
            for (int p = 0; p < num_of_elements; p++) //TODO CHANGE TO nocc[0] -- but why??
                sum += temp_array[p];
            Console.WriteLine(".NET sum: " + sum);
            Console.WriteLine("OCL sum: " + nocc[0]);
            
            #endregion

#endif

            #region COLLISION CELL ARRAY CREATION

            //uint[] outArray = new uint[nocc[0]];
            ulong[] out_array = new ulong[nocc[0]];
            uint[] indexes = new uint[nocc[0]];
            b_ccArray = new ComputeBuffer<ulong>(context, ComputeMemoryFlags.ReadWrite, nocc[0]);//TODO !!!!! CHANGE TO nocc[0]

            GCHandle gch_ta = GCHandle.Alloc(temp_array, GCHandleType.Pinned);
            IntPtr ptr_ta = gch_ta.AddrOfPinnedObject();

            b_start = time.ElapsedMilliseconds;

            b_temp3 =
                new ComputeBuffer<ulong>(context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.CopyHostPointer, num_of_elements, ptr_ta);
            b_ccIndexes =
                new ComputeBuffer<uint>(context, ComputeMemoryFlags.ReadWrite, nocc[0]);//TODO!!! CHANGE TO nocc[0]

            b_count += time.ElapsedMilliseconds - b_start;

            k_ccArrayCreation.SetMemoryArgument(0, b_reorder);
            k_ccArrayCreation.SetMemoryArgument(1, b_ccArray);
            k_ccArrayCreation.SetMemoryArgument(2, b_temp2);
            k_ccArrayCreation.SetMemoryArgument(3, b_occPerRad);
            k_ccArrayCreation.SetMemoryArgument(4, b_temp3);
            k_ccArrayCreation.SetMemoryArgument(5, b_ccIndexes);
            k_ccArrayCreation.SetMemoryArgument(6, b_flags);

            ccArrayC_start = time.ElapsedMilliseconds;

            try
            {
                queue.Execute(k_ccArrayCreation, null, new long[] { num_of_elements }, null, null);
            }
            catch (Exception e)
            {
                File.WriteAllText(Application.StartupPath + @"\exeLog.txt", e.Message);
            }
            queue.Finish();

            Console.WriteLine("TIME SPENT POPULATING COLLISION CELL ARRAY: " + (time.ElapsedMilliseconds - ccArrayC_start) + " ms");
            time.Stop();

            #endregion

            Console.WriteLine("TIME SPENT EXECUTING BROAD-PHASE COLLISION DETECTION: " + time.ElapsedMilliseconds + " ms");
            Console.WriteLine("TIME SPENT CREATING DEVICE BUFFERS: " + b_count + " ms");

#if PRINT

            #region CHECK RESULT

            queue.ReadFromBuffer(b_ccArray, ref out_array, true, null);
            queue.ReadFromBuffer(b_ccIndexes, ref indexes, true, null);
            queue.Finish();
            
            string output = "";
            for (int t = 0; t < nocc[0]; t++)//CHANGE TO nocc[0]
            {
                output += "---INDEX--- " + t + "\n\t";
                if ((out_array[t] & ((ulong)1 << 63)) != (ulong)0)
                    output += "[H] ";
                output += (uint)out_array[t] + "\n\t";
                output += indexes[t] + "\n";
            }

            File.WriteAllText(Application.StartupPath + @"\outputLog.txt", output);
            
            #endregion

#endif

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
                DisposeComponents();
                DisposeQueueAndContext();
            }
            catch (Exception e){
                Console.WriteLine("Error encountered while releasing buffers - " + e.Message);
                File.WriteAllText(Application.StartupPath + @"\exeLog.txt", e.Message);
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
                DisposeComponents();
                DisposeQueueAndContext();
            }
            catch (Exception e) {
                Console.WriteLine("Error encountered while releasing resources - " + e.Message);
                File.WriteAllText(Application.StartupPath + @"\exeLog.txt", e.Message);
            }
            return;
        }

        public void InitializeComponents()
        {
            //ceb = new ComputeEventList();
            time3 = new Stopwatch();
            time3.Start();

            p_dataInitialization = new ComputeProgram(context, s_dataInitialization);
            p_dataInitialization.Build(devices, "-g", null, IntPtr.Zero);
            p_reorder = new ComputeProgram(context, s_reorder);
            p_reorder.Build(devices, "-g", null, IntPtr.Zero);
            p_elementCount = new ComputeProgram(context, s_elementCount);
            p_elementCount.Build(devices, "-g", null, IntPtr.Zero);
            p_prefixSum = new ComputeProgram(context, s_prefixSum);
            p_prefixSum.Build(devices, "-g", null, IntPtr.Zero);
            p_ccArrayCreation = new ComputeProgram(context, s_ccArrayCreation);
            p_ccArrayCreation.Build(devices, "-g", null, IntPtr.Zero);
            p_radixSort = new ComputeProgram(context, s_radixSort);
            p_radixSort.Build(devices, "-g", null, IntPtr.Zero);

            p_count = time3.ElapsedMilliseconds;

            k_start = time3.ElapsedMilliseconds;

            k_dataInitialization = p_dataInitialization.CreateKernel("dataInitialization");
            k_reorder = p_reorder.CreateKernel("reorder");
            k_elementCount = p_elementCount.CreateKernel("elementCount");
            k_prefixSum = p_prefixSum.CreateKernel("prefixSum");
            k_ccArrayCreation = p_ccArrayCreation.CreateKernel("ccArrayCreation");
            kernel_block_sort = p_radixSort.CreateKernel("clBlockSort");
            kernel_block_scan = p_radixSort.CreateKernel("clBlockScan");
            kernel_block_prefix = p_radixSort.CreateKernel("clBlockPrefix");
            kernel_reorder = p_radixSort.CreateKernel("clReorder");

            k_count = time3.ElapsedMilliseconds - k_start;

            Console.WriteLine("TIME SPENT INITIALIZING AND BUILDING PROGRAMS: " + p_count + "ms");
            Console.WriteLine("TIME SPENT CREATING KERNELS: " + k_count + "ms");
        }

        public void InitializePlatformPropertiesAndDevices()
        {
            platform = ComputePlatform.Platforms[0];
            properties = new ComputeContextPropertyList(platform);
            devices = new List<ComputeDevice>();
            foreach (ComputeDevice device in platform.Devices)
                devices.Add(device);
        }

        public void InitializeQueueAndContext()
        {
            time2 = new Stopwatch();
            time2.Start();
            context = new ComputeContext(devices, properties, null, IntPtr.Zero);
            Console.WriteLine("TIME SPENT INITIALIZING CONTEXT: " + time2.ElapsedMilliseconds + " ms");
            time2.Reset();
            time2.Start();
            queue = new ComputeCommandQueue(context, ComputePlatform.Platforms[0].Devices[0], ComputeCommandQueueFlags.Profiling);
            Console.WriteLine("TIME SPENT INITIALIZING QUEUE: " + time2.ElapsedMilliseconds + " ms");
            time2.Stop();
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
        }

        public void DisposeQueueAndContext()
        {
            queue.Dispose();
            context.Dispose();
        }

        public void ReadAllSources()
        {
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
        }

        public void DisposeBuffers()
        {
            try
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
            catch (Exception e)
            {
                File.WriteAllText(Application.StartupPath + @"\exeLog.txt", e.Message);
            }
        }
    }
}
