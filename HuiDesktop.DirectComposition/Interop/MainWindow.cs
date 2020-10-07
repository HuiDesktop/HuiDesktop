using System;
using System.Drawing;

namespace HuiDesktop.DirectComposition.Interop
{
    public class MainWindow
    {
        public string Title { get; private set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int Left { get; set; }
        public int Top { get; set; }
        public Rectangle Rect
        {
            get => new Rectangle { X = Left, Y = Top, Width = Width, Height = Height };
            set { Width = value.Width; Height = value.Height; Left = value.X; Top = value.Y; }
        }
        public IntPtr Handle { get; private set; }
        public const int GWL_EXSTYLE = -20;

        public MainWindow(string title, int width, int height)
        {
            Title = title;
            Width = width;
            Height = height;
            Left = 0;
            Top = 0;

            WindowStyles style = WindowStyles.WS_POPUP;
            WindowExStyles styleEx = WindowExStyles.WS_EX_NOREDIRECTIONBITMAP | WindowExStyles.WS_EX_TOPMOST;
            Handle = User32.CreateWindowEx((int)styleEx, ManagedApplication.WndClassName, Title, (int)style, Left, Top,
                                             width, height, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);

            if (Handle == IntPtr.Zero)
            {
                DebugHelper.CheckWin32Error();
                throw new Exception("Unreachable!");
            }

            User32.ShowWindow(Handle, ShowWindowCommand.Normal);
            User32.SetWindowLong(Handle, GWL_EXSTYLE, User32.GetWindowLong(Handle, GWL_EXSTYLE) | (uint)(WindowExStyles.WS_EX_TRANSPARENT | WindowExStyles.WS_EX_LAYERED));
        }

        internal void MoveWindow()
        {
            User32.MoveWindow(Handle, Left, Top, Width, Height, false);
        }
    }
}
