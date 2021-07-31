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
    }
}
