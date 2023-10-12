using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security;

namespace Microsoft.Win32.SafeHandles
{
    [SuppressUnmanagedCodeSecurityAttribute]
    internal sealed class SafeProcessingHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        internal static SafeProcessingHandle InvalidHandle = new SafeProcessingHandle(IntPtr.Zero);

        internal SafeProcessingHandle() : base(true) { }

        internal SafeProcessingHandle(IntPtr handle) : base(true)
        {
            SetHandle(handle);
        }

        [DllImport("kernel32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto, SetLastError = true)]
        internal static extern SafeProcessingHandle OpenProcess(int access, bool inherit, int processId);

        internal void InitialSetHandle(IntPtr h)
        {
            Debug.Assert(base.IsInvalid, "Safe handle should only be set once");
            base.handle = h;
        }

        protected override bool ReleaseHandle()
        {
            return SafeMethods.CloseHandle(handle);
        }
    }
}