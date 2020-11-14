using HuiDesktop.DirectComposition.DirectX;
using HuiDesktop.DirectComposition.Interop;
using System;
using System.Drawing;
using System.Threading;
using Vortice.Direct3D11;

namespace HuiDesktop.DirectComposition
{
    public class MainWindow
    {
        Device device;
        SwapChain swapChain;
        bool copyToHitTestEvent = false;
        bool minimized = false;
        bool ignoreClick = false;
        Rectangle hitTestRect;
        public HitTestWindow hitTestWindow;

        #region In one layer
        Geometry? geometry;
        Effect? effect;
        Texture2D? texture;
        SpinLock textureLock = new SpinLock();
        #endregion

        bool resized = false;
        private int width;
        private int height;
        private int left;
        private int top;
        private bool topmost = false;

        public IntPtr Handle { get; }
        public string Title { get; }
        public bool Minimized { get => minimized; set => minimized = value; }
        public int Left
        {
            get => left;
            set
            {
                if (value != left)
                {
                    left = value;
                    LocationChanged();
                }
            }
        }
        public int Top
        {
            get => top;
            set
            {
                if (value != top)
                {
                    top = value;
                    LocationChanged();
                }
            }
        }
        public int Width
        {
            get => width;
            set
            {
                if (value != width)
                {
                    width = value;
                    SizeChanged();
                }
            }
        }
        public int Height
        {
            get => height;
            set
            {
                if (value != height)
                {
                    height = value;
                    SizeChanged();
                }
            }
        }
        public Rectangle Rect
        {
            get => new Rectangle(Left, Top, Width, Height);
            set
            {
                if (left != value.X || top != value.Y)
                {
                    Left = value.X;
                    Top = value.Y;
                    LocationChanged();
                }
                if (width != value.Width || height != value.Height)
                {
                    width = value.Width;
                    height = value.Height;
                    SizeChanged();
                }
            }
        }
        public Point Location
        {
            get => new(Left, Top);
            set
            {
                if (left != value.X || top != value.Y)
                {
                    Left = value.X;
                    Top = value.Y;
                    LocationChanged();
                }
            }
        }
        public Size Size
        {
            get => new(Width, Height);
            set
            {
                if (width != value.Width || height != value.Height)
                {
                    width = value.Width;
                    height = value.Height;
                    SizeChanged();
                }
            }
        }

        public Action LocationChanged { get; set; } = () => { };//TODO
        public Action SizeChanged { get; set; } = () => { };
        public bool Topmost
        {
            get => topmost;
            set
            {
                if (value == topmost)
                {
                    return;
                }
                topmost = value;
                User32.SetWindowPos(Handle, topmost ? new IntPtr(-1) : new(-2), Left, Top, Width, Height, 0);
            }
        }

        public bool ShowInTaskbar { get; set; }//TODO
        public bool IgnoreClick { get; set; }
        public Action<MouseButton, Point> MouseDown { get; set; }
        public Action<MouseButton, Point> MouseUp { get; set; }
        public Action<Point> MouseMove { get; internal set; }

        public MainWindow(int width, int height, ID3D11DeviceContext ctx, Device device)
        {
            this.device = device;

            #region Win32
            Title = "HuiDesktop";
            Width = width;
            Height = height;
            Left = 0;
            Top = 0;

            WindowStyles style = WindowStyles.WS_POPUP;
            WindowExStyles styleEx = WindowExStyles.WS_EX_NOREDIRECTIONBITMAP;
            Handle = User32.CreateWindowEx((int)styleEx, ManagedApplication.WndClassName, Title, (int)style, Left, Top,
                                             width, height, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);

            if (Handle == IntPtr.Zero)
            {
                DebugHelper.CheckWin32Error();
                throw new Exception("Unreachable!");
            }

            User32.ShowWindow(Handle, ShowWindowCommand.Normal);

            const int GWL_EXSTYLE = -20;
            uint value = User32.GetWindowLong(Handle, GWL_EXSTYLE);
            value |= (uint)(WindowExStyles.WS_EX_TRANSPARENT | WindowExStyles.WS_EX_LAYERED);
            User32.SetWindowLong(Handle, GWL_EXSTYLE, value);
            #endregion

            swapChain = device.CreateSwapChain(ctx, width, height);
            device.InitDC(Handle, swapChain);
            hitTestWindow = new(8, 8, this, (rect) => { this.hitTestRect = rect; copyToHitTestEvent = true; }, device.nativeDevice);

            SizeChanged += () => { resized = true; MoveWindow(); };
            LocationChanged += () => MoveWindow();
        }

        internal void ReleaseCapture()
        {
            User32.ReleaseCapture();
            hitTestWindow.captured = false;
        }

        internal void CaptureMouse()
        {
            User32.SetCapture(hitTestWindow.Handle);
            hitTestWindow.captured = true;
        }

        public void UpdateFrame(IntPtr sharedHandler)
        {
            {
                bool locked = false;
                do textureLock.Enter(ref locked); while (!locked);
            }
            if (texture?.SharedHandler != sharedHandler)
            {
                texture = device.OpenSharedResource(sharedHandler);
            }
            textureLock.Exit();
        }

        public void Render(ID3D11DeviceContext ctx)
        {
            if (texture == null)
            {
                return;
            }
            using (new Binder(swapChain, ctx))
            {
                if (resized)
                {
                    swapChain.Resize(ctx, Size);
                    resized = false;
                }

                swapChain.Clear(ctx);

                //I only want to write only one layer!
                if (geometry == null)
                {
                    geometry = device.CreateQuad(0, 0, Width, Height, true);
                }
                if (effect == null)
                {
                    effect = device.CreateDefaultEffect();
                }

                {
                    bool locked = false;
                    do textureLock.Enter(ref locked); while (!locked);
                }
                using (new Binder(geometry, ctx))
                using (new Binder(effect, ctx))
                using (new Binder(texture, ctx))
                {
                    geometry.Draw(ctx);
                }
                textureLock.Exit();


                swapChain.Present(1);

                if (copyToHitTestEvent && new Rectangle(new(), Size).Contains(hitTestRect))
                {
                    //TODO: Known bug: 修改Size以后这里可能会出错(鼠标在老大小（小）和新大小（大）之间)估计是BackBuffer是老大小的原因
                    //TODO: 感觉只有鼠标在窗口内的时候动画才是全速的，不知为啥
                    copyToHitTestEvent = false;
                    swapChain.CopyRegion(ctx, hitTestWindow.texture.QueryInterface<ID3D11Resource>(), hitTestRect);
                    var mapped = hitTestWindow.texture.Map(Vortice.DXGI.MapFlags.Read);
                    hitTestWindow.OnMapped(mapped.Pitch, mapped.PBits, hitTestRect);
                }
            }
        }

        internal void MoveWindow()
        {
            User32.MoveWindow(Handle, Left, Top, Width, Height, false);
        }
    }
}
