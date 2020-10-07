using HuiDesktop.DirectComposition.Interop;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HuiDesktop.DirectComposition
{
    public class HitTestWindow
    {
        public int Width { get; }
        public int Height { get; }
        public IntPtr Handle { get; private set; }
        public ManagedApplication ParentApplication { get; }
        public Point Position => position;
        // 阻止鼠标捕获线程发送拷贝指令
        // 由渲染进程在渲染指令发送给GPU前设置，由本窗口UpdateLayeredWindow以后复位
        // 本标志保证OnMapped使用scan0时不会突然暴毙
        public bool blockUpdate;

        private IntPtr screenDC, memDC;
        private Thread trackMouseThread;
        // 窗口应该移动到的位置，由鼠标捕获线程写入，UpdateLayeredWindow读取
        private Point position;
        // 在UpdateLayeredWindow使用，与窗口大小相同
        private Size bitmapSize;
        // 发送拷贝指令
        private readonly Action<Rectangle> requestCopyBitmap;
        public HitTestWindow(int width, int height, ManagedApplication parentApplication, Action<Rectangle> requestCopyBitmap)
        {
            Width = width;
            Height = height;
            ParentApplication = parentApplication;
            bitmapSize = new Size(Width, Height);
            this.requestCopyBitmap = requestCopyBitmap;

            CreateWindow();
            InitGdi();
            trackMouseThread = new Thread(MonUpdate);
            trackMouseThread.IsBackground = true;
            trackMouseThread.Start();
        }

        private void CreateWindow()
        {
            var x = 0;
            var y = 0;

            Handle = User32.CreateWindowEx(
                (int)(WindowExStyles.WS_EX_LAYERED | WindowExStyles.WS_EX_TOPMOST),
                ManagedApplication.WndClassName,
                "HitTest",
                (int)WindowStyles.WS_POPUP,
                x, y, Width, Height,
                IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);

            if (Handle == IntPtr.Zero)
            {
                throw new NullReferenceException($"{Marshal.GetLastWin32Error()}");
            }

            User32.ShowWindow(Handle, ShowWindowCommand.Normal);
            //User32.SetLayeredWindowAttributes(Handle, 0, 255, 0x00000002/*LWA_ALPHA*/);
        }

        private void InitGdi()
        {
            screenDC = User32.GetDC(Handle);
            memDC = Gdi32.CreateCompatibleDC(screenDC);
        }

        // 鼠标捕获线程
        // 每次while以后Sleep(1)保证CPU占用
        private void MonUpdate()
        {
            Point cursorPoint;
            while (true)
            {
                if (!blockUpdate)
                {
                    User32.GetCursorPos(out cursorPoint);
                    if (ParentApplication.mainWindow.Rect.Contains(cursorPoint))
                    {
                        var rect = new Rectangle(cursorPoint.X - (Width / 2) - ParentApplication.mainWindow.Rect.X,
                                                 cursorPoint.Y - (Height / 2) - ParentApplication.mainWindow.Rect.Y,
                                                 Width, Height);
                        if (rect.X < 0) rect.X = 0;
                        else if (rect.X + rect.Width > ParentApplication.mainWindow.Width) rect.X = ParentApplication.mainWindow.Width - rect.Width;
                        if (rect.Y < 0) rect.Y = 0;
                        else if (rect.Y + rect.Height > ParentApplication.mainWindow.Height) rect.Y = ParentApplication.mainWindow.Height - rect.Height;
                        position = new Point(ParentApplication.mainWindow.Left + rect.X, ParentApplication.mainWindow.Top + rect.Y);
                        requestCopyBitmap(rect);
                    }
                }
                Thread.Sleep(1);
            }
        }

        private static Point zeroPoint = new Point(0, 0);
        private const byte AC_SRC_OVER = 0x00;
        private const byte AC_SRC_ALPHA = 0x01;
        private static User32.BLENDFUNCTION blendFunc = new User32.BLENDFUNCTION
        {
            BlendOp = AC_SRC_OVER,
            SourceConstantAlpha = 1,
            AlphaFormat = AC_SRC_ALPHA,
            BlendFlags = 0
        };

        // 渲染线程在完成拷贝以后的回调
        // 由blockUpdate保证中途scan0对应内容不会变化
        public void OnMapped(int pitch, IntPtr scan0)
        {
            using (var managedBitmap = new Bitmap(Width, Height, pitch, System.Drawing.Imaging.PixelFormat.Format32bppArgb, scan0))
            {
                var bitmap = managedBitmap.GetHbitmap();
                var old = Gdi32.SelectObject(memDC, bitmap);
                if (old == IntPtr.Zero)
                {
                    DebugHelper.CheckWin32Error();
                }

                if (!User32.UpdateLayeredWindow(Handle, screenDC, ref position, ref bitmapSize, memDC, ref zeroPoint, 0,
                    ref blendFunc, (int)User32.UpdateLayeredWindowFlags.Alpha))
                {
                    DebugHelper.CheckWin32Error();
                }

                Gdi32.SelectObject(memDC, old);
                Gdi32.DeleteObject(bitmap);
            }

            blockUpdate = false;
        }
    }
}
