using HuiDesktop.DirectComposition.DirectX;
using HuiDesktop.DirectComposition.Interop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Vortice.Direct3D11;

namespace HuiDesktop.DirectComposition
{
    public class MainWindow
    {
        Device device;
        SwapChain swapChain;

        #region In a layer
        Geometry? geometry;
        Effect? effect;
        Texture2D? texture;
        SpinLock textureLock = new SpinLock();
        #endregion

        public IntPtr Handle { get; }
        public int Left { get; set; }
        public int Top { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string Title { get; }
        public Rectangle Rect
        {
            get => new Rectangle(Left, Top, Width, Height);
            set
            {
                Left = value.X;
                Top = value.Y;
                Width = value.Width;
                Height = value.Height;
            }
        }

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
            WindowExStyles styleEx = WindowExStyles.WS_EX_NOREDIRECTIONBITMAP | WindowExStyles.WS_EX_TOPMOST;
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
            Debug.Assert(swapChain != null);
            device.InitDC(Handle, swapChain);
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
                /*
                TODO: resize
                if (resize_)
                {
                    RECT rc;
                    GetClientRect(hwnd(), &rc);
                    auto const width = rc.right - rc.left;
                    auto const height = rc.bottom - rc.top;
                    if (width && height)
                    {
                        resize_ = false;
                        composition_->resize(sync_interval_ != 0, width, height);
                        swapchain_->resize(width, height);
                    }
                }
                }*/
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


                swapChain.Present(0);
            }
        }

        internal void MoveWindow()
        {
            User32.MoveWindow(Handle, Left, Top, Width, Height, false);
        }
    }
}
