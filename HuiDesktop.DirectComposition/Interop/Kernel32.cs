﻿using System;
using System.Runtime.InteropServices;

namespace HuiDesktop.DirectComposition.Interop
{
    internal static class Kernel32
    {
        public const string Name = "kernel32";

        [DllImport(Name, CharSet = CharSet.Unicode)]
        public static extern IntPtr LoadLibrary(string fileName);

        [DllImport(Name, CharSet = CharSet.Ansi, BestFitMapping = false)]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

        [DllImport(Name, ExactSpelling = true, SetLastError = true)]
        public static extern bool FreeLibrary([In] IntPtr hModule);

        [DllImport(Name, CharSet = CharSet.Unicode)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport(Name)]
        public static extern IntPtr GetCurrentProcess();

        [DllImport(Name)]
        public static extern IntPtr GetCurrentThread();

        [return: MarshalAs(UnmanagedType.U1)]
        [DllImport(Name, SetLastError = true)]
        public static extern bool GetProcessAffinityMask(IntPtr hProcess, out UIntPtr lpProcessAffinityMask, out UIntPtr lpSystemAffinityMask);

        [DllImport(Name)]
        public static extern UIntPtr SetThreadAffinityMask(IntPtr hThread, UIntPtr dwThreadAffinityMask);
    }
}