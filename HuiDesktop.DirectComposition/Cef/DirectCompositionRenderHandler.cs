using CefSharp;
using CefSharp.Enums;
using CefSharp.OffScreen;
using CefSharp.Structs;
using HuiDesktop.DirectComposition.Interop;
using System;
using System.Drawing;

namespace HuiDesktop.DirectComposition.Cef
{
    class DirectCompositionRenderHandler : IRenderHandler
    {
        private MainWindow window;
        private Action<Rectangle, IntPtr> RequestRenderToMainWindow;

        public DirectCompositionRenderHandler(Action<Rectangle, IntPtr> requestRenderToMainWindow, MainWindow window)
        {
            RequestRenderToMainWindow = requestRenderToMainWindow;
            this.window = window;
        }

        public void Dispose()
        {
            //All vals are ref
        }

        public ScreenInfo? GetScreenInfo()
        {
            return new ScreenInfo { DeviceScaleFactor = 1 };
        }

        public bool GetScreenPoint(int viewX, int viewY, out int screenX, out int screenY)
        {
            screenX = 0;
            screenY = 0;
            return false;
        }

        public Rect GetViewRect()
        {
            return new Rect(0, 0, window.Width, window.Height);
        }

        bool skip;

        public void OnAcceleratedPaint(PaintElementType type, Rect dirtyRect, IntPtr sharedHandle)
        {
            if (type != PaintElementType.View) return;
            if (skip) { skip = false;return; }
            skip = true;
            RequestRenderToMainWindow(new Rectangle(dirtyRect.X, dirtyRect.Y, dirtyRect.Width, dirtyRect.Height), sharedHandle);
        }

        public void OnCursorChange(IntPtr cursor, CursorType type, CursorInfo customCursorInfo)
        {
            //QwQ 卖个萌就返回吧
        }

        public void OnImeCompositionRangeChanged(Range selectedRange, Rect[] characterBounds)
        {
            //QwQ 卖个萌就返回把
        }

        public void OnPaint(PaintElementType type, Rect dirtyRect, IntPtr buffer, int width, int height)
        {
            throw new InvalidOperationException("Expect OnAcceleratedPaint");
        }

        public void OnPopupShow(bool show)
        {
            throw new NotImplementedException();
        }

        public void OnPopupSize(Rect rect)
        {
            throw new NotImplementedException();
        }

        public void OnVirtualKeyboardRequested(IBrowser browser, TextInputMode inputMode)
        {
            throw new NotImplementedException();
        }

        public bool StartDragging(IDragData dragData, DragOperationsMask mask, int x, int y)
        {
            throw new NotImplementedException();
        }

        public void UpdateDragCursor(DragOperationsMask operation)
        {
            throw new NotImplementedException();
        }
    }
}
