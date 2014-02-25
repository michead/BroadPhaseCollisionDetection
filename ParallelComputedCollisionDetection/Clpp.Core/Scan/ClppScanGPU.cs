using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Cloo;
using Clpp.Core.Utilities;

namespace Clpp.Core.Scan
{
    public class ClppScanGPU<T> : ClppScan<T> where T : struct
    {
        private ComputeKernel _kernelScan;
        private ComputeProgram _kernelScanProgram;
        private string _kernelSource;

        public ClppScanGPU(ClppContext clppContext, long maxElements)
            : base(clppContext, maxElements)
        {
            System.IO.StreamReader kernelStream = new System.IO.StreamReader("clppScanGPU.cl");
            _kernelSource = kernelStream.ReadToEnd();
            //_kernelSource = GetKernelSource("Clpp.Core.Scan.clppScanGPU.cl");
            _kernelScanProgram = new ComputeProgram(clppContext.Context, _kernelSource);


#if __APPLE__
    //const char buildOptions = "-DMAC -cl-fast-relaxed-math";
	const string buildOptions = "";
#else
            //const char* buildOptions = "-cl-fast-relaxed-math";
            const string buildOptions = "";
#endif

            _kernelScanProgram.Build(new List<ComputeDevice>
                                     {
                                         _clppContext.Device
                                     },
                                     buildOptions,
                                     null,
                                     IntPtr.Zero);


            _kernelScan = _kernelScanProgram.CreateKernel("kernel__scan_block_anylength");

            //---- Get the workgroup size
            // ATI : Actually the wavefront size is only 64 for the highend cards(48XX, 58XX, 57XX), but 32 for the middleend cards and 16 for the lowend cards.
            // NVidia : 32
            _workGroupSize = (uint) _kernelScan.GetWorkGroupSize(_clppContext.Device);

            _isClBuffersOwner = false;
        }

        ~ClppScanGPU()
        {
            Dispose(false);
        }

        protected override void Dispose(bool managed)
        {
            if (managed)
            {
                DisposeHelper.Dispose(ref _kernelScan);
                DisposeHelper.Dispose(ref _kernelScanProgram);

                if (_isClBuffersOwner)
                {
                    DisposeHelper.Dispose(ref _clBufferValues);
                }
            }
            //release unmanaged resources

            base.Dispose(managed);
        }

        public override void PopDatas()
        {
            _clppContext.CommandQueue.Read(_clBufferValues, true, 0, _valuesCount, _values, null);
        }

        public override void PopDatas(IntPtr dataSetPtr, long dataSetCount)
        {
            if (dataSetCount < _valuesCount)
                throw new ArgumentException("buffer not big enough", "dataSetCount");
            
            _clppContext.CommandQueue.Read(_clBufferValues, true, 0, _valuesCount, dataSetPtr, null);
        }

        public override void PushCLDatas(ComputeBuffer<T> clBufferValues)
        {
            _values = IntPtr.Zero;
            _valuesCount = clBufferValues.Count;

            _isClBuffersOwner = false;

            _clBufferValues = clBufferValues;
        }

        public override void PushDatas(IntPtr inBuffer, long inBufferCount)
        {
            //---- Store some values
            _values = inBuffer;
            var reallocate = inBufferCount > _valuesCount || !_isClBuffersOwner;
            _valuesCount = inBufferCount;

            //---- Copy on the device
            if (reallocate)
            {
                //---- Release
                if (_clBufferValues != null)
                {
                    _clBufferValues.Dispose();
                }

                //---- Allocate & copy on the device
                _clBufferValues = new ComputeBuffer<T>(_clppContext.Context,
                                                          ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.UseHostPointer,
                                                          _valuesCount,
                                                          _values);
                _isClBuffersOwner = true;
            }
            else
            {
                // Just resend
                _clppContext.CommandQueue.Write(_clBufferValues, false, 0, inBufferCount, inBuffer, null);
            }
        }

        public override void Scan()
        {
            var blockSize = _valuesCount / _workGroupSize;
            var B = blockSize*_workGroupSize;
            if ((_valuesCount % _workGroupSize) > 0)
            {
                blockSize++;
            }

            var localWorkSize = new long[] {_workGroupSize};
            var globalWorkSize = new[] { ToMultipleOf(_valuesCount / blockSize, _workGroupSize) };

            _kernelScan.SetLocalArgument(0, _workGroupSize * _valueSize);
            _kernelScan.SetMemoryArgument(1, _clBufferValues);
            _kernelScan.SetValueArgument<uint>(2, (uint) B);
            _kernelScan.SetValueArgument<uint>(3, (uint)_valuesCount);
            _kernelScan.SetValueArgument<uint>(4, (uint) blockSize);

            _clppContext.CommandQueue.Execute(_kernelScan, null, globalWorkSize, localWorkSize, null);

            //            _clppContext.CommandQueue.Wait(ev);
        }

        private static long ToMultipleOf(long N, long @base)
        {
            return (long) (Math.Ceiling((double) N/@base)*@base);
        }
    }
}