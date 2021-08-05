using CefSharp;
using CefSharp.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace HuiDesktop
{
    /// <summary>
    /// BasicWindow.xaml 的交互逻辑
    /// </summary>
    public partial class BasicWindow : Window
    {
        public ChromiumWebBrowser browser;
        public NotifyIcon notifyIcon = new NotifyIcon();
        private ShowDevToolsLifeSpanHandler showDevToolsLifeSpanHandler = new ShowDevToolsLifeSpanHandler();
        private NextGenJsApi api;

        public BasicWindow(Package.StartupInfo info)
        {
            var requestHandler = new RequestHandler();
            requestHandler.AddPackage(info.fromPackage);
            foreach (var i in info.dependencies) requestHandler.AddPackage(Package.PackageManager.packages[i]);
            Startup(requestHandler, info.url, GlobalSettings.DisableBlackList);
        }

        public BasicWindow(IRequestHandler requestHandler, string url, bool disableBlackList)
        {
            Startup(requestHandler, url, disableBlackList);
        }

        public void Startup(IRequestHandler requestHandler, string url, bool disableBlackList)
        {
            CefInitialize.InitializeCefSharp(disableBlackList);
            InitializeComponent();

            Top = 0;
            Left = 0;
            Height = SystemParameters.WorkArea.Height;
            Width = SystemParameters.WorkArea.Width;

            notifyIcon.Text = "HuiDesktop";
            notifyIcon.Icon = Properties.Resources.GlobalIcon;
            notifyIcon.Visible = true;
            notifyIcon.MouseClick += NotifyIcon_MouseClick;
            notifyIcon.ContextMenu = new System.Windows.Forms.ContextMenu();
            notifyIcon.ContextMenu.MenuItems.Add(new System.Windows.Forms.MenuItem("DevTools", ContextMenu_DevTools));
            notifyIcon.ContextMenu.MenuItems.Add(new System.Windows.Forms.MenuItem("刷新", ContextMenu_Reload));
            notifyIcon.ContextMenu.MenuItems.Add(new System.Windows.Forms.MenuItem("设置", ContextMenu_Settings));
            notifyIcon.ContextMenu.MenuItems.Add(new System.Windows.Forms.MenuItem("调试设置开关", ContextMenu_ChangeConfigDevTools));
            notifyIcon.ContextMenu.MenuItems.Add(new System.Windows.Forms.MenuItem("退出", ContextMenu_Exit));

            browser = new ChromiumWebBrowser();
            browser.BrowserSettings = new BrowserSettings { WindowlessFrameRate = 60, LocalStorage = CefState.Enabled };
            AddChild(browser);

            api = new NextGenJsApi(this);
            browser.RequestHandler = requestHandler;
            browser.MenuHandler = new NullMenuHandler();
            browser.JavascriptObjectRepository.ResolveObject += (sender, e) =>
            {
                var repo = e.ObjectRepository;
                if (e.ObjectName == "_huiDesktopIpcBridge")
                {
                    repo.NameConverter = new CefSharp.JavascriptBinding.CamelCaseJavascriptNameConverter();
                    repo.Register("_huiDesktopIpcBridge", api, isAsync: true);
                }
            };

            browser.LifeSpanHandler = showDevToolsLifeSpanHandler;
            browser.Address = url;
        }

        private void NotifyIcon_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            WindowState = WindowState.Normal;
            Activate();
        }

        private void ContextMenu_DevTools(object sender, EventArgs e)
        {
            browser.ShowDevTools();
        }

        private void ContextMenu_ChangeConfigDevTools(object sender, EventArgs e)
        {
            showDevToolsLifeSpanHandler.showDevTools ^= true;
        }

        private void ContextMenu_Reload(object sender, EventArgs e)
        {
            browser.GetBrowser().Reload();
        }

        private void ContextMenu_Settings(object sender, EventArgs e)
        {
            api.InvokeSettings();
        }

        private void ContextMenu_Exit(object sender, EventArgs e)
        {
            Close();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (Topmost == true && ShowInTaskbar == false && WindowState == WindowState.Minimized)
                WindowState = WindowState.Normal;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            notifyIcon.Visible = false;
            //Cef.Shutdown(); https://github.com/cefsharp/CefSharp/issues/820
            //Environment.Exit(0);
        }
    }

    class ShowDevToolsLifeSpanHandler : CefSharp.Handler.LifeSpanHandler
    {
        public volatile bool showDevTools;
        public volatile uint popupCount = 0;

        protected override void OnAfterCreated(IWebBrowser chromiumWebBrowser, IBrowser browser)
        {
            base.OnAfterCreated(chromiumWebBrowser, browser);
            if (popupCount > 0)
            {
                popupCount -= 1;
                if (showDevTools) browser?.ShowDevTools();
            }
        }

        protected override bool OnBeforePopup(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, string targetUrl, string targetFrameName, WindowOpenDisposition targetDisposition, bool userGesture, IPopupFeatures popupFeatures, IWindowInfo windowInfo, IBrowserSettings browserSettings, ref bool noJavascriptAccess, out IWebBrowser newBrowser)
        {
            popupCount += 1;
            return base.OnBeforePopup(chromiumWebBrowser, browser, frame, targetUrl, targetFrameName, targetDisposition, userGesture, popupFeatures, windowInfo, browserSettings, ref noJavascriptAccess, out newBrowser);
        }
    }

    static class CefInitialize
    {
        public static void InitializeCefSharp(bool disableBlackList)
        {
            var settings = new CefSettings
            {
                BrowserSubprocessPath = System.IO.Path.Combine(ApplicationInfo.CefSharpFolder, "CefSharp.BrowserSubprocess.exe"),
                CachePath = ApplicationInfo.RelativePath("BrowserStorage", "Cache"),
                UserDataPath = ApplicationInfo.RelativePath("BrowserStorage", "UserData"),
                LogFile = ApplicationInfo.RelativePath("Debug.log"),
                AcceptLanguageList = "zh-CN,en-US,en"
            };
            if (disableBlackList)
            {
                settings.CefCommandLineArgs.Add("enable-webgl", "1");
                settings.CefCommandLineArgs.Add("ignore-gpu-blacklist", "1");
            }
            CefSharp.Cef.Initialize(settings);
        }
    }

    class NextGenJsApi
    {
        private IJavascriptCallback positionChangedCallback, settingCallback;
        private BasicWindow window;
        private DragMoveManager dragMoveManager;

        public NextGenJsApi(BasicWindow parent)
        {
            window = parent;
            dragMoveManager = new DragMoveManager(parent, parent.browser);
            window.LocationChanged += Window_LocationChanged;
        }

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            if (positionChangedCallback?.CanExecute == true)
            {
                positionChangedCallback.ExecuteAsync(window.Left, window.Top);
            }
        }

        public void RegisterWindowPositionListener(IJavascriptCallback callback)
        {
            positionChangedCallback = callback;
        }

        public void SetWindowPosition(double x, double y)
        {
            window.Dispatcher.Invoke(() =>
            {
                window.Left = x;
                window.Top = y;
            });
        }

        public void SetWindowSize(double w, double h)
        {
            window.Dispatcher.Invoke(() =>
            {
                window.Width = w;
                window.Height = h;
            });
        }

        public class Position
        {
            public double X { get; set; }
            public double Y { get; set; }

            public Position(double x, double y)
            {
                X = x;
                Y = y;
            }
        }

        public class Size
        {
            public double Width { get; set; }
            public double Height { get; set; }

            public Size(double x, double y)
            {
                Width = x;
                Height = y;
            }
        }

        public class ScreenInfo
        {
            public double Width { get; set; }
            public double Height { get; set; }
            public double AvaliWidth { get; set; }
            public double AvaliHeight { get; set; }
            public double AvaliX { get; set; }
            public double AvaliY { get; set; }

            public ScreenInfo()
            {
                AvaliX = SystemParameters.WorkArea.Top;
                AvaliY = SystemParameters.WorkArea.Left;
                AvaliWidth = SystemParameters.WorkArea.Width;
                AvaliHeight = SystemParameters.WorkArea.Height;
                Width = SystemParameters.PrimaryScreenWidth;
                Height = SystemParameters.PrimaryScreenHeight;
            }
        }

        public Position GetWindowPosition()
        {
            double left = 0, top = 0;
            window.Dispatcher.Invoke(() => { left = window.Left; top = window.Top; });
            return new Position(left, top);
        }

        public Size GetWindowSize()
        {
            double w = 0, h = 0;
            window.Dispatcher.Invoke(() => { w = window.Width; h = window.Height; });
            return new Size(w, h);
        }

        public ScreenInfo GetScreenInfo()
        {
            return new ScreenInfo();
        }

        public void RegisterDragMoveListener(IJavascriptCallback callback)
        {
            dragMoveManager.dragMoveCallback = callback;
        }

        public void RegisterSetting(IJavascriptCallback callback)
        {
            settingCallback = callback;
        }

        public void InvokeSettings()
        {
            if (settingCallback != null && settingCallback.CanExecute)
            {
                settingCallback.ExecuteAsync();
            }
        }

        public void SetBooleanAttribute(string attr, bool value)
        {
            switch (attr)
            {
                case "TopMost":
                    window.Dispatcher.Invoke(() => window.Topmost = value);
                    break;
                case "ShowInTaskBar":
                    window.Dispatcher.Invoke(() => window.ShowInTaskbar = value);
                    break;
                case "ClickTransparent":
                    window.Dispatcher.Invoke(() =>
                    {
                        var handle = new System.Windows.Interop.WindowInteropHelper(window).Handle;
                        var t = Win32Api.GetWindowLong(handle, Win32Api.GWL_EXSTYLE);
                        if ((((t & Win32Api.WS_EX_TRANSPARENT) != 0) ^ value) == true)
                        {
                            t ^= Win32Api.WS_EX_TRANSPARENT;
                            Win32Api.SetWindowLong(handle, Win32Api.GWL_EXSTYLE, t);
                        }
                    });
                    break;
                case "DragMoveLeft":
                    dragMoveManager.canLeft = value;
                    break;
                case "DragMoveRight":
                    dragMoveManager.canRight = value;
                    break;
            }
        }

        private class DragMoveManager
        {
            Window window;
            Point point;
            MouseButton button;
            bool busy, joke;

            public bool canLeft, canRight;
            /*  dragMoveCallback(keyType, moveType)
             *  keyType: 0: left, 1: right
             *  moveType: 0: click, 1: down, 2: up
             */
            public IJavascriptCallback dragMoveCallback;

            public DragMoveManager(Window window, ChromiumWebBrowser browser)
            {
                this.window = window;
                browser.PreviewMouseDown += MouseDown;
                window.MouseMove += MouseMove;
                window.MouseUp += MouseUp;
                busy = false;
            }

            private void MouseUp(object sender, MouseButtonEventArgs e)
            {
                if (button != e.ChangedButton)
                {
                    e.Handled = false;
                    return;
                }
                e.Handled = true;
                if (!busy) return;
                busy = false;
                Mouse.Capture(null);
                if (dragMoveCallback == null || !dragMoveCallback.CanExecute) return;
                if (button == MouseButton.Left) dragMoveCallback.ExecuteAsync(0, joke ? 0 : 2);
                else if (button == MouseButton.Right) dragMoveCallback.ExecuteAsync(1, joke ? 0 : 2);
            }

            private void MouseDown(object sender, MouseButtonEventArgs e)
            {
                switch (e.ChangedButton)
                {
                    case MouseButton.Left:
                        if (!canLeft)
                        {
                            e.Handled = false;
                            return;
                        }
                        break;
                    case MouseButton.Right:
                        if (!canRight)
                        {
                            e.Handled = false;
                            return;
                        }
                        break;
                    default: return;
                }
                e.Handled = true;
                if (busy) return;
                button = e.ChangedButton;
                point = e.GetPosition(window);
                Mouse.Capture(window);
                joke = busy = true;
            }

            private void MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
            {
                if (!((button == MouseButton.Left && e.LeftButton == MouseButtonState.Pressed) || (button == MouseButton.Right && e.RightButton == MouseButtonState.Pressed)))
                {
                    e.Handled = false;
                    return;
                }
                e.Handled = true;
                if (!busy) return;
                bool pressing;
                if (button == MouseButton.Left) pressing = e.LeftButton == MouseButtonState.Pressed;
                else pressing = e.RightButton == MouseButtonState.Pressed;
                if (pressing)
                {
                    var vector = e.GetPosition(window) - point;
                    if (joke && (vector.X > 3 || vector.Y > 3 || vector.X < -3 || vector.Y < -3))
                    {
                        joke = false;
                        if (dragMoveCallback == null || !dragMoveCallback.CanExecute) return;
                        if (button == MouseButton.Left) dragMoveCallback.ExecuteAsync(0, 1);
                        else if (button == MouseButton.Right) dragMoveCallback.ExecuteAsync(1, 1);
                    }
                    if (!joke)
                    {
                        window.Left += vector.X;
                        window.Top += vector.Y;
                    }
                }
                else
                {
                    busy = false;
                    Mouse.Capture(null);
                    if (dragMoveCallback == null || !dragMoveCallback.CanExecute) return;
                    if (button == MouseButton.Left) dragMoveCallback.ExecuteAsync(0, joke ? 0 : 2);
                    else if (button == MouseButton.Right) dragMoveCallback.ExecuteAsync(1, joke ? 0 : 2);
                }
            }
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
