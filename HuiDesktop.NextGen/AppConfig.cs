using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HuiDesktop.NextGen
{
    public class AppConfig
    {
        public static AppConfig Instance { get; private set; }

        public string AutoRunSandboxName { get; set; } = "";
        public bool AutoCheckUpdate { get; set; }
        public bool ForceWebGL { get; set; } = true;
        public bool JoinSharePlan { get; set; }

        public static void Load()
        {
            try
            {
                Instance = Newtonsoft.Json.JsonConvert.DeserializeObject<AppConfig>(File.ReadAllText(FileSystemManager.AppConfigPath));
            }
            catch (Exception)
            {
                Instance = new AppConfig();
                Instance.Save();
                return;
            }
        }

        public void Save()
        {
            File.WriteAllText(FileSystemManager.AppConfigPath, Newtonsoft.Json.JsonConvert.SerializeObject(this));
            var key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
            if (string.IsNullOrWhiteSpace(AutoRunSandboxName))
            {
                key.DeleteValue("HuiDesktop", false);
            }
            else
            {
                key.SetValue("HuiDesktop.NextGen", System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName + " --autorun");
            }
        }

        public static void BindScheme()
        {
            Registry.CurrentUser.DeleteSubKeyTree(@"SOFTWARE\Classes\hdt", false);
            using (var key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Classes\hdt", true))
            {
                key.SetValue("", "URL:HuiDesktop Protocol");
                key.SetValue("URL Protocol", "");
            }
            using (var key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Classes\hdt\Shell\Open\Command", true))
            {
                key.SetValue("", $"\"{typeof(AppConfig).Assembly.Location}\" --hdt-url %1");
            }
        }
    }
}
