using System;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using System.Threading;
using Vortice.Direct2D1;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;
using static Vortice.Direct2D1.D2D1;
using static Vortice.Direct3D11.D3D11;
using static Vortice.DXGI.DXGI;

namespace HuiDesktop.DirectComposition.Interop
{
    internal class Device
    {
        private IDXGIFactory2 dxgi_factory;
        private ID2D1Factory2 d2d_factory;
        private ID3D11Device d3d_device;
        private IDXGIDevice dxgi_device;
        private ID2D1Device1 d2d_device;
        private ID2D1DeviceContext d2d_dc;
        private ID3D11DeviceContext d3d_dc;
        private ID2D1RenderTarget d2d_renderTarget;
        private IDXGISurface2 dxgi_surface;
        private IDXGISwapChain1 dxgi_swapChain;
        private IDXGISurface1 hitTestSurface;
        private DCompositionDevice dc_device;
        private DCompositionTarget dc_target;
        private DCompositionVisual dc_visual;
        private ManagedApplication application;
        private Thread renderThread;
        private AutoResetEvent resumeRenderThreadEvent = new AutoResetEvent(true);
        private bool renderToMainWindow;
        private bool copyBitmap;
        private Layer layer;
        private SwapChain swapChain;

        public Rectangle HitTestRect { get; set; }
        public event Action<int, IntPtr>? OnCopiedBitmap;
        public event Action? OnStartCopy;

        public Size newSize;
        public bool needResize;

        public Device(ManagedApplication application)
        {
            this.application = application;
            #region <D3D, D2D, DXGI> Init factories and devices
            CreateDXGIFactory2(false, out dxgi_factory).CheckError();
            D2D1CreateFactory(out d2d_factory).CheckError();
            D3D11CreateDevice(
                IntPtr.Zero,
                DriverType.Hardware,
                DeviceCreationFlags.BgraSupport,
                new Vortice.Direct3D.FeatureLevel[]
                {
                    Vortice.Direct3D.FeatureLevel.Level_11_0,
                    Vortice.Direct3D.FeatureLevel.Level_11_1
                },
                out d3d_device,
                out d3d_dc).CheckError();

            dxgi_device = d3d_device.QueryInterface<IDXGIDevice>();
            d2d_factory.CreateDevice(dxgi_device, out d2d_device);
            d2d_dc = d2d_device.CreateDeviceContext(DeviceContextOptions.EnableMultithreadedOptimizations);//optimize
            #endregion

            #region <DXGI> Create swap chain and surface(s) //TODO: add single point surface
            SwapChainDescription1 description = new SwapChainDescription1
            {
                Format = Format.B8G8R8A8_UNorm,
                Usage = Vortice.DXGI.Usage.RenderTargetOutput,
                SwapEffect = SwapEffect.FlipSequential,
                BufferCount = 2,
                SampleDescription = new SampleDescription { Count = 1 },
                AlphaMode = Vortice.DXGI.AlphaMode.Premultiplied,
                Width = application.mainWindow.Width,
                Height = application.mainWindow.Height
            };

            var pixelFormat = new PixelFormat
            {
                AlphaMode = Vortice.Direct2D1.AlphaMode.Premultiplied,
                Format = Format.B8G8R8A8_UNorm
            };

            dxgi_swapChain = dxgi_factory.CreateSwapChainForComposition(dxgi_device, description);
            dxgi_surface = dxgi_swapChain.GetBuffer<IDXGISurface2>(0);

            hitTestSurface = d3d_device.CreateTexture2D(new Texture2DDescription
            {
                Width = Config.copyRectSize,
                Height = Config.copyRectSize,
                Format = Format.B8G8R8A8_UNorm,
                MipLevels = 1,
                ArraySize = 1,
                SampleDescription = new SampleDescription
                {
                    Count = 1,
                    Quality = 0
                },
                Usage = Vortice.Direct3D11.Usage.Staging,
                CpuAccessFlags = CpuAccessFlags.Read,
                BindFlags = BindFlags.None,
                OptionFlags = ResourceOptionFlags.None
            }).QueryInterface<IDXGISurface1>();
            #endregion
            //float dpiX, dpiY;
            //d2d_factory.GetDesktopDpi(out dpiX, out dpiY);
            //d2d_renderTarget = d2d_factory.CreateDxgiSurfaceRenderTarget(dxgi_surface, new RenderTargetProperties
            //{
            //    Type = RenderTargetType.Hardware,
            //    PixelFormat = pixelFormat,
            //    DpiX = dpiX,
            //    DpiY = dpiY,
            //    Usage = RenderTargetUsage.None,
            //    MinLevel = Vortice.Direct2D1.FeatureLevel.Level_10
            //});

            renderThread = new Thread(Render);
            renderThread.IsBackground = true;
            renderThread.Start();

            #region Direct composition
            dc_device = new DCompositionDevice(dxgi_device);
            dc_target = dc_device.CreateTargetForHwnd(application.mainWindow.Handle, true);
            dc_visual = dc_device.CreateVisual();
            dc_visual.SetContent(dxgi_swapChain).CheckError();
            dc_target.SetRoot(dc_visual).CheckError();
            dc_device.Commit().CheckError();
            #endregion

            swapChain = new SwapChain(d3d_dc, d3d_device, d3d_device.CreateRenderTargetView(dxgi_surface.QueryInterface<ID3D11Resource>()), application.mainWindow.Width, application.mainWindow.Height);
            layer = new Layer(d3d_device);
            layer.Move(new Rectangle(0, 0, application.mainWindow.Width, application.mainWindow.Height));
            d3d_dc.Flush();
        }

