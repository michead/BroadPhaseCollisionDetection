using System;
using System.Collections.Generic;
using System.Diagnostics;
using Cloo;
using Clpp.Core.Scan;
using Clpp.Core.Utilities;

namespace Clpp.Core.Sort
{
    public class ClppSortRadixSortGPU : ClppSort
    {
        private readonly long _bits;
        private readonly uint _keySize;
        private readonly bool _keysOnly;
        private readonly long _maxElements;
        private readonly uint _valueSize;
        private readonly long _workgroupSize;
        private ComputeBuffer<byte> _clBuffer_dataSet;
        private ComputeBuffer<byte> _clBuffer_dataSetOut;
        private ComputeBuffer<int> _clBuffer_radixHist1;
        private ComputeBuffer<int> _clBuffer_radixHist2;
        private IntPtr _dataSet;
        private IntPtr _dataSetOut;
        private long _datasetSize;
        private bool _is_clBuffersOwner;
        private ComputeKernel _kernel_LocalHistogram;
        private ComputeKernel _kernel_RadixLocalSort;
        private ComputeKernel _kernel_RadixPermute;
        private ComputeProgram _kernelsProgram;
        private string _kernelsSource;
        private ClppScan<int> _scan;

        public ClppSortRadixSortGPU(ClppContext clppContext, long maxElements, long bits, bool keysOnly) : base(clppContext)
        {
            _maxElements = maxElements;
            _bits = bits;
            _keysOnly = keysOnly;

            _keysOnly = keysOnly;
            _valueSize = 4;
            _keySize = 4;
            _clBuffer_dataSet = null;
            _clBuffer_dataSetOut = null;

            _kernelsSource = GetKernelSource("Clpp.Core.Sort.clppSortRadixSortGPU.cl");
            _kernelsProgram = new ComputeProgram(clppContext.Context, _kernelsSource);

            _kernelsProgram.Build(new List<ComputeDevice>
                                  {
                                      _clppContext.Device
                                  },
                                  null,
                                  null,
                                  IntPtr.Zero);

            _kernel_RadixLocalSort = _kernelsProgram.CreateKernel("kernel__radixLocalSort");

            _kernel_LocalHistogram = _kernelsProgram.CreateKernel("kernel__localHistogram");

            _kernel_RadixPermute = _kernelsProgram.CreateKernel("kernel__radixPermute");

            //---- Get the workgroup size
            _workgroupSize = 32;

            _scan = ClppScan<int>.CreateBest(clppContext, maxElements);
        }

        protected override void Dispose(bool managed)
        {
            if (managed)
            {
                DisposeHelper.Dispose(ref _scan);
                DisposeHelper.Dispose(ref _kernel_RadixPermute);
                DisposeHelper.Dispose(ref _kernel_LocalHistogram);
                DisposeHelper.Dispose(ref _kernel_RadixLocalSort);
                DisposeHelper.Dispose(ref _kernelsProgram);
            }

            base.Dispose(managed);
        }

        public void PopDatas()
        {
            PopDatas(_dataSetOut);
        }

