using System;
using Cloo;
using Clpp.Core.Utilities;

namespace Clpp.Core
{
    public class ClppProgram : IDisposable
    {
        protected readonly ClppContext _clppContext;

        public ClppProgram(ClppContext clppContext)
        {
            _clppContext = clppContext;
        }

        ~ClppProgram()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool managed)
        {
            if (managed)
            {
                // get rid of managed resources
            }
            // get rid of unmanaged resources
        }

        protected string GetKernelSource(string programSourceResourcePath)
        {
            var source = EmbeddedResourceUtilities.ReadEmbeddedStream(programSourceResourcePath);
            return PreProcess(source);
        }

        protected virtual string PreProcess(string programSource)
        {
            var source = "";
            switch (_clppContext.Vendor)
            {
                case VendorEnum.Intel:
                {
                    source += "#define OCL_PLATFORM_INTEL\n";
                }
                    break;
                case VendorEnum.AMD:
                {
                    source += "#define OCL_PLATFORM_AMD\n";
                }
                    break;
                case VendorEnum.NVidia:
                {
                    source += "#define OCL_PLATFORM_NVIDIA\n";
                }
                    break;
                default:
                {
                    source += "#define OCL_PLATFORM_UNKNOW\n";
                }
                    break;
            }

            switch (_clppContext.Device.Type)
            {
                case ComputeDeviceTypes.Cpu:
                {
                    source += "#define OCL_DEVICE_CPU\n";
                }
                    break;
                case ComputeDeviceTypes.Gpu:
                {
                    source += "#define OCL_DEVICE_GPU\n";
                }
                    break;
                case ComputeDeviceTypes.Accelerator:
                case ComputeDeviceTypes.All:
                case ComputeDeviceTypes.Default:
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return source + programSource;
        }

        // only if you use unmanaged resources directly in B
    }
}