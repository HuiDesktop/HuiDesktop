using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HuiDesktop
{
    public class MyConfiguration : ConfigurationSection
    {
        private const string AppSettingsName = "huiDesktopSettings";
        private static MyConfiguration globalConfig;
        private static Configuration config;

        [ConfigurationProperty("UseDirectComposition", DefaultValue = false, IsRequired = true)]
        public bool UseDirectComposition { get => (bool)this["UseDirectComposition"]; set => this["UseDirectComposition"] = value; }
        [ConfigurationProperty("HitTestSize", DefaultValue = 16, IsRequired = true)]
        public int HitTestSize { get => (int)this["HitTestSize"]; set => this["HitTestSize"] = value; }

        public static MyConfiguration GlobalConfig
        {
            get
            {
                if (globalConfig == null)
                {
                    config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                    try
                    {
                        globalConfig = config.Sections[AppSettingsName] as MyConfiguration;
                        if (globalConfig == null) throw new Exception();
                    }
                    catch
                    {
                        globalConfig = new MyConfiguration();
                        config.Sections.Remove(AppSettingsName);
                        config.Sections.Add(AppSettingsName, globalConfig);
                        config.Save();
                    }
                }
                return globalConfig;
            }
        }

        public static void Save()
        {
            System.Diagnostics.Debug.Assert(config != null);
            config.Save();
        }
    }
}
