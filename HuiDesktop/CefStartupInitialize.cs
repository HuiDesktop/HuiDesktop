using CefSharp;
using CefSharp.Wpf;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace HuiDesktop
{
    static class CefStartupInitialize
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void InitializeCefSharp()
        {
            CefSharpSettings.LegacyJavascriptBindingEnabled = true;
            CefSharpSettings.WcfEnabled = true;

            var settings = new CefSettings
            {
                BrowserSubprocessPath = Path.Combine(ApplicationInfo.CefSharpFolder, "CefSharp.BrowserSubprocess.exe"),
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
            Cef.Initialize(settings);
        }

        internal static Assembly Resolver(object sender, ResolveEventArgs args)
        {
            if (args.Name.StartsWith("CefSharp"))
            {
                string assemblyName = args.Name.Split(new[] { ',' }, 2)[0] + ".dll";
                string archSpecificPath = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, Environment.Is64BitProcess ? "x64" : "x86", assemblyName);
                return File.Exists(archSpecificPath) ? Assembly.LoadFile(archSpecificPath) : null;
            }
            return null;
        }
    }
}
