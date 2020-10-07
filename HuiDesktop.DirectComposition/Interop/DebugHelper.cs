using System;
using System.Runtime.InteropServices;

namespace HuiDesktop.DirectComposition.Interop
{
    internal class DebugHelper
    {
        public static void CheckWin32Error()
        {
            if (Marshal.GetLastWin32Error() != 0)
            {
                throw new Exception($"Win32 error code: {Marshal.GetLastWin32Error()}");
            }
        }
    }
}
