using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Cloo;

namespace Clpp.Core.Scan
{
    public abstract class ClppScan<T> : ClppProgram where T : struct
    {
        protected readonly long _maxElements;
        protected ComputeBuffer<T> _clBufferValues;

        protected bool _isClBuffersOwner;
        protected int _valueSize;

        protected IntPtr _values;
        protected long _valuesCount;
        protected long _workGroupSize;

        private readonly Dictionary<Type, string> _typeMap = new Dictionary<Type, string>
                                                             {
                                                                 {typeof (float), "float"},
                                                                 {typeof (double), "double"},
                                                                 {typeof (int), "int"},
                                                                 {typeof (uint), "uint"},
                                                                 {typeof (long), "long"},
                                                                 {typeof (ulong), "ulong"},
                                                             };

        public ClppScan(ClppContext clppContext, long maxElements) : base(clppContext)
        {
            _valueSize = Marshal.SizeOf(typeof (T));
            _maxElements = maxElements;
        }

        public abstract void PopDatas();
        public abstract void PopDatas(IntPtr outBuffer, long sizeBytes);
        public abstract void PushCLDatas(ComputeBuffer<T> inBuffer);
        public abstract void PushDatas(IntPtr inBuffer, long sizeBytes);
        public abstract void Scan();

        protected override string PreProcess(string programSource)
        {
            var source = "";

            if (typeof (T) == typeof(double))
            {
                source += "#pragma OPENCL EXTENSION cl_khr_fp64 : enable\n";
            }

            source += string.Format("#define T {0}\n", _typeMap[typeof (T)]);


            source += programSource;

            return base.PreProcess(source);
        }

        public static ClppScan<T> CreateBest(ClppContext clppContext, long maxElements)
        {
            return CreateBest(clppContext.Device.Type, clppContext, maxElements);
        }


        public static ClppScan<T> CreateBest(ComputeDeviceTypes deviceTypes, ClppContext clppContext, long maxElements) 
        {
            switch (deviceTypes)
            {
                case ComputeDeviceTypes.Default:
                case ComputeDeviceTypes.Cpu:
                case ComputeDeviceTypes.Accelerator:
                case ComputeDeviceTypes.All:
                    return new ClppScanDefault<T>(clppContext, maxElements);
                case ComputeDeviceTypes.Gpu:
                    return new ClppScanGPU<T>(clppContext, maxElements);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}