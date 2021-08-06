using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HuiDesktop.NextGen.Asset
{
    public static class SandboxManager
    {
        static List<Sandbox> sandboxes = new List<Sandbox>();

        public static IEnumerable<Sandbox> Sandboxes => sandboxes;

        public static void LoadSandboxesFromDirectory(string directory)
        {
            sandboxes.Clear();
            foreach (var i in Directory.EnumerateDirectories(directory))
            {
                try
                {
                    var sandbox = Sandbox.LoadFromDirectory(i);
                    sandboxes.Add(sandbox);
                }
                catch
                {
                    //TODO: Something should be applied
                }
            }
        }

        public static void LoadSandboxes()
        {
            LoadSandboxesFromDirectory(FileSystemManager.SandboxPath);
        }
    }
}
