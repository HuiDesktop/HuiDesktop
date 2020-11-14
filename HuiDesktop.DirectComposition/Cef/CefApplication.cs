using CefSharp;
using CefSharp.OffScreen;
using HuiDesktop.DirectComposition.Cef;
using System;
using System.Threading;
using System.Windows.Forms;

namespace HuiDesktop.DirectComposition
{
    public class CefApplication
    {
        private ManagedApplication application;
        private ChromiumWebBrowser browser;
        private JsApi api;
        public NotifyIcon notifyIcon = new();
        Thread cefThread, renderThread;

        public CefApplication(string address = "https://webglsamples.org/aquarium/aquarium.html", RequestHandler requestHandler = null)
        {
            CefInitialize.InitializeCefSharp();
            application = new ManagedApplication();
            application.mainWindow.MouseMove += point =>
            {
                if (browser?.IsBrowserInitialized == true)
                {
                    browser.GetBrowserHost().SendMouseMoveEvent(new(point.X, point.Y, CefEventFlags.None), false);
                }
            };
            application.mainWindow.MouseDown += (button, point) =>
            {
                if (button == MouseButton.Left) OnLeftDown(point);
                else OnRightDown(point);
            };
            application.mainWindow.MouseUp += (button, point) =>
            {
                if (button == MouseButton.Left) OnLeftUp(point);
                else OnRightUp(point);
            };


            browser = new ChromiumWebBrowser(string.Empty, null, null, false);
            browser.RequestHandler = requestHandler;
            browser.MenuHandler = new NullMenuHandler();
            browser.RenderHandler = new DirectCompositionRenderHandler(application.mainWindow.UpdateFrame, application.mainWindow);
            api = new(application.mainWindow, browser);
            browser.JavascriptObjectRepository.Register(name: "huiDesktop",
                                                        objectToBind: api,
                                                        isAsync: false,
                                                        options: new BindingOptions { CamelCaseJavascriptNames = false });

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
            application.OnMouseLeftDown += OnLeftDown;
            application.OnMouseLeftUp += OnLeftUp;
            browser.BrowserInitialized += (_, __) =>
            {
                renderThread.Start();
                browser.Load(address);
                browser.ShowDevTools();
            };

            notifyIcon.Text = "HuiDesktop";
            notifyIcon.Icon = Properties.Resources.GlobalIcon;
            notifyIcon.Visible = true;
            notifyIcon.MouseClick += NotifyIcon_MouseClick;
            notifyIcon.ContextMenu = new ContextMenu();
            notifyIcon.ContextMenu.MenuItems.Add(new MenuItem("DevTools", ContextMenu_DevTools));
            notifyIcon.ContextMenu.MenuItems.Add(new MenuItem("刷新", ContextMenu_Reload));
            notifyIcon.ContextMenu.MenuItems.Add(new MenuItem("设置", ContextMenu_Settings));
            notifyIcon.ContextMenu.MenuItems.Add(new MenuItem("退出", ContextMenu_Exit));
        }

        private void NotifyIcon_MouseClick(object sender, MouseEventArgs e)
        {
            //TODO
        }

        private void ContextMenu_DevTools(object sender, EventArgs e)
        {
            browser.ShowDevTools();
        }

        private void ContextMenu_Reload(object sender, EventArgs e)
        {
            browser.GetBrowser().Reload();
        }

        private void ContextMenu_Settings(object sender, EventArgs e)
        {
            browser.GetBrowser().MainFrame.ExecuteJavaScriptAsync("requestSettings()", browser.Address);
        }

        private void ContextMenu_Exit(object sender, EventArgs e)
        {
            Environment.Exit(0);
            //TODO
        }

        public void OnLeftDown(System.Drawing.Point point)
        {
            browser.GetBrowserHost()?.SendMouseClickEvent(new MouseEvent(point.X, point.Y, CefEventFlags.None), MouseButtonType.Left, false, 1);
        }

        public void OnRightDown(System.Drawing.Point point)
        {
            browser.GetBrowserHost()?.SendMouseClickEvent(new MouseEvent(point.X, point.Y, CefEventFlags.None), MouseButtonType.Right, false, 1);
        }

        public void OnLeftUp(System.Drawing.Point point)
        {
            browser.GetBrowserHost()?.SendMouseClickEvent(new MouseEvent(point.X, point.Y, CefEventFlags.None), MouseButtonType.Left, true, 1);
        }

        public void OnRightUp(System.Drawing.Point point)
        {
            browser.GetBrowserHost()?.SendMouseClickEvent(new MouseEvent(point.X, point.Y, CefEventFlags.None), MouseButtonType.Right, true, 1);
        }

        public void Run()
        {
            application.Run();
            CefSharp.Cef.Shutdown();
            cefThread.Abort();
        }
    }

    static class CefInitialize
    {
        public static void InitializeCefSharp()
        {
            CefSharpSettings.LegacyJavascriptBindingEnabled = true;
            CefSharpSettings.WcfEnabled = true;

            var settings = new CefSettings
            {
                BrowserSubprocessPath = System.IO.Path.Combine(ApplicationInfo.CefSharpFolder, "CefSharp.BrowserSubprocess.exe"),
                CachePath = ApplicationInfo.RelativePath("BrowserStorage", "Cache"),
                UserDataPath = ApplicationInfo.RelativePath("BrowserStorage", "UserData"),
                LogFile = ApplicationInfo.RelativePath("Debug.log"),
                AcceptLanguageList = "zh-CN,en-US,en"
            };
            if (GlobalSettings.DisableBlackList)
            {
                settings.CefCommandLineArgs.Add("enable-webgl", "1");
                settings.CefCommandLineArgs.Add("ignore-gpu-blacklist", "1");
            }
            CefSharp.Cef.Initialize(settings);
        }
    }

    class NullMenuHandler : IContextMenuHandler
    {
        public bool RunContextMenu(IWebBrowser browserControl, IBrowser browser, IFrame frame, IContextMenuParams parameters, IMenuModel model, IRunContextMenuCallback callback)
        {
            return false;
        }
        public void OnBeforeContextMenu(IWebBrowser browserControl, IBrowser browser, IFrame frame, IContextMenuParams parameters, IMenuModel model)
        {
            model.Clear();
        }

        public void OnContextMenuDismissed(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame)
        {
        }

        public bool OnContextMenuCommand(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame,
                                         IContextMenuParams parameters,
                                         CefMenuCommand commandId, CefEventFlags eventFlags)
        {
            throw new NotImplementedException();
        }
    }
}
