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
        private JsApi api;

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
            notifyIcon.ContextMenu.MenuItems.Add(new System.Windows.Forms.MenuItem("退出", ContextMenu_Exit));

            browser = new ChromiumWebBrowser();
            browser.BrowserSettings = new BrowserSettings { WindowlessFrameRate = 60, LocalStorage = CefState.Enabled };
            AddChild(browser);

            api = new JsApi(this);
            browser.RequestHandler = requestHandler;
            browser.MenuHandler = new NullMenuHandler();
            browser.JavascriptObjectRepository.Settings.LegacyBindingEnabled = true;
            browser.JavascriptObjectRepository.ResolveObject += (s, e) =>
            {
                if (e.ObjectName == CefSharp.Internals.JavascriptObjectRepository.LegacyObjects)
                {
                    //ADD YOUR LEGACY
                    e.ObjectRepository.Register(name: "huiDesktop", objectToBind: api, isAsync: false);
                }
            };

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
            Cef.Shutdown();
            Environment.Exit(0);
        }
    }

    static class CefInitialize
    {
        public static void InitializeCefSharp(bool disableBlackList)
        {
            CefSharpSettings.WcfEnabled = true;

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

    class JsApi
    {
        public static string CallWithTryCatch(string name)
            => $"try{{{name}}}catch(e){{}}";

        public class EasyBasicWindow
        {
            private Window _parent;
            private double top, left, width, height;

            public EasyBasicWindow(JsApi parent)
            {
                _parent = parent._parent;
                _parent.LocationChanged += Window_LocationChanged;
                _parent.SizeChanged += Window_SizeChanged;
            }

            public double Left
            {
                get => left;
                set => _parent.Dispatcher.Invoke(() => _parent.Left = value);
            }

            public double Top
            {
                get => top;
                set => _parent.Dispatcher.Invoke(() => _parent.Top = value);
            }

            public double Width
            {
                get => width;
                set => _parent.Dispatcher.Invoke(() => _parent.Width = value);
            }

            public double Height
            {
                get => height;
                set => _parent.Dispatcher.Invoke(() => _parent.Height = value);
            }

            private void Window_LocationChanged(object sender, EventArgs e)
            {
                left = Convert.ToInt32((sender as Window).Left);
                top = Convert.ToInt32((sender as Window).Top);
            }

            private void Window_SizeChanged(object sender, EventArgs e)
            {
                height = Convert.ToInt32((sender as Window).Height);
                width = Convert.ToInt32((sender as Window).Width);
            }
        }

        public class EasyWorkingArea
        {
            public double Top { get; }
            public double Left { get; }
            public double Width { get; }
            public double Height { get; }

            public EasyWorkingArea()
            {
                Top = SystemParameters.WorkArea.Top;
                Left = SystemParameters.WorkArea.Left;
                Width = SystemParameters.WorkArea.Width;
                Height = SystemParameters.WorkArea.Height;
            }
        }

        public class EasyBasicScreen
        {
            public double Width { get; }
            public double Height { get; }

            public EasyBasicScreen()
            {
                Width = SystemParameters.PrimaryScreenWidth;
                Height = SystemParameters.PrimaryScreenHeight;
            }
        }

        private class DragMoveManager
        {
            Window window;
            ChromiumWebBrowser browser;
            Point point;
            MouseButton button;
            bool busy, joke;

            public bool canLeft, canRight;

            public DragMoveManager(Window window, ChromiumWebBrowser browser)
            {
                this.window = window;
                this.browser = browser;
                browser.PreviewMouseDown += MouseDown;
                window.MouseMove += MouseMove;
                window.MouseUp += MouseUp;
                busy = false;
            }

            private void MouseUp(object sender, MouseButtonEventArgs e)
            {
                if (!busy) return;
                if (button != e.ChangedButton) return;
                e.Handled = true;
                busy = false;
                System.Diagnostics.Debug.WriteLine("Up");
                System.Diagnostics.Debug.Flush();
                Mouse.Capture(null);
                browser.GetBrowser().MainFrame.ExecuteJavaScriptAsync(CallWithTryCatch($"huiDesktop_DragMove_OnMouse{button}{(joke ? "Click" : "Up")}()"), browser.Address);
            }

            private void MouseDown(object sender, MouseButtonEventArgs e)
            {
                if (busy) return;
                switch (e.ChangedButton)
                {
                    case MouseButton.Left:
                        if (!canLeft) return;
                        break;
                    case MouseButton.Right:
                        if (!canRight) return;
                        break;
                    default: return;
                }
                System.Diagnostics.Debug.WriteLine("Down");
                System.Diagnostics.Debug.Flush();
                e.Handled = true;
                button = e.ChangedButton;
                point = e.GetPosition(window);
                Mouse.Capture(window);
                joke = busy = true;
            }

            private void MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
            {
                if (!busy) return;
                e.Handled = true;
                bool pressing;
                if (button == MouseButton.Left) pressing = e.LeftButton == MouseButtonState.Pressed;
                else pressing = e.RightButton == MouseButtonState.Pressed;
                if (pressing)
                {
                    var vector = e.GetPosition(window) - point;
                    if (joke && (vector.X > 3 || vector.Y > 3 || vector.X < -3 || vector.Y < -3))
                    {
                        joke = false;
                        browser.GetBrowser().MainFrame.ExecuteJavaScriptAsync(CallWithTryCatch($"huiDesktop_DragMove_OnMouse{button}Down()"), browser.Address);
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
                    browser.GetBrowser().MainFrame.ExecuteJavaScriptAsync(CallWithTryCatch($"huiDesktop_DragMove_OnMouse{button}{(joke ? "Click" : "Up")}()"), browser.Address);
                }
            }
        }

        private BasicWindow _parent;
        private DragMoveManager _dragMoveManager;
        public EasyBasicWindow Window { get; }
        public EasyWorkingArea WorkingArea { get; }
        public EasyBasicScreen Screen { get; }

        public JsApi(BasicWindow parent)
        {
            _parent = parent;
            _dragMoveManager = new DragMoveManager(_parent, _parent.browser);
            Window = new EasyBasicWindow(this);
            WorkingArea = new EasyWorkingArea();
            Screen = new EasyBasicScreen();
        }

        public int ApiVersion => 1;

        public bool TopMost { set => _parent.Dispatcher.Invoke(() => _parent.Topmost = value); }
        public bool DragMoveLeft { get => _dragMoveManager.canLeft; set => _dragMoveManager.canLeft = value; }
        public bool DragMoveRight { get => _dragMoveManager.canRight; set => _dragMoveManager.canRight = value; }
        public bool ShowInTaskbar { set => _parent.Dispatcher.Invoke(() => _parent.ShowInTaskbar = value); }
        public bool ClickTransparent
        {
            set => _parent.Dispatcher.Invoke(() =>
              {
                  var handle = new System.Windows.Interop.WindowInteropHelper(_parent).Handle;
                  var t = Win32Api.GetWindowLong(handle, Win32Api.GWL_EXSTYLE);
                  if ((((t & Win32Api.WS_EX_TRANSPARENT) != 0) ^ value) == true)
                  {
                      t ^= Win32Api.WS_EX_TRANSPARENT;
                      Win32Api.SetWindowLong(handle, Win32Api.GWL_EXSTYLE, t);
                  }
              });
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
