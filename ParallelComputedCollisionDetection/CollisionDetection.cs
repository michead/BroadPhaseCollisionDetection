#define CONVERSION

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
using System.IO;

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
        IList<ComputeEventBase> ceb;
        ComputePlatform platform = ComputePlatform.Platforms[0];
        ComputeContext context;
        IList<ComputeDevice> devices;
        public BodyData[] array;
        ComputeProgram program;
        string Arvo;
        string RadixSort;
        string reorder;
        public string log;
        public string deviceInfo;
        public const int CBITS = 4;
        public const int BLOCK_SIZE = 64;
        public const int R = 16;
        public const uint bits_to_sort = 4294967295;
        public uint[] rs_array;

        public void deviceSetUp(){
            ComputeContextPropertyList properties = new ComputeContextPropertyList(platform);
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

        public unsafe void CollisionDetectionSetUp()
        {
            ceb = new List<ComputeEventBase>();
            int nob = Program.window.number_of_bodies;
            uint element_count = (uint)nob * 8;
            uint[] copy = new uint[element_count];
            uint[] indexArrayIn = new uint[element_count];
            for (int j = 0; j < element_count; j++)
                indexArrayIn[j] = (uint)j;
            /*IntPtr ptr_i = Marshal.AllocHGlobal((int)(element_count * sizeof(uint)));
            for(int i=0; i < element_count; i++)
                Marshal.StructureToPtr(indexArrayIn[i], ptr_i + i*sizeof(uint), false);*/
            GCHandle gch_iai = GCHandle.Alloc(indexArrayIn, GCHandleType.Pinned);
            IntPtr ptr_i = gch_iai.AddrOfPinnedObject();
            uint[] indexArrayOut = new uint[element_count];

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
            System.IO.StreamReader kernelStream = new System.IO.StreamReader("Kernels/Arvo.cl");
            Arvo = kernelStream.ReadToEnd();
            kernelStream.Close();
            kernelStream = new System.IO.StreamReader("Kernels/oclRadixSort.cl");
            RadixSort = kernelStream.ReadToEnd();
            kernelStream.Close();
            kernelStream = new System.IO.StreamReader("Kernels/reorder.cl");
            reorder = kernelStream.ReadToEnd();
            kernelStream.Close();
            //GCHandle gch_nob = GCHandle.Alloc(nob, GCHandleType.Pinned);
            //IntPtr anob = gch_nob.AddrOfPinnedObject();
            float ge = Program.window.grid_edge;
            GCHandle gch_ge = GCHandle.Alloc(ge, GCHandleType.Pinned);
            IntPtr age = gch_ge.AddrOfPinnedObject();

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
            ComputeBuffer<ulong> oArray = new ComputeBuffer<ulong>
                (context, ComputeMemoryFlags.ReadWrite, element_count);
            ComputeBuffer<uint> cellArray = new ComputeBuffer<uint>
                (context, ComputeMemoryFlags.ReadWrite, element_count);
            /*ComputeBuffer<int> numOfBodies = new ComputeBuffer<int>
                (context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.CopyHostPointer, sizeof(int), anob);*/
            ComputeBuffer<float> gridEdge = new ComputeBuffer<float>
                (context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.CopyHostPointer, sizeof(float), age);
            program = new ComputeProgram(context, Arvo);
            program.Build(devices, "-g", null, IntPtr.Zero);
            ComputeKernel kernelArvo = program.CreateKernel("Arvo");
            kernelArvo.SetMemoryArgument(0, objArray);
            //kernelArvo.SetMemoryArgument(1, numOfBodies);
            kernelArvo.SetMemoryArgument(1, gridEdge);
            kernelArvo.SetMemoryArgument(2, cellArray);
            kernelArvo.SetMemoryArgument(3, oArray);

            uint[] objIDarray = new uint[element_count];

            #region INITIALIZING RADIX SORT MEMBERS
            uint block_count = (uint)Math.Ceiling((float)element_count / BLOCK_SIZE);
            uint lScanCount = block_count * (1 << CBITS) / 4;
            uint lScanSize = (uint)Math.Ceiling(((float)lScanCount / BLOCK_SIZE)) * BLOCK_SIZE;
            uint globalSize = block_count * BLOCK_SIZE;
            #endregion

            #region COMPILING RADIX SORT
            ComputeProgram rad_sort = new ComputeProgram(context, RadixSort);
            rad_sort.Build(devices, "-g", null, IntPtr.Zero);
            ComputeKernel kernel_block_sort = rad_sort.CreateKernel("clBlockSort");
            ComputeKernel kernel_block_scan = rad_sort.CreateKernel("clBlockScan");
            ComputeKernel kernel_block_prefix = rad_sort.CreateKernel("clBlockPrefix");
            ComputeKernel kernel_reorder = rad_sort.CreateKernel("clReorder");
            #endregion

            #region POPULATING RADIX SORT BUFFERS
            ComputeBuffer<uint> bfBlockScan = new ComputeBuffer<uint>(context, ComputeMemoryFlags.ReadWrite 
                | ComputeMemoryFlags.AllocateHostPointer, block_count * (1 << CBITS));
            ComputeBuffer<uint> bfBlockOffset = new ComputeBuffer<uint>(context, ComputeMemoryFlags.ReadWrite 
                | ComputeMemoryFlags.AllocateHostPointer, block_count * (1 << CBITS));
            ComputeBuffer<uint> bfBlockSum = new ComputeBuffer<uint>(context, ComputeMemoryFlags.ReadWrite
                | ComputeMemoryFlags.AllocateHostPointer, BLOCK_SIZE);
            ComputeBuffer<uint> temp = new ComputeBuffer<uint>(context, ComputeMemoryFlags.ReadWrite
                | ComputeMemoryFlags.AllocateHostPointer, element_count * sizeof(uint));
            ComputeBuffer<uint> iArrayIn = new ComputeBuffer<uint>(context, ComputeMemoryFlags.ReadWrite
                | ComputeMemoryFlags.CopyHostPointer, element_count, ptr_i);
            ComputeBuffer<uint> iArrayOut = new ComputeBuffer<uint>(context, ComputeMemoryFlags.ReadWrite, element_count * sizeof(uint));
            #endregion

            //execution
            ComputeCommandQueue queue = 
                new ComputeCommandQueue(context, ComputePlatform.Platforms[0].Devices[0], ComputeCommandQueueFlags.Profiling);
            queue.Execute(kernelArvo, null, new long[] { nob }, null, null);
            rs_array = new uint[nob * 8];
            queue.ReadFromBuffer<uint>(cellArray, ref rs_array, false, ceb);

            GCHandle gch_sc = GCHandle.Alloc(lScanCount, GCHandleType.Pinned);
            IntPtr ptr_sc = gch_sc.AddrOfPinnedObject();
            GCHandle gch_ec = GCHandle.Alloc(element_count, GCHandleType.Pinned);
            IntPtr ptr_ec = gch_ec.AddrOfPinnedObject();
            
            ComputeBuffer<uint> bf_sc = 
                    new ComputeBuffer<uint>(context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.CopyHostPointer, sizeof(uint), ptr_sc);
            ComputeBuffer<uint> bf_ec =
                    new ComputeBuffer<uint>(context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.CopyHostPointer, sizeof(uint), ptr_ec);

            #region libCL RADIX SORT ITERATION
            for (uint j = 0; j < 32; j += CBITS)
            {
                GCHandle gch_iter = GCHandle.Alloc(j, GCHandleType.Pinned);
                IntPtr ptr_j = gch_iter.AddrOfPinnedObject();

                ComputeBuffer<uint> iter =
                    new ComputeBuffer<uint>(context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.CopyHostPointer, sizeof(uint), ptr_j);

                #region BLOCK SORT
                kernel_block_sort.SetMemoryArgument(0, cellArray);
                kernel_block_sort.SetMemoryArgument(1, temp);
                kernel_block_sort.SetMemoryArgument(2, iArrayIn);
                kernel_block_sort.SetMemoryArgument(3, iArrayOut);
                kernel_block_sort.SetMemoryArgument(4, iter);
                kernel_block_sort.SetMemoryArgument(5, bfBlockScan);
                kernel_block_sort.SetMemoryArgument(6, bfBlockOffset);
                kernel_block_sort.SetMemoryArgument(7, bf_ec);
                queue.Execute(kernel_block_sort, null, new long[] { globalSize }, new long[] { BLOCK_SIZE }, null);
                queue.Finish();
                #endregion

                #region BLOCK SCAN
                kernel_block_scan.SetMemoryArgument(0, bfBlockScan);
                kernel_block_scan.SetMemoryArgument(1, bfBlockSum);
                kernel_block_scan.SetMemoryArgument(2, bf_sc);
                queue.Execute(kernel_block_scan, null, new long[] { lScanSize }, new long[] { BLOCK_SIZE }, null);
                queue.Finish();
                #endregion

                #region BLOCK PREFIX
                kernel_block_prefix.SetMemoryArgument(0, bfBlockScan);
                kernel_block_prefix.SetMemoryArgument(1, bfBlockSum);
                kernel_block_prefix.SetMemoryArgument(2, bf_sc);
                queue.Execute(kernel_block_prefix, null, new long[] { lScanSize }, new long[] { BLOCK_SIZE }, null);
                queue.Finish();
                #endregion

                #region REORDER
                kernel_reorder.SetMemoryArgument(0, temp);
                kernel_reorder.SetMemoryArgument(1, cellArray);
                kernel_reorder.SetMemoryArgument(2, iArrayOut);
                kernel_reorder.SetMemoryArgument(3, iArrayIn);
                kernel_reorder.SetMemoryArgument(4, bfBlockScan);
                kernel_reorder.SetMemoryArgument(5, bfBlockOffset);
                kernel_reorder.SetMemoryArgument(6, iter);
                kernel_reorder.SetMemoryArgument(7, bf_ec);
                queue.Execute(kernel_reorder, null, new long[] { globalSize }, new long[] { BLOCK_SIZE }, null);
                queue.Finish();
                #endregion

                iter.Dispose();
                gch_iter.Free();
            }
            temp.Dispose();
            bf_sc.Dispose();
            bfBlockOffset.Dispose();
            bfBlockScan.Dispose();
            bfBlockSum.Dispose();
            kernel_block_prefix.Dispose();
            kernel_block_scan.Dispose();
            kernel_block_sort.Dispose();
            rad_sort.Dispose();
            #endregion

            queue.ReadFromBuffer<uint>(cellArray, ref copy, false, ceb);

            #region clpp.net RADIX SORT
            /*
            IntPtr rs = Marshal.AllocHGlobal(nob * 8);
            ClppContext clppContext = new ClppContext(ComputeDeviceTypes.Cpu);
            clppContext.PrintInformation();
            Clpp.Core.Sort.ClppSortRadixSortGPU sort = new Clpp.Core.Sort.ClppSortRadixSortGPU(clppContext, nob * 8, (long)bits_to_sort, true);
            queue.ReadFromBuffer<uint>(cellArray, ref rs_array, false, null);
            for (int h = 0; h < nob * 8; h++)
                copy[h] = rs_array[h];
            Array.Sort(copy);
            GCHandle ah = GCHandle.Alloc(rs_array, GCHandleType.Pinned);
            IntPtr a_ah = ah.AddrOfPinnedObject();

            //TODO: modify source to use old compute buffer!  
            sort.PushDatas(a_ah, nob * 8);
            sort.Sort();
            sort.PopDatas();
            ah.Free();*/
            #endregion

            #region reorder
            ulong[] o_array = new ulong[element_count];
            ComputeBuffer<ulong> oA = new ComputeBuffer<ulong>(context, ComputeMemoryFlags.ReadWrite, element_count);
            ComputeProgram reOrder = new ComputeProgram(context, reorder);
            reOrder.Build(devices, "-g", null, IntPtr.Zero);
            ComputeKernel k_reord = reOrder.CreateKernel("reorder");
            k_reord.SetMemoryArgument(0, iArrayIn);
            k_reord.SetMemoryArgument(1, oArray);
            k_reord.SetMemoryArgument(2, oA);
            k_reord.SetMemoryArgument(3, bf_ec);
            queue.Execute(k_reord, null, new long[] { element_count }, null, null);
            queue.Finish();

            queue.ReadFromBuffer<ulong>(oA, ref o_array, false, ceb);

            reOrder.Dispose();
            k_reord.Dispose();
            
            Array.Sort<uint>(rs_array);

            indexArrayOut = new uint[element_count];
            queue.ReadFromBuffer<uint>(iArrayIn, ref indexArrayOut, false, ceb);
            #endregion

            
            #region read from buffer
            array = new BodyData[nob];
            byte[] result = new byte[structSize * nob];
            IntPtr intPtr = Marshal.AllocHGlobal(structSize*nob);
            queue.ReadFromBuffer<byte>(objArray, ref result, false, ceb);
            queue.Finish();

            Marshal.Copy(result, 0, intPtr, structSize * nob); 
            for(int i=0; i < nob; i++)
                array[i] = (BodyData)Marshal.PtrToStructure(intPtr + i*structSize, typeof(BodyData));
            checkCorrectness();
            Marshal.FreeHGlobal(intPtr);
#endregion

            #region COLLISION CELLS -- setup
            string scc = "";
            kernelStream = new System.IO.StreamReader("Kernels/setupCollisionCells.cl");
            scc = kernelStream.ReadToEnd();
            kernelStream.Close();

            uint[] occurrences = new uint[1093];
            uint[] n_occurrences = new uint[1093];
            uint[] temp_array = new uint[element_count];
            ulong[] out_array = new ulong[element_count];
            uint[] nocc = new uint[1];
            //uint[] numOfCC = new uint[1];
            GCHandle gch_temp = GCHandle.Alloc(temp_array, GCHandleType.Pinned);
            IntPtr ptr_temp = gch_temp.AddrOfPinnedObject();
            GCHandle gch_occ = GCHandle.Alloc(occurrences, GCHandleType.Pinned);
            IntPtr ptr_occ = gch_occ.AddrOfPinnedObject();
            GCHandle gch_nOcc = GCHandle.Alloc(n_occurrences, GCHandleType.Pinned);
            IntPtr ptr_nOcc = gch_nOcc.AddrOfPinnedObject();
            //GCHandle gch_outArray = GCHandle.Alloc(out_array, GCHandleType.Pinned);
            //IntPtr ptr_outArray = gch_outArray.AddrOfPinnedObject();
            GCHandle gch_nocc = GCHandle.Alloc(nocc, GCHandleType.Pinned);
            IntPtr ptr_nocc = gch_nocc.AddrOfPinnedObject();

            ComputeBuffer<uint> offset =
                new ComputeBuffer<uint>(context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.CopyHostPointer, 1093, ptr_occ);
            ComputeBuffer<uint> bf_occ_per_radix =
                new ComputeBuffer<uint>(context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.CopyHostPointer, 1093, ptr_nOcc);
            ComputeBuffer<uint> bf_temp =
                new ComputeBuffer<uint>(context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.CopyHostPointer, element_count, ptr_temp);
            /*ComputeBuffer<ulong> bf_outA =
                new ComputeBuffer<ulong>(context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.CopyHostPointer, element_count, ptr_outArray);*/
            ComputeBuffer<uint> numOfCC =
                new ComputeBuffer<uint>(context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.CopyHostPointer, sizeof(uint), ptr_nocc);

            /*ComputeBuffer<uint> bf_tot =
                new ComputeBuffer<uint>(context, ComputeMemoryFlags.ReadWrite, sizeof(uint));*/

            /*IntPtr ptr_num = Marshal.AllocHGlobal(sizeof(uint));
            Marshal.StructureToPtr(numOfCC, ptr_num, false);*/

            ComputeProgram cCellsProgram = new ComputeProgram(context, scc);
            cCellsProgram.Build(devices, "-g", null, IntPtr.Zero);
            ComputeKernel k_ccs = cCellsProgram.CreateKernel("setupCollisionCells");
            k_ccs.SetMemoryArgument(0, oA);
            //k_ccs.SetMemoryArgument(1, bf_outA);
            k_ccs.SetMemoryArgument(1, bf_temp);
            k_ccs.SetMemoryArgument(2, numOfCC);
            k_ccs.SetMemoryArgument(3, offset);
            k_ccs.SetMemoryArgument(4, bf_occ_per_radix);
            //k_ccs.SetMemoryArgument(6, bf_tot);
            try
            {
                queue.Execute(k_ccs, null, new long[] { element_count }, new long[] { element_count }, ceb);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                File.WriteAllText(@"C:\Users\simone\Desktop\exeLog.txt", String.Empty);
                File.WriteAllText(@"C:\Users\simone\Desktop\exeLog.txt", e.Message);
            }
            queue.Finish();

            //queue.ReadFromBuffer<uint>(bf_tot, ref numOfCC, true, null);
            //queue.ReadFromBuffer<ulong>(bf_outA, ref out_array, false, ceb);
            queue.ReadFromBuffer<uint>(bf_temp, ref temp_array, false, ceb);
            queue.ReadFromBuffer<uint>(bf_occ_per_radix, ref n_occurrences, false, ceb);
            queue.ReadFromBuffer<uint>(numOfCC, ref nocc, false, ceb);


            uint sum = 0;
            for (int p = 0; p < element_count; p++)
                sum += temp_array[p];

            Console.WriteLine(sum);
            Console.WriteLine(nocc[0]);

            string hs = "";
            for (int h = 0; h < 1093; h++)
            {
                hs += "[" + h + "] " +
                    //temp_array[h] + "\n\t" + 
                    n_occurrences[h] + "\n";
            }

            uint[] debug = new uint[element_count];
            for (int i = 0; i < element_count; i++)
                debug[i] = (uint)o_array[i];
            Console.WriteLine(debug.Max());


            File.WriteAllText(@"C:\Users\simone\Desktop\crashLog.txt", String.Empty);
            File.WriteAllText(@"C:\Users\simone\Desktop\crashLog.txt", hs);

            string s = "";
            for (int h = 0; h < element_count; h++)
            {
                s += "INDEX: " + h + "\n";
                s += copy[h] + "\n"; //OCL
                //s += (uint)o_array[h] + "\n";
                s += rs_array[h] + "\n"; //CPU
                s += unchecked((uint)o_array[h]) + "\n"; //OCL reordered array cell ID
                s += ((o_array[h] & ((ulong)281470681743360)) >> 32) + "\n"; //OCL reordered array obj ID
                s += (indexArrayOut[h] / 8) + "\n"; // cell index divided by 8 --> should be equal to the line above;
                s += temp_array[h] + "\n";
            }
            File.WriteAllText(@"C:\Users\simone\Desktop\exeLog.txt", String.Empty);
            File.WriteAllText(@"C:\Users\simone\Desktop\exeLog.txt", s);

            #endregion
            //COLLISION CELLS -- creation
            string ccc = "";
            kernelStream = new System.IO.StreamReader("Kernels/createCollisionCells.cl");
            ccc = kernelStream.ReadToEnd();
            kernelStream.Close();

            ComputeProgram cCellsProgram2 = new ComputeProgram(context, ccc);
            cCellsProgram2.Build(devices, "-g", null, IntPtr.Zero);
            ComputeKernel k_ccs2 = cCellsProgram2.CreateKernel("createCollisionCells");

            uint[] outArray = new uint[nocc[0]];
            GCHandle gch_outArray = GCHandle.Alloc(outArray, GCHandleType.Pinned);
            IntPtr ptr_outArray = gch_outArray.AddrOfPinnedObject();

            ComputeBuffer<ulong> bf_outA =
                new ComputeBuffer<ulong>(context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.CopyHostPointer, element_count, ptr_outArray);

            int maxIter = (int)Math.Log(element_count, 2);
            for (uint d = 0; d < maxIter; d++)
            {
                GCHandle gch_iteration = GCHandle.Alloc(d, GCHandleType.Pinned);
                IntPtr ptr_iteration = gch_iteration.AddrOfPinnedObject();
                ComputeBuffer<uint> bf_iteration =
                    new ComputeBuffer<uint>(context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.CopyHostPointer, sizeof(uint), ptr_iteration);
                //k_ccs2.SetMemoryArgument(0, oA);
                //k_ccs2.SetMemoryArgument(1, bf_outA);
                k_ccs2.SetMemoryArgument(0, bf_temp);
                //k_ccs2.SetMemoryArgument(3, numOfCC);
                //k_ccs2.SetMemoryArgument(4, offset);
                //k_ccs2.SetMemoryArgument(5, bf_occ_per_radix);
                k_ccs2.SetMemoryArgument(1, bf_iteration);
                try
                {
                    queue.Execute(k_ccs2, null, new long[] { element_count }, new long[] { element_count }, ceb);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    File.WriteAllText(@"C:\Users\simone\Desktop\exeLog.txt", String.Empty);
                    File.WriteAllText(@"C:\Users\simone\Desktop\exeLog.txt", e.Message);
                }
                queue.Finish();
                gch_iteration.Free();
            }

            string sscan = "";
            
            queue.ReadFromBuffer<uint>(bf_temp, ref temp_array, false, ceb);
            //queue.ReadFromBuffer<uint>(numOfCC, ref nocc, false, ceb);

            for (int h = 0; h < nocc[0]; h++)
            {
                sscan += temp_array[h] + "\n";
            }
            File.WriteAllText(@"C:\Users\simone\Desktop\scanLog.txt", String.Empty);
            File.WriteAllText(@"C:\Users\simone\Desktop\scanLog.txt", sscan);

            //Console.WriteLine("Size: " + nocc[0]);

            #region DISPOSAL REGION -- REMEMBER TO DISPOSE() THE REMAINING BUFFERS IN THE NEAR FUTURE
            Marshal.FreeHGlobal(ptr);
            cCellsProgram.Dispose();
            gch_outArray.Free();
            bf_outA.Dispose();
            k_ccs2.Dispose();
            cCellsProgram2.Dispose();
            k_ccs.Dispose();
            offset.Dispose();
            gch_nocc.Free();
            gch_temp.Free();
            gch_occ.Free();
            gch_nOcc.Free();
            kernelArvo.Dispose();
            program.Dispose();
            bf_ec.Dispose();
            bf_occ_per_radix.Dispose();
            bf_outA.Dispose();
            oA.Dispose();
            oArray.Dispose();
            bf_temp.Dispose();
            //bf_tot.Dispose();
            gch_ec.Free();
            gch_sc.Free();
            gch_ge.Free();
            //gch_nob.Free();
            cellArray.Dispose();
            objArray.Dispose();
            //numOfBodies.Dispose();
            gridEdge.Dispose();
            queue.Dispose();
            context.Dispose();
#endregion
        }

        void checkCorrectness()
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
                        + "\n\n";
                }
            }
        }
    }
}