        internal void RequestHitTest(Rectangle obj)
        {
            HitTestRect = obj;
            copyBitmap = true;
            resumeRenderThreadEvent.Set();
        }

        Texture2D texture2;

        private void Render()
        {
            while (true)
            {
                resumeRenderThreadEvent.WaitOne();
                if (renderToMainWindow)
                {
                    lock (texture2)
                        using (new Binder(d3d_dc, swapChain))
                        {
                            if (needResize)
                            {
                                needResize = false;
                                Resize(newSize.Width, newSize.Height);
                            }
                            layer.RenderTexture(d3d_dc, texture2);
                        }
                    dxgi_swapChain.Present(0, 0).CheckError();
                    renderToMainWindow = false;
                }
                if (copyBitmap)
                {
                    OnStartCopy?.Invoke();

                    d3d_dc.CopySubresourceRegion(
                        hitTestSurface.QueryInterface<ID3D11Resource>(), 0, 0, 0, 0,
                        dxgi_surface.QueryInterface<ID3D11Resource>(), 0,
                        new Vortice.Mathematics.Box(HitTestRect.X, HitTestRect.Y, 0,
                                                    HitTestRect.X + HitTestRect.Width,
                                                    HitTestRect.Y + HitTestRect.Height, 1));

                    var mappedRect = hitTestSurface.Map(Vortice.DXGI.MapFlags.Read);
                    OnCopiedBitmap?.Invoke(mappedRect.Pitch, mappedRect.PBits);
                    hitTestSurface.Unmap();

                    copyBitmap = false;
                }
                Thread.Sleep(Config.renderSleep);
            }
        }

        private void Resize(int width, int height)
        {
            swapChain.Resize(d3d_dc, dxgi_swapChain, width, height, ref dxgi_surface);
        }

        public void RequestRenderSharedTexture(Rectangle rect, IntPtr sharedTexture)
        {
            if (texture2 == null || texture2.sharedHandle != sharedTexture)
            {
                texture2 = new Texture2D(d3d_device, sharedTexture);
            }
            renderToMainWindow = true;
            resumeRenderThreadEvent.Set();
        }
    }
}
