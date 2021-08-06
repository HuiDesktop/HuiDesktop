using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace HuiDesktop.NextGen
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        public static Assembly Resolver(object sender, ResolveEventArgs args)
        {
#if MULTIARCH
            if (args.Name.StartsWith("CefSharp"))
            {
                string assemblyName = args.Name.Split(new[] { ',' }, 2)[0] + ".dll";
                string archSpecificPath = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, Environment.Is64BitProcess ? "x64" : "x86", assemblyName);
                return File.Exists(archSpecificPath) ? Assembly.LoadFile(archSpecificPath) : null;
            }
#endif
            return null;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            AppDomain.CurrentDomain.AssemblyResolve += Resolver;

            FileSystemManager.SetPath();
            Asset.ModuleManager.LoadModulesFromDirectory(FileSystemManager.ModulePath);
            Asset.SandboxManager.LoadSandboxesFromDirectory(FileSystemManager.SandboxPath);

            AppConfig.Load();
            if (AppConfig.Instance.JoinSharePlan)
            {
                _ = SharePlanService.UploadAtExe();
            }

            base.OnStartup(e);
        }
    }
}
