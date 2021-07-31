using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HuiDesktop.NextGen.Asset
{
    public class Sandbox
    {
        private readonly Guid[] dependencies;

        public string Name { get; }
        public IEnumerable<Guid> Dependencies => dependencies;

        private Sandbox(string name, Guid[] dependencies)
        {
            Name = name;
            this.dependencies = dependencies;
        }

        public static Sandbox LoadFromDirectoryWithName(string directory, string name)
        {
            if (string.IsNullOrWhiteSpace(name) || name.EndsWith(".hdtinc")) throw new ArgumentException("Invalid sandbox name.");
            foreach (var c in Path.GetInvalidFileNameChars())
            {
                if (name.Contains(c)) throw new ArgumentException("Invalid sandbox name.");
            }
            var ls = File.ReadAllLines(Path.Combine(directory, name + ".hdtinc"));
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
            return new Sandbox(name, gs.ToArray());
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
