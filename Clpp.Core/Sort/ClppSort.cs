using System;
using Cloo;

namespace Clpp.Core.Sort
{
    public abstract class ClppSort : ClppProgram
    {
        public ClppSort(ClppContext clppContext) : base(clppContext) {}

        public static IDisposable CreateBest(ComputeDeviceTypes deviceTypes, ClppContext clppContext, long maxElements, int sortBits, bool keysOnly)
        {
            switch (deviceTypes)
            {
                case ComputeDeviceTypes.Gpu:
                    return new ClppSortRadixSortGPU(clppContext, maxElements, sortBits, keysOnly);
                case ComputeDeviceTypes.Default:
                case ComputeDeviceTypes.Cpu:
                case ComputeDeviceTypes.Accelerator:
                case ComputeDeviceTypes.All:
                    throw new NotImplementedException();
                default:
                    throw new ArgumentOutOfRangeException("deviceTypes");
            }
        }
    }
}