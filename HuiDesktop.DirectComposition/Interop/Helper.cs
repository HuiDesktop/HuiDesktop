using System;
using System.Runtime.InteropServices;

namespace HuiDesktop.DirectComposition.Interop
{
    internal static class Helper
    {
        public unsafe static T GetDelegateByOffset<T>(this IntPtr ptr, long offset) where T : Delegate
               => Marshal.GetDelegateForFunctionPointer<T>(*(IntPtr*)((long)*(IntPtr*)ptr + (offset * sizeof(void*))));
    }
}
