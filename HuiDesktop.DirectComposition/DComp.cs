using System;
using HuiDesktop.DirectComposition.Interop;
using SharpGen.Runtime;
using Vortice.DXGI;

namespace HuiDesktop.DirectComposition
{
    public class DCompositionDevice
    {
        private static readonly Guid _comRefiid = new Guid("C37EA93A-E7AA-450D-B16F-9746CB0407F3");
        private IntPtr _nativePointer;
        public IntPtr NativePointer => _nativePointer;

        public DCompositionDevice(IDXGIDevice device)
        {
            Result result;
            unsafe
            {
                fixed (Guid* iidptr = &_comRefiid)
                fixed (IntPtr* ptr = &_nativePointer)
                {
                    result = DComp.DCompositionCreateDevice(device.NativePointer, iidptr, ptr);
                }
            }
            result.CheckError();
        }

        private unsafe delegate int CreateTargetForHwndMethodDelegate(IntPtr thisv, IntPtr hwnd, bool topmost, IntPtr* target);
        private unsafe delegate int CreateVisualMethodDelegate(IntPtr thisv, IntPtr* visual);
        private delegate int CommitMethodDelegate(IntPtr thisv);
        private CreateTargetForHwndMethodDelegate CreateTargetForHwndMethod
            => _nativePointer.GetDelegateByOffset<CreateTargetForHwndMethodDelegate>(6);
        private CreateVisualMethodDelegate CreateVisualMethod
            => _nativePointer.GetDelegateByOffset<CreateVisualMethodDelegate>(7);
        private CommitMethodDelegate CommitMethod
            => _nativePointer.GetDelegateByOffset<CommitMethodDelegate>(3);

        public DCompositionTarget CreateTargetForHwnd(IntPtr hWnd, bool topmost)
        {
            Result result;
            IntPtr targetPointer;
            unsafe
            {
                result = CreateTargetForHwndMethod(_nativePointer, hWnd, topmost, &targetPointer);
            }
            result.CheckError();
            if (targetPointer == IntPtr.Zero) throw new NullReferenceException("Unexpected null targetPointer.");
            return new DCompositionTarget(targetPointer);
        }
        public DCompositionVisual CreateVisual()
        {
            Result result;
            IntPtr targetPointer;
            unsafe
            {
                result = CreateVisualMethod(_nativePointer, &targetPointer);
            }
            result.CheckError();
            if (targetPointer == IntPtr.Zero) throw new NullReferenceException("Unexpected null targetPointer.");
            return new DCompositionVisual(targetPointer);
        }
        public Result Commit()
        {
            return CommitMethod(_nativePointer);
        }
    }

    public class DCompositionTarget
    {
        private IntPtr _nativePointer;
        public IntPtr NativePointer => _nativePointer;

        public DCompositionTarget(IntPtr nativePointer)
        {
            _nativePointer = nativePointer;
        }

        private delegate int SetRootMethodDelegate(IntPtr thisv, IntPtr visual);

        private SetRootMethodDelegate SetRootMethod
            => _nativePointer.GetDelegateByOffset<SetRootMethodDelegate>(3);
        public Result SetRoot(DCompositionVisual visual)
        {
            return SetRootMethod(_nativePointer, visual.NativePointer);
        }
    }

    public class DCompositionVisual
    {
        private IntPtr _nativePointer;
        public IntPtr NativePointer => _nativePointer;

        public DCompositionVisual(IntPtr nativePointer)
        {
            _nativePointer = nativePointer;
        }

        private delegate int SetContentMethodDelegate(IntPtr thisv, IntPtr content);
        private SetContentMethodDelegate SetContentMethod
            => _nativePointer.GetDelegateByOffset<SetContentMethodDelegate>(15);

        public Result SetContent(ComObject content)
        {
            return SetContentMethod(_nativePointer, content.NativePointer);
        }
    }
}