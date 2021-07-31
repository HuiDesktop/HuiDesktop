using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HuiDesktop.NextGen
{
    static class FileSystemManager
    {
        public static string BasePath { get; set; }
        public static string ModulePath { get; private set; }
        public static string SandboxPath { get; private set; }
        public static string AppConfigPath { get; private set; }

        public static string NextGenModulePath { get; private set; }

        public static void SetPath()
        {
            if (Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Files"))) BasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Files");
            else BasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "HuixStudio", "HuiDesktopNextGen");
            ModulePath = Path.Combine(BasePath, "Modules");
            SandboxPath = Path.Combine(BasePath, "Sandboxes");
            AppConfigPath= Path.Combine(BasePath, "config.json");
            NextGenModulePath = Path.Combine(BasePath, "NextGenModules");
            if (!Directory.Exists(ModulePath)) Directory.CreateDirectory(ModulePath);
            if (!Directory.Exists(SandboxPath)) Directory.CreateDirectory(SandboxPath);
            if (!Directory.Exists(NextGenModulePath)) Directory.CreateDirectory(NextGenModulePath);
            AppConfig.Load();
        }
    }
}
