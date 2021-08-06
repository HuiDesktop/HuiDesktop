using System;
using System.Runtime.InteropServices;

namespace HuiDesktop.DirectComposition.Interop
{
    internal static class DComp
    {
        public const string name = "dcomp.dll";

        [DllImport(name)]
        public unsafe static extern int DCompositionCreateDevice(IntPtr dxgiDevice, Guid* iid, IntPtr* dcompositionDevice);
    }
}