        public void PushDatas(IntPtr dataSet, long datasetSize)
        {
            //---- Store some values
            _dataSet = dataSet;
            _dataSetOut = dataSet;
            var reallocate = datasetSize > _datasetSize || !_is_clBuffersOwner;
            _datasetSize = datasetSize;

            //---- Prepare some buffers
            if (reallocate)
            {
                //---- Release
                if (_clBuffer_dataSet != null)
                {
                    DisposeHelper.Dispose(ref _clBuffer_dataSet);
                    DisposeHelper.Dispose(ref _clBuffer_dataSetOut);
                    DisposeHelper.Dispose(ref _clBuffer_radixHist1);
                    DisposeHelper.Dispose(ref _clBuffer_radixHist2);
                }

                //---- Allocate
                var numBlocks = RoundUpDiv(_datasetSize, _workgroupSize*4);

                // histogram : 16 values per block
                _clBuffer_radixHist1 = new ComputeBuffer<int>(_clppContext.Context, ComputeMemoryFlags.ReadWrite, 16*numBlocks);

                // histogram : 16 values per block
                _clBuffer_radixHist2 = new ComputeBuffer<int>(_clppContext.Context, ComputeMemoryFlags.ReadWrite, 16*numBlocks);

                //---- Copy on the device
                if (_keysOnly)
                {
                    _clBuffer_dataSet = new ComputeBuffer<byte>(_clppContext.Context,
                                                                ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.UseHostPointer,
                                                                _keySize*_datasetSize,
                                                                _dataSet);

                    _clBuffer_dataSetOut = new ComputeBuffer<byte>(_clppContext.Context, ComputeMemoryFlags.ReadWrite, _keySize*_datasetSize);
                }
                else
                {
                    _clBuffer_dataSet = new ComputeBuffer<byte>(_clppContext.Context,
                                                                ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.UseHostPointer,
                                                                (_valueSize + _keySize)*_datasetSize,
                                                                _dataSet);

                    _clBuffer_dataSetOut = new ComputeBuffer<byte>(_clppContext.Context, ComputeMemoryFlags.ReadWrite, (_valueSize + _keySize)*_datasetSize);
                }

                _is_clBuffersOwner = true;
            }
            else
            {
                // Just resend
                if (_keysOnly)
                {
                    _clppContext.CommandQueue.Write(_clBuffer_dataSet, false, 0, _keySize*_datasetSize, _dataSet, null);
                }
                else
                {
                    _clppContext.CommandQueue.Write(_clBuffer_dataSet, false, 0, (_valueSize + _keySize)*_datasetSize, _dataSet, null);
                }
            }
        }

        public void Sort()
        {
            // Satish et al. empirically set b = 4. The size of a work-group is in hundreds of
            // work-items, depending on the concrete device and each work-item processes more than one
            // stream element, usually 4, in order to hide latencies.

            var numBlocks = RoundUpDiv(_datasetSize, _workgroupSize*4);
            var Ndiv4 = RoundUpDiv(_datasetSize, 4);

            var global = new[] {ToMultipleOf(Ndiv4, _workgroupSize)};
            var local = new[] {_workgroupSize};

            ComputeMemory dataA = _clBuffer_dataSet;
            ComputeMemory dataB = _clBuffer_dataSetOut;
            for (var bitOffset = 0; bitOffset < _bits; bitOffset += 4)
            {
                // 1) Each workgroup sorts its tile by using local memory
                // 2) Create an histogram of d=2^b digits entries
#if BENCHMARK
		sw.StartTimer();
#endif

                RadixLocal(global, local, dataA, _clBuffer_radixHist1, _clBuffer_radixHist2, bitOffset);

#if BENCHMARK
		sw.StopTimer();
		cout << "Local sort       " << sw.GetElapsedTime() << endl;

		sw.StartTimer();
#endif

                LocalHistogram(global, local, dataA, _clBuffer_radixHist1, _clBuffer_radixHist2, bitOffset);

#if BENCHMARK
		sw.StopTimer();
		cout << "Local histogram  " << sw.GetElapsedTime() << endl;

		//**********
		//clEnqueueReadBuffer(_context->clQueue, dataA, CL_TRUE, 0, sizeof(int) * _datasetSize, _dataSetOut, 0, NULL, NULL);
		//**********
		
		// 3) Scan the p*2^b = p*(16) entry histogram table. Stored in column-major order, computes global digit offsets.
		sw.StartTimer();
#endif

                _scan.PushCLDatas(_clBuffer_radixHist1);
                _scan.Scan();

#if BENCHMARK
		_scan->waitCompletion();
		sw.StopTimer();
		cout << "Global scan      " << sw.GetElapsedTime() << endl;
        
		// 4) Prefix sum results are used to scatter each work-group's elements to their correct position.
		sw.StartTimer();
#endif

                RadixPermute(global, local, dataA, dataB, _clBuffer_radixHist1, _clBuffer_radixHist2, bitOffset, numBlocks);

#if BENCHMARK
		sw.StopTimer();
		cout << "Global reorder   " << sw.GetElapsedTime() << endl;
#endif

                // swap
                var tmp = dataA;
                dataA = dataB;
                dataB = tmp;
            }

            //if ((_bits/4) % 2 == 0)
            //clEnqueueReadBuffer(_context->clQueue, _clBuffer_dataSet, CL_TRUE, 0, sizeof(int) * _datasetSize, _dataSet, 0, NULL, NULL);
            //else
            //clEnqueueReadBuffer(_context->clQueue, _clBuffer_dataSetOut, CL_TRUE, 0, sizeof(int) * _datasetSize, _dataSet, 0, NULL, NULL);
#if TEST_STEPS

    //---- Test the local sort
    //int mult = _keysOnly ? 1 : 2;

    // Verify that it is locally sorted
    //clEnqueueReadBuffer(_context->clQueue, _clBuffer_dataSetOut, CL_TRUE, 0, sizeof(int) * mult * _datasetSize, _dataSet, 0, NULL, NULL);
	
    //int previous = 0;
    //for(long i = 0; i < _datasetSize; i++)
    //{
    //	int extracted = ((int*)_dataSet)[i*mult];
    //	if (previous > extracted)
    //	{
    //		cout << "Radix sort GPU FAILED - local sort " << endl;
    //		return;
    //	}
    //	previous = extracted;

    //	if (i%32 == 0) previous = 0;
    //}
#endif
        }

