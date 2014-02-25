using System;

namespace Clpp.Core.Utilities
{
    /// <summary>
    /// This class is used to wrap another IDisposable
    /// It is useful for keeping track of ownership of an object (i.e. if the object should be disposed by owner or not)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal sealed class OwnedObject<T> : IDisposable where T : IDisposable
    {
        private readonly bool _owned;

        public OwnedObject(T value, bool owned)
        {
            Value = value;
            _owned = owned;
        }

        public T Value { get; private set; }

        public bool IsOwned
        {
            get { return _owned; }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                // get rid of managed resources
                if (IsOwned)
                {
                    Value.Dispose();
                    Value = default(T);
                }
            }
        }
    }
}