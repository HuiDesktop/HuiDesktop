using HuiDesktop.DirectComposition.Interop;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace HuiDesktop.DirectComposition
{
    public class ManagedApplication
    {
        public static readonly IntPtr HInstance = Kernel32.GetModuleHandle(string.Empty);
        public static readonly string WndClassName = "DxTestWindow";
        public readonly WNDPROC windowProc;
        public MainWindow mainWindow;
        public HitTestWindow hitTestWindow;
        public event Action<Point> OnMouseLeftDown;
        public event Action<Point> OnMouseLeftUp; 
        internal Device device;

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

            mainWindow = new MainWindow("HuiDesktop Direct Composition Target", 1024, 1024);
            device = new Device(this);
            hitTestWindow = new HitTestWindow(8, 8, this, device.RequestHitTest);
            device.OnStartCopy += () => hitTestWindow.blockUpdate = true;
            device.OnCopiedBitmap += hitTestWindow.OnMapped;

            ready = true;
        }

        bool isMouseDown;
        Point cursorPos;

        private IntPtr ProcessWindowMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            if (!ready) return User32.DefWindowProc(hWnd, msg, wParam, lParam);

            if (hWnd == hitTestWindow?.Handle)
            {
                switch ((WindowMessage)msg)
                {
                    case WindowMessage.Destroy:
                        User32.PostQuitMessage(0);
                        return IntPtr.Zero;
                    case WindowMessage.MouseMove:
                        if (isMouseDown)
                        {
                            User32.GetCursorPos(out Point point);
                            mainWindow.Left += point.X - cursorPos.X;
                            mainWindow.Top += point.Y - cursorPos.Y;
                            cursorPos = point;
                            mainWindow.MoveWindow();
                        }
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
                case WindowMessage.NcLButtonDown:
                case WindowMessage.RButtonDown:
                    isMouseDown = true;
                    User32.GetCursorPos(out cursorPos);
                    User32.SetCapture(hitTestWindow!.Handle); //一般窗口出来用户点的时候这个已经初始化了吧
                    break;
                case WindowMessage.RButtonUp:
                case WindowMessage.NcRButtonUp:
                    isMouseDown = false;
                    User32.ReleaseCapture();
                    break;
                case WindowMessage.LButtonDown:
                    OnMouseLeftDown?.Invoke(GetVirtualPos());
                    break;
                case WindowMessage.LButtonUp:
                    OnMouseLeftUp?.Invoke(GetVirtualPos());
                    break;
                case WindowMessage.Move:
                    mainWindow.Rect = new Rectangle(GetPointByLParam(lParam), new Size(mainWindow.Width, mainWindow.Height));
                    break;
            }
            return User32.DefWindowProc(hWnd, msg, wParam, lParam);
        }

        private Point GetVirtualPos()
        {
            User32.GetCursorPos(out Point point);
            return new Point(point.X - mainWindow.Left, point.Y - mainWindow.Top);
        }

        private Point GetPointByLParam(IntPtr lParam)
        {
            return new Point((short)((int)lParam.ToInt64() & 0xffff), (short)((int)lParam.ToInt64() >> 16));
        }

        public void Run()
        {
            Message msg = new Message();
            while (msg.Value != (uint)WindowMessage.Quit)
            {
                User32.GetMessage(out msg, IntPtr.Zero, 0, 0);
                User32.TranslateMessage(ref msg);
                User32.DispatchMessage(ref msg);
            }
        }
    }
}
