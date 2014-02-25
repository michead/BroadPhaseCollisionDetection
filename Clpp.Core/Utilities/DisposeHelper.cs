using System;

namespace Clpp.Core.Utilities
{
    public class DisposeHelper
    {
        public static void Dispose<T>(ref T disposable) where T: IDisposable
        {
            if (disposable != null)
            {
                disposable.Dispose();
                disposable = default(T);
            }
        } 
    }
}