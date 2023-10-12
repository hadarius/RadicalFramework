using System;
using Radical.Instant.Series.Stocking;

namespace Microsoft.Win32.SafeHandles
{
#if !NET40Plus

    public sealed class SafeMMViewHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        internal SafeMMViewHandle()
            : base(true) { }

        internal SafeMMViewHandle(IntPtr handle, bool ownsHandle)
            : base(ownsHandle)
        {
            base.SetHandle(handle);
        }

        protected override bool ReleaseHandle()
        {
            try
            {
                return UnsafeMethods.UnmapViewOfFile(this.handle);
            }
            finally
            {
                this.handle = IntPtr.Zero;
            }
        }

        public unsafe void AcquireIntPtr(ref byte* pointer)
        {
            bool flag = false;
            base.DangerousAddRef(ref flag);
            pointer = (byte*)this.handle.ToPointer();
        }

        public void ReleaseIntPtr()
        {
            base.DangerousRelease();
        }
    }
#endif
}