using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HuiDesktop.NextGen
{
    [Obsolete]
    public class Sandbox
    {
        public string SandboxName { get; }
        public Guid MainModuleGuid { get; }
        public Module MainModule
        {
            get
            {
                if (ModuleManager.ModuleDictionary.TryGetValue(MainModuleGuid, out var val))
                {
                    return val;
                }
                return null;
            }
        }
        public string Path { get; }

        public Sandbox(string sandboxName, Guid mainModuleGuid, string path)
        {
            SandboxName = sandboxName;
            MainModuleGuid = mainModuleGuid;
            Path = path;
        }
    }

    [Obsolete]
    static class SandboxManager
    {
        public static Dictionary<string, Sandbox> SandboxDictionary { get; private set; } = new Dictionary<string, Sandbox>();

        public static void LoadSandboxes()
        {
            SandboxDictionary.Clear();
            foreach (var i in Directory.EnumerateDirectories(FileSystemManager.SandboxPath))
            {
                string moduleIdPath = Path.Combine(i, "mainModule");
                if (File.Exists(moduleIdPath))
                {
                    if (Guid.TryParse(File.ReadAllText(moduleIdPath), out Guid id))
                    {
                        string name = Path.GetFileName(i);
                        SandboxDictionary.Add(name, new Sandbox(name, id, i));
                    }
                }
            }
        }

        public static string CreateEmptySandbox(string name)
        {
            var r = Path.Combine(FileSystemManager.SandboxPath, name);
            Directory.CreateDirectory(r);
            return r;
        }
    }
}
