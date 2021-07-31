using System;
using System.Collections.Generic;
using System.IO;

namespace HuiDesktop.NextGen.Asset
{
    /// <summary>
    /// 全局模块管理
    /// </summary>
    public static class ModuleManager
    {
        static Dictionary<Guid, Module> modules = new Dictionary<Guid, Module>();

        public static void LoadModulesFromDirectory(string directroy)
        {
            modules.Clear();
            foreach (var i in Directory.EnumerateDirectories(directroy))
            {
                try
                {
                    var module = Module.LoadFromDirectory(i);
                    modules.Add(module.Id, module);
                }
                catch
                {
                    //TODO: Something should be applied
                }
            }
        }

        public static Module GetModule(Guid id)
        {
            if (modules.TryGetValue(id, out var value))
            {
                return value;
            }
            return null;
        }
    }
}
