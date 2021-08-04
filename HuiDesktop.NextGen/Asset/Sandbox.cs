using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HuiDesktop.NextGen.Asset
{
    public class Sandbox
    {
        private readonly Guid[] dependencies;

        public string BasePath { get; }
        public string Name { get; }
        public IEnumerable<Guid> Dependencies => dependencies;

        private Sandbox(string basePath, string name, Guid[] dependencies)
        {
            BasePath = basePath;
            Name = name;
            this.dependencies = dependencies;
        }

        public IEnumerable<ModuleLaunchInfo> GetLaunchInfos()
        {
            var l = new List<ModuleLaunchInfo>();
            foreach (var i in dependencies)
            {
                if (ModuleManager.GetModule(i) is Module m)
                {
                    l.AddRange(m.LaunchInfos);
                }
            }
            return l;
        }

        public IEnumerable<ModuleLaunchInfo> GetSetupLaunchInfos()
        {
            var l = new List<ModuleLaunchInfo>();
            foreach (var i in dependencies)
            {
                if (ModuleManager.GetModule(i) is Module m)
                {
                    l.AddRange(m.SetupInfos);
                }
            }
            return l;
        }

        internal void SetDependencies(List<Guid> list)
        {
            using (var f = new StreamWriter(Path.Combine(BasePath, "includes.txt")))
            {
                foreach (var i in list)
                {
                    f.WriteLine(i);
                }
            }
        }

        public static Sandbox LoadFromDirectory(string directory)
        {
            if (string.IsNullOrWhiteSpace(directory)) throw new ArgumentException("Invalid sandbox name.");
            var ls = File.ReadAllLines(Path.Combine(directory, "includes.txt"));
            var gs = new List<Guid>();
            foreach (var i in ls)
            {
                if (string.IsNullOrWhiteSpace(i)) continue;
                if (Guid.TryParse(i, out var g))
                {
                    gs.Add(g);
                }
                else
                {
                    //TODO: Something should be applied
                }
            }
            return new Sandbox(directory, Path.GetFileName(directory.TrimEnd('/', '\\')), gs.ToArray());
        }

        public static string Create(string name, string baseDirectory = null)
        {
            if (baseDirectory == null) baseDirectory = FileSystemManager.NextGenSandboxPath;
            var path = Path.Combine(baseDirectory, name);
            Directory.CreateDirectory(path);
            Directory.CreateDirectory(Path.Combine(baseDirectory, name, "Root"));
            File.WriteAllText(Path.Combine(baseDirectory, name, "includes.txt"), "");
            return path;
        }

        /// <summary>
        /// 检查此沙盒的依赖
        /// </summary>
        /// <returns>依赖完整则返回Empty，否则返回缺少的包</returns>
        public Guid CheckDependencies()
        {
            foreach (var i in dependencies)
            {
                if (ModuleManager.GetModule(i) == null)
                {
                    return i;
                }
            }
            return Guid.Empty;
        }

        public void Remove()
        {
            Directory.Delete(BasePath, true);
        }

        public string StringfySandboxInfo()
        {
            var root = new { name = Name, dependencies = new List<object>() };
            foreach (var i in Dependencies)
            {
                if (ModuleManager.GetModule(i) is Module m)
                {
                    root.dependencies.Add(new { id = m.Id, name = m.Name.ToString(), friendlyName = m.FriendlyName, features = m.Features });
                }
            }
            return Newtonsoft.Json.JsonConvert.SerializeObject(root);
        }
    }
}
