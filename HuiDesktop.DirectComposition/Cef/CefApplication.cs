using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CefSharp;
using CefSharp.OffScreen;
using CefSharp.Structs;
using HuiDesktop.DirectComposition.Cef;

namespace HuiDesktop.DirectComposition
{
    public class CefApplication
    {
        private ManagedApplication application;
        private ChromiumWebBrowser browser;
        Thread cefThread, renderThread;

        public CefApplication()
        {
            application = new ManagedApplication();
            browser = new ChromiumWebBrowser(string.Empty, null, null, false);
            browser.RenderHandler = new DirectCompositionRenderHandler(application.mainWindow.UpdateFrame, application.mainWindow);

            var info = new WindowInfo();
            info.SetAsWindowless(IntPtr.Zero);
            info.WindowlessRenderingEnabled = true;
            info.SharedTextureEnabled = true;
            info.ExternalBeginFrameEnabled = true;

            browser.CreateBrowser(info);
            cefThread = new Thread((ThreadStart)delegate
            {
                while (true) { CefSharp.Cef.DoMessageLoopWork(); Thread.Sleep(1); }
            });
            renderThread = new Thread((ThreadStart)delegate
            {
                var host = browser.GetBrowserHost() ?? throw new NullReferenceException();
                while (true)
                {
                    host.SendExternalBeginFrame();
                    Thread.Sleep(16);
                }
            });
            cefThread.Start();
            browser.BrowserInitialized += (_, __) => { browser.Load("https://webglsamples.org/aquarium/aquarium.html"); renderThread.Start(); };
            application.OnMouseLeftDown += OnLeftDown;
            application.OnMouseLeftUp += OnLeftUp;
            //Task.Factory.StartNew(() =>
            //{
            //    Thread.Sleep(5000);
            //    application.mainWindow.Size = new(512, 512);
            //    browser.GetBrowserHost().WasResized();
            //});
        }

        public void OnLeftDown(System.Drawing.Point point)
        {
            browser.GetBrowserHost()?.SendMouseClickEvent(new MouseEvent(point.X, point.Y, CefEventFlags.None), MouseButtonType.Left, false, 1);
            Debug.WriteLine(point);
        }

        public void OnLeftUp(System.Drawing.Point point)
        {
            browser.GetBrowserHost()?.SendMouseClickEvent(new MouseEvent(point.X, point.Y, CefEventFlags.None), MouseButtonType.Left, true, 1);
        }

        public void Run()
        {
            application.Run();
            CefSharp.Cef.Shutdown();
            cefThread.Abort();
        }
    }
}
