using CefSharp;
using CefSharp.Wpf;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;

namespace HuiDesktop
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            if (Directory.Exists(ApplicationInfo.RelativePath("packages")) == false)
                Directory.CreateDirectory(ApplicationInfo.RelativePath("packages"));
            if (Environment.GetCommandLineArgs().Length > 2 && Environment.GetCommandLineArgs()[1] == "--copy")
            {
                var args = Environment.GetCommandLineArgs().Skip(2);
                foreach (var i in args)
                {
                    if (!i.StartsWith(ApplicationInfo.RelativePath("packages" + '\\')))
                    {
                        var path = ApplicationInfo.RelativePath("packages", Path.GetFileName(i));
                        if (File.Exists(path))
                        {
                            if (Directory.Exists(ApplicationInfo.RelativePath("backupPackages")) == false) Directory.CreateDirectory(ApplicationInfo.RelativePath("backupPackages"));
                            File.Move(path, ApplicationInfo.RelativePath("backupPackages", Guid.NewGuid().ToString() + Path.GetFileName(i)));
                        }
                        File.Copy(i, path);
                    }
                }
            }
            Package.PackageManager.LoadPackages(ApplicationInfo.RelativePath("packages"));

            if (Directory.Exists(ApplicationInfo.RelativePath("localPackages")) == false)
                Directory.CreateDirectory(ApplicationInfo.RelativePath("localPackages"));
            else Package.PackageManager.LoadLocalPackages(ApplicationInfo.RelativePath("localPackages"));

            AppDomain.CurrentDomain.AssemblyResolve += CefStartupInitialize.Resolver;
            CefStartupInitialize.InitializeCefSharp();
        }
    }
}
