using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace HuiDesktop.Wpf.Play
{
    class KeyboardHook
    {
        int hHook;

        Win32Api.HookProc KeyboardHookDelegate;

        public event Action<int> OnKeyDownEvent;
        public event Action<int> OnKeyUpEvent;

        public KeyboardHook() { }

        public void SetHook()
        {
            KeyboardHookDelegate = new Win32Api.HookProc(KeyboardHookProc);
            Process cProcess = Process.GetCurrentProcess();
            ProcessModule cModule = cProcess.MainModule;
            var mh = Win32Api.GetModuleHandle(cModule.ModuleName);
            hHook = Win32Api.SetWindowsHookEx(Win32Api.WH_KEYBOARD_LL, KeyboardHookDelegate, mh, 0);
        }

        public void UnHook()
        {
            Win32Api.UnhookWindowsHookEx(hHook);
            hHook = 0;
        }

        ~KeyboardHook()
        {
            if (hHook != 0) Win32Api.UnhookWindowsHookEx(hHook);
        }

        private int KeyboardHookProc(int nCode, int wParam, IntPtr lParam)
        {
            //如果该消息被丢弃（nCode<0）或者没有事件绑定处理程序则不会触发事件
            try
            {
                if ((nCode >= 0) && (OnKeyDownEvent != null || OnKeyUpEvent != null))
                {
                     Win32Api.KeyboardHookStruct KeyDataFromHook = (Win32Api.KeyboardHookStruct)Marshal.PtrToStructure(lParam, typeof(Win32Api.KeyboardHookStruct));
                    int keyData = KeyDataFromHook.vkCode;
                    if (keyData == 32)
                    {
                        //WM_KEYDOWN和WM_SYSKEYDOWN消息，将会引发OnKeyDownEvent事件
                        if (OnKeyDownEvent != null && (wParam == Win32Api.WM_KEYDOWN || wParam == Win32Api.WM_SYSKEYDOWN))
                        {
                            OnKeyDownEvent(keyData);
                        }

                        //WM_KEYUP和WM_SYSKEYUP消息，将引发OnKeyUpEvent事件 
                        if (OnKeyUpEvent != null && (wParam == Win32Api.WM_KEYUP || wParam == Win32Api.WM_SYSKEYUP))
                        {
                            OnKeyUpEvent(keyData);
                        }
                    }
                }
            }
            finally
            {
            }
            return Win32Api.CallNextHookEx(hHook, nCode, wParam, lParam);
        }
    }
}