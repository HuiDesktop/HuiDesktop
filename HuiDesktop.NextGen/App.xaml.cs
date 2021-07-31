using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;

namespace HuiDesktop.NextGen
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            AppDomain.CurrentDomain.AssemblyResolve += CefStartupInitialize.Resolver;

            FileSystemManager.SetPath();
            Asset.ModuleManager.LoadModulesFromDirectory(FileSystemManager.NextGenModulePath);
            Asset.SandboxManager.LoadSandboxesFromDirectory(FileSystemManager.NextGenSandboxPath);

            Task.Run(async () =>
            {
                var cli = new HttpClient();
                await cli.GetAsync("https://desktop.huix.cc/api/stat/online?ver=" + UpdateService.Version);
            });

            base.OnStartup(e);
        }
    }
}
