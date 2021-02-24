using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
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
            FileSystemManager.SetPath();
            ModuleManager.LoadModules();
            SandboxManager.LoadSandboxes();
            base.OnStartup(e);
        }
    }
}
