using CefSharp;
using CefSharp.OffScreen;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace HuiDesktop.DirectComposition
{
    static class ApplicationInfo
    {
        internal static string Version
        {
            get
            {
                var version = Assembly.GetExecutingAssembly().GetName().Version;
                return $"{version.Major}.{version.Minor}.{version.Build}";
            }
        }
        internal static string BaseFolder => AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
        internal static string CefSharpFolder => Path.Combine(BaseFolder, Environment.Is64BitProcess ? "x64" : "x86");

        internal static string RelativePath(params string[] paths)
        {
            string[] full = new string[paths.Length + 1];
            paths.CopyTo(full, 1);
            full[0] = BaseFolder;
            return Path.Combine(full);
        }
    }

    public static class CefStartupInitialize
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void InitializeCefSharp()
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
            CefSharp.Cef.Initialize(settings);
        }

        public static Assembly Resolver(object sender, ResolveEventArgs args)
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
