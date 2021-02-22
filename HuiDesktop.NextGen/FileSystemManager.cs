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
        public static string ModulePath { get; private set; }
        public static string SandboxPath { get; private set; }

        public static void SetPath()
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "HuixStudio", "HuiDesktopNextGen");
            ModulePath = Path.Combine(path, "Modules");
            SandboxPath = Path.Combine(path, "Sandboxes");
            if (!Directory.Exists(ModulePath)) Directory.CreateDirectory(ModulePath);
            if (!Directory.Exists(SandboxPath)) Directory.CreateDirectory(SandboxPath);
        }
    }
}
