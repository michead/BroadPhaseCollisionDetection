using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Cloo;
using Clpp.Core.Utilities;

namespace Clpp.Core
{
    public class ClppContext : IDisposable
    {
        private readonly ComputePlatform _platform;
        private OwnedObject<ComputeCommandQueue> _commandQueue;
        private OwnedObject<ComputeContext> _context;

        public ClppContext() : this(0, 0) {}


        public ClppContext(ComputeDeviceTypes deviceTypes)
        {
            var platforms = ComputePlatform.Platforms;
            _platform =  platforms.First(a=>a.Devices.Any(b=>b.Type.HasFlag(deviceTypes)));

            if (_platform.Vendor.Equals("Intel", StringComparison.OrdinalIgnoreCase))
            {
                Vendor = VendorEnum.Intel;
            }
            else if (_platform.Vendor.Equals("AMD"))
            {
                Vendor = VendorEnum.AMD;
            }
            else if (_platform.Vendor.Equals("Advanced Micro Devices"))
            {
                Vendor = VendorEnum.AMD;
            }
            else if (_platform.Vendor.Equals("NVidia"))
            {
                Vendor = VendorEnum.NVidia;
            }
            else if (_platform.Vendor.Equals("Apple"))
            {
                Vendor = VendorEnum.NVidia;
            }

            Device = _platform.Devices.First(a => a.Type.HasFlag(deviceTypes));

            var context = new ComputeContext(new List<ComputeDevice>
                                             {
                                                 Device
                                             },
                                             new ComputeContextPropertyList(_platform),
                                             ErrorHandler,
                                             IntPtr.Zero);
            _context = new OwnedObject<ComputeContext>(context, true);

            var commandQueue = new ComputeCommandQueue(Context, Device, ComputeCommandQueueFlags.Profiling);
            _commandQueue = new OwnedObject<ComputeCommandQueue>(commandQueue, true);
        }

        public ClppContext(int platformId, int deviceId)
        {
            var platforms = ComputePlatform.Platforms;

            platformId = Math.Min(platformId, platforms.Count);
            _platform = platforms[platformId];

            if (_platform.Vendor.Equals("Intel", StringComparison.OrdinalIgnoreCase))
            {
                Vendor = VendorEnum.Intel;
            }
            else if (_platform.Vendor.Equals("AMD"))
            {
                Vendor = VendorEnum.AMD;
            }
            else if (_platform.Vendor.Equals("Advanced Micro Devices"))
            {
                Vendor = VendorEnum.AMD;
            }
            else if (_platform.Vendor.Equals("NVidia"))
            {
                Vendor = VendorEnum.NVidia;
            }
            else if (_platform.Vendor.Equals("Apple"))
            {
                Vendor = VendorEnum.NVidia;
            }

            Device = _platform.Devices[Math.Min(deviceId, _platform.Devices.Count)];

            var context = new ComputeContext(new List<ComputeDevice>
                                             {
                                                 Device
                                             },
                                             new ComputeContextPropertyList(_platform),
                                             ErrorHandler,
                                             IntPtr.Zero);
            _context = new OwnedObject<ComputeContext>(context, true);

            var commandQueue = new ComputeCommandQueue(Context, Device, ComputeCommandQueueFlags.Profiling);
            _commandQueue = new OwnedObject<ComputeCommandQueue>(commandQueue, true);
        }

        public ClppContext(ComputeDevice device, ComputeContext context, ComputeCommandQueue commandQueue)
        {
            _platform = device.Platform;

            if (_platform.Vendor.Equals("Intel", StringComparison.OrdinalIgnoreCase))
            {
                Vendor = VendorEnum.Intel;
            }
            else if (_platform.Vendor.Equals("AMD"))
            {
                Vendor = VendorEnum.AMD;
            }
            else if (_platform.Vendor.Equals("Advanced Micro Devices"))
            {
                Vendor = VendorEnum.AMD;
            }
            else if (_platform.Vendor.Equals("NVidia"))
            {
                Vendor = VendorEnum.NVidia;
            }
            else if (_platform.Vendor.Equals("Apple"))
            {
                Vendor = VendorEnum.NVidia;
            }

            Device = device;

            _context = new OwnedObject<ComputeContext>(context, false);

            _commandQueue = new OwnedObject<ComputeCommandQueue>(commandQueue, false);
        }


        ~ClppContext()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // get rid of managed resources
                DisposeHelper.Dispose(ref _commandQueue);
                DisposeHelper.Dispose(ref _context);
            }
            // get rid of unmanaged resources
        }

        public ComputeCommandQueue CommandQueue
        {
            get { return _commandQueue.Value; }
        }

        public ComputeContext Context
        {
            get { return _context.Value; }
        }

        public ComputeDevice Device { get; private set; }

        // only if you use unmanaged resources directly in B

        public VendorEnum Vendor { get; private set; }

        public void PrintInformation()
        {
            Console.WriteLine("OpenCL Platform : " + _platform.Name);
            Console.WriteLine("OpenCL Device : " + Device.Name);
        }

        private void ErrorHandler(string errorinfo, IntPtr cldataptr, IntPtr cldatasize, IntPtr userdataptr)
        {
            Debug.WriteLine(errorinfo);
        }
    }

    public enum VendorEnum
    {
        Intel,
        AMD,
        NVidia
    }
}