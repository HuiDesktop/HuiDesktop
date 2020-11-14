using CefSharp.OffScreen;
using HuiDesktop.DirectComposition.Interop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HuiDesktop.DirectComposition
{
    public enum MouseButton
    {
        Left,
        Right
    }
    class JsApi
    {
        public static string CallWithTryCatch(string name)
            => $"try{{{name}}}catch(e){{}}";

        public class EasyBasicWindow
        {
            private MainWindow _parent;
            private double top, left, width, height;

            public EasyBasicWindow(JsApi parent)
            {
                _parent = parent._parent;
                _parent.SizeChanged += Window_SizeChanged;
                _parent.LocationChanged += Window_LocationChanged;
            }

            public double Left
            {
                get => left;
                set => _parent.Left = Convert.ToInt32(value);
            }

            public double Top
            {
                get => top;
                set => _parent.Top = Convert.ToInt32(value);
            }

            public double Width
            {
                get => width;
                set => _parent.Width = Convert.ToInt32(value);
            }

            public double Height
            {
                get => height;
                set => _parent.Height = Convert.ToInt32(value);
            }

            private void Window_LocationChanged()
            {
                left = _parent.Left;
                top = _parent.Top;
            }

            private void Window_SizeChanged()
            {
                width = _parent.Width;
                height = _parent.Height;
            }
        }

        public class EasyWorkingArea
        {
            public double Top { get; }
            public double Left { get; }
            public double Width { get; }
            public double Height { get; }

            public EasyWorkingArea()
            { // 用了WPF的东西，不清真
                Top = System.Windows.SystemParameters.WorkArea.Top;
                Left = System.Windows.SystemParameters.WorkArea.Left;
                Width = System.Windows.SystemParameters.WorkArea.Width;
                Height = System.Windows.SystemParameters.WorkArea.Height;
            }
        }

        public class EasyBasicScreen
        {
            public double Width { get; }
            public double Height { get; }

            public EasyBasicScreen()
            {
                Width = System.Windows.SystemParameters.PrimaryScreenWidth;
                Height = System.Windows.SystemParameters.PrimaryScreenHeight;
            }
        }

        private class DragMoveManager
        {
            MainWindow window;
            ChromiumWebBrowser browser;
            Point point;
            bool busy, joke;
            MouseButton button;

            public bool canLeft, canRight;

            public DragMoveManager(MainWindow window, ChromiumWebBrowser browser)
            {
                this.window = window;
                this.browser = browser;
                window.MouseDown += MouseDown;
                window.MouseMove += MouseMove;
                window.MouseUp += (button, _) => MouseUp(button);
                busy = false;
            }

            private void MouseUp(MouseButton button)
            {
                if (!busy) return;
                if (this.button != button) return;
                busy = false;
                window.ReleaseCapture();
                Debug.WriteLine(joke);
                browser.GetBrowser().MainFrame.ExecuteJavaScriptAsync(CallWithTryCatch($"huiDesktop_DragMove_OnMouse{button}{(joke ? "Click" : "Up")}()"), browser.Address);
            }

            private void MouseDown(MouseButton button, Point position)
            {
                if (busy) return;
                switch (button)
                {
                    case MouseButton.Left:
                        if (!canLeft) return;
                        break;
                    case MouseButton.Right:
                        if (!canRight) return;
                        break;
                    default: return;
                }
                this.button = button;
                point = position;
                joke = busy = true;
                window.CaptureMouse();
            }

            private void MouseMove(Point position)
            {
                if (!busy) return;
                Debug.WriteLine(position);
                Point vector = new(position.X - point.X, position.Y - point.Y);
                if (joke && (vector.X > 3 || vector.Y > 3 || vector.X < -3 || vector.Y < -3))
                {
                    joke = false;
                    Debug.WriteLine("Moving");
                    browser.GetBrowser().MainFrame.ExecuteJavaScriptAsync(CallWithTryCatch($"huiDesktop_DragMove_OnMouse{button}Down()"), browser.Address);
                }
                if (!joke)
                {
                    window.Left += vector.X;
                    window.Top += vector.Y;
                }
            }
        }

        private MainWindow _parent;
        private ChromiumWebBrowser _browser;
        private DragMoveManager _dragMoveManager;
        public EasyBasicWindow Window { get; }
        public EasyWorkingArea WorkingArea { get; }
        public EasyBasicScreen Screen { get; }

        public JsApi(MainWindow parent, ChromiumWebBrowser browser)
        {
            _parent = parent;
            _browser = browser;
            _dragMoveManager = new DragMoveManager(_parent, _browser);
            Window = new EasyBasicWindow(this);
            WorkingArea = new EasyWorkingArea();
            Screen = new EasyBasicScreen();
        }

        public int ApiVersion => 1;

        public bool TopMost { set => _parent.Topmost = value; }
        public bool DragMoveLeft { get => _dragMoveManager.canLeft; set => _dragMoveManager.canLeft = value; }
        public bool DragMoveRight { get => _dragMoveManager.canRight; set => _dragMoveManager.canRight = value; }
        public bool ShowInTaskbar { set => _parent.ShowInTaskbar = value; }
        public bool ClickTransparent { set => _parent.IgnoreClick = value; }
    }
}
