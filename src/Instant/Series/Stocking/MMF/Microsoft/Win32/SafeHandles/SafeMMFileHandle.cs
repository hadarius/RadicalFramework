using System;
using Radical.Instant.Series.Stocking;

namespace Microsoft.Win32.SafeHandles
{
#if !NET40Plus

    public sealed class SafeMMFileHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        internal SafeMMFileHandle()
            : base(true) { }

        internal SafeMMFileHandle(IntPtr handle, bool ownsHandle)
            : base(ownsHandle)
        {
            base.SetHandle(handle);
        }

        protected override bool ReleaseHandle()
        {
            try
            {
                return UnsafeMethods.CloseHandle(this.handle);
            }
            finally
            {
                this.handle = IntPtr.Zero;
            }
        }
    }
#endif
}