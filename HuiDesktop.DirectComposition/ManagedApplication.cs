using HuiDesktop.DirectComposition.Interop;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using HuiDesktop.DirectComposition.DirectX;
using Vortice.Direct3D11;
using System.Threading;

namespace HuiDesktop.DirectComposition
{
    public class ManagedApplication
    {
        public static readonly IntPtr HInstance = Kernel32.GetModuleHandle(string.Empty);
        public static readonly string WndClassName = "DxTestWindow";
        public readonly WNDPROC windowProc;
        public MainWindow mainWindow;
        public event Action<Point> OnMouseLeftDown;
        public event Action<Point> OnMouseLeftUp;
        private Device device;
        private ID3D11DeviceContext ctx;

        bool ready;

        public ManagedApplication()
        {
            windowProc = ProcessWindowMessage;

            var wndClassEx = new WNDCLASSEX
            {
                Size = Unsafe.SizeOf<WNDCLASSEX>(),
                Styles = WindowClassStyles.CS_HREDRAW | WindowClassStyles.CS_VREDRAW | WindowClassStyles.CS_OWNDC,
                WindowProc = windowProc,
                InstanceHandle = HInstance,
                CursorHandle = User32.LoadCursor(IntPtr.Zero, SystemCursor.IDC_ARROW),
                BackgroundBrushHandle = IntPtr.Zero,
                IconHandle = IntPtr.Zero,
                ClassName = WndClassName,
            };

            if (User32.RegisterClassEx(ref wndClassEx) == 0)
            {
                DebugHelper.CheckWin32Error();
            }

            device = new Device(out ctx);
            mainWindow = new MainWindow(1024, 1024, ctx, device);
            //hitTestWindow = new HitTestWindow(8, 8, this, device.RequestHitTest);
            //device.OnStartCopy += () => hitTestWindow.blockUpdate = true;
            //device.OnCopiedBitmap += hitTestWindow.OnMapped;

            ready = true;
        }

        private IntPtr ProcessWindowMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            if (!ready) return User32.DefWindowProc(hWnd, msg, wParam, lParam);

            Point GetRelativePoint()
            {
                var cursorPoint = GetPointByLParam(lParam);
                return new(
                            cursorPoint.X + mainWindow.hitTestWindow.Left - mainWindow.Left,
                            cursorPoint.Y + mainWindow.hitTestWindow.Top - mainWindow.Top);
            }
            if (hWnd == mainWindow.hitTestWindow?.Handle)
            {
                switch ((WindowMessage)msg)
                {
                    case WindowMessage.MouseMove:
                        if (mainWindow.hitTestWindow.captured)
                            Debug.WriteLine(GetRelativePoint());
                        return IntPtr.Zero;
                    case WindowMessage.Destroy:
                        User32.PostQuitMessage(0);
                        return IntPtr.Zero;
                    case WindowMessage.LButtonDown:
                        mainWindow.MouseDown(MouseButton.Left, GetRelativePoint());
                        return IntPtr.Zero;
                    case WindowMessage.LButtonUp:
                        mainWindow.MouseUp(MouseButton.Left, GetRelativePoint());
                        return IntPtr.Zero;
                    case WindowMessage.RButtonDown:
                        mainWindow.MouseDown(MouseButton.Right, GetRelativePoint());
                        return IntPtr.Zero;
                    case WindowMessage.RButtonUp:
                        mainWindow.MouseUp(MouseButton.Right, GetRelativePoint());
                        return IntPtr.Zero;
                }
                User32.PostMessage(mainWindow.Handle, (WindowMessage)msg, wParam, lParam);
                return User32.DefWindowProc(hWnd, msg, wParam, lParam);
            }

            switch ((WindowMessage)msg)
            {
                case WindowMessage.Destroy:
                    User32.PostQuitMessage(0);
                    break;
            }
            return User32.DefWindowProc(hWnd, msg, wParam, lParam);
        }

        private Point GetPointByLParam(IntPtr lParam)
        {
            return new Point((short)((int)lParam.ToInt64() & 0xffff), (short)((int)lParam.ToInt64() >> 16));
        }

        public void Run()
        {
            const int PM_REMOVE = 1;
            Message msg = new Message();
            while (msg.Value != (uint)WindowMessage.Quit)
            {
                if (User32.PeekMessage(out msg, IntPtr.Zero, 0, 0, PM_REMOVE))
                {
                    User32.TranslateMessage(ref msg);
                    User32.DispatchMessage(ref msg);
                }
                else
                {
                    mainWindow.Render(ctx);
                }
                Thread.Sleep(1);
            }
        }
    }
}