        protected override string PreProcess(string programSource)
        {
            var source = "";
            // clpp itself does not yet support templating... todo
            //if (_templateType == Int)
            //{
            //source = _keysOnly ? "#define MAX_KV_TYPE (int)(0x7FFFFFFF)\n" : "#define MAX_KV_TYPE (int2)(0x7FFFFFFF,0xFFFFFFFF)\n";
            //source += "#define K_TYPE int\n";
            //source += _keysOnly ? "#define KV_TYPE int\n" : "#define KV_TYPE int2\n";
            //source += "#define K_TYPE_IDENTITY 0\n";
            //}
            //else if (_templateType == UInt)
            //{
            source = _keysOnly ? "#define MAX_KV_TYPE (uint)(0xFFFFFFFF)\n" : "#define MAX_KV_TYPE (uint2)(0xFFFFFFFF,0xFFFFFFFF)\n";
            source += "#define K_TYPE uint\n";
            source += _keysOnly ? "#define KV_TYPE uint\n" : "#define KV_TYPE uint2\n";
            source += "#define K_TYPE_IDENTITY 0\n";
            //}

            if (_keysOnly)
            {
                source += "#define KEYS_ONLY 1\n";
            }

            return base.PreProcess(source + programSource);
        }

        private void PopDatas(IntPtr dataSet)
        {
            if (_keysOnly)
            {
                if ((_bits/4)%2 == 0)
                {
                    _clppContext.CommandQueue.Read(_clBuffer_dataSet, true, 0, _keySize*_datasetSize, dataSet, null);
                }
                else
                {
                    _clppContext.CommandQueue.Read(_clBuffer_dataSetOut, true, 0, _keySize*_datasetSize, dataSet, null);
                }
            }
            else
            {
                if ((_bits/4)%2 == 0)
                {
                    _clppContext.CommandQueue.Read(_clBuffer_dataSet, true, 0, (_valueSize + _keySize)*_datasetSize, dataSet, null);
                }
                else
                {
                    _clppContext.CommandQueue.Read(_clBuffer_dataSetOut, true, 0, (_valueSize + _keySize)*_datasetSize, dataSet, null);
                }
            }
        }

        private void PushCLDatas(ComputeBuffer<byte> clBuffer_dataSet)
        {
            _is_clBuffersOwner = false;

            var datasetSize = clBuffer_dataSet.Size;

            //---- Store some values
            var reallocate = datasetSize > _datasetSize;
            _datasetSize = datasetSize;

            //---- Prepare some buffers
            if (reallocate)
            {
                //---- Release
                if (_clBuffer_dataSet != null)
                {
                    DisposeHelper.Dispose(ref _clBuffer_dataSet);
                    DisposeHelper.Dispose(ref _clBuffer_dataSetOut);
                    DisposeHelper.Dispose(ref _clBuffer_radixHist1);
                    DisposeHelper.Dispose(ref _clBuffer_radixHist2);
                }

                //---- Allocate
                var numBlocks = RoundUpDiv(_datasetSize, _workgroupSize*4);

                // column size = 2^b = 16

                // histogram : 16 values per block
                _clBuffer_radixHist1 = new ComputeBuffer<int>(_clppContext.Context, ComputeMemoryFlags.ReadWrite, 16*numBlocks);

                // histogram : 16 values per block
                _clBuffer_radixHist2 = new ComputeBuffer<int>(_clppContext.Context, ComputeMemoryFlags.ReadWrite, 16*numBlocks);
            }

            // ISSUE : We need 2 different buffers, but
            // a) when using 32 bits sort(by example) the result buffer is _clBuffer_dataSet
            // b) when using 28 bits sort(by example) the result buffer is _clBuffer_dataSetOut
            // Without copy, how can we do to put the result in _clBuffer_dataSet when using 28 bits ?

            _clBuffer_dataSet = clBuffer_dataSet;

            if (_keysOnly)
            {
                _clBuffer_dataSetOut = new ComputeBuffer<byte>(_clppContext.Context, ComputeMemoryFlags.ReadWrite, _keySize*_datasetSize);
            }
            else
            {
                _clBuffer_dataSetOut = new ComputeBuffer<byte>(_clppContext.Context, ComputeMemoryFlags.ReadWrite, (_valueSize + _keySize)*_datasetSize);
            }
        }

        private long RoundUpDiv(long A, long B)
        {
            return (A + B - 1)/(B);
        }

        private static long ToMultipleOf(long N, long @base)
        {
            return (long) (Math.Ceiling(N/(double) @base)*@base);
        }

        private void LocalHistogram(long[] global, long[] local, ComputeMemory data, ComputeMemory hist, ComputeMemory blockHists, int bitOffset)
        {
            _kernel_LocalHistogram.SetMemoryArgument(0, data);
            _kernel_LocalHistogram.SetValueArgument(1, bitOffset);
            _kernel_LocalHistogram.SetMemoryArgument(2, hist);
            _kernel_LocalHistogram.SetMemoryArgument(3, blockHists);
            _kernel_LocalHistogram.SetValueArgument(4, (uint) _datasetSize);

            _clppContext.CommandQueue.Execute(_kernel_LocalHistogram, null, global, local, null);

#if BENCHMARK
   _clppContext.CommandQueue.Finish(); 
#endif
        }

        private void RadixLocal(long[] global, long[] local, ComputeMemory data, ComputeMemory hist, ComputeMemory blockHists, int bitOffset)
        {
            var a = 0;
            long workgroupSize = 128;

            var Ndiv = RoundUpDiv(_datasetSize, 4); // Each work item handle 4 entries
            var global_128 = new[] {ToMultipleOf(Ndiv, workgroupSize)};
            long[] local_128 = {workgroupSize};

            /*if (_keysOnly)
		clStatus  = clSetKernelArg(_kernel_RadixLocalSort, a++, _keySize * 2 * 4 * workgroupSize, (void*)NULL);
	else
		clStatus  = clSetKernelArg(_kernel_RadixLocalSort, a++, (_valueSize+_keySize) * 2 * 4 * workgroupSize, (void*)NULL);// 2 KV array of 128 items (2 for permutations)*/

            _kernel_RadixLocalSort.SetMemoryArgument(a++, data);
            _kernel_RadixLocalSort.SetValueArgument(a++, bitOffset);
            _kernel_RadixLocalSort.SetValueArgument(a++, (int)_datasetSize);

            _clppContext.CommandQueue.Execute(_kernel_RadixLocalSort, null, global_128, local_128, null);

#if BENCHMARK
   _clppContext.CommandQueue.Finish(); 
#endif
        }

        private void RadixPermute(long[] global,
                                  long[] local,
                                  ComputeMemory dataIn,
                                  ComputeMemory dataOut,
                                  ComputeBuffer<int> histScan,
                                  ComputeBuffer<int> blockHists,
                                  int bitOffset,
                                  long numBlocks)
        {
            _kernel_RadixPermute.SetMemoryArgument(0, dataIn);
            _kernel_RadixPermute.SetMemoryArgument(1, dataOut);
            _kernel_RadixPermute.SetMemoryArgument(2, histScan);
            _kernel_RadixPermute.SetMemoryArgument(3, blockHists);
            _kernel_RadixPermute.SetValueArgument<int>(4, bitOffset);
            _kernel_RadixPermute.SetValueArgument<int>(5, (int)_datasetSize);
            _kernel_RadixPermute.SetValueArgument<int>(6, (int)numBlocks);

            _clppContext.CommandQueue.Execute(_kernel_RadixPermute, null, global, local, null);

#if BENCHMARK
   _clppContext.CommandQueue.Finish(); 
#endif
        }
    }
}