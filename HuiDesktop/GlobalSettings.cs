using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HuiDesktop
{
    internal static class GlobalSettings
    {
        public static bool AutoRun
        {
            get
            {
                var key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run");
                var val = key.GetValue("HuiDesktop");
                if (val == null || !(val is string)) return false;
                return (val as string) == ApplicationInfo.RelativePath("HuiDesktop.exe") + " --autorun";
            }
            set
            {
                var key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                if (value) key.SetValue("HuiDesktop", ApplicationInfo.RelativePath("HuiDesktop.exe") + " --autorun");
                else key.DeleteValue("HuiDesktop", false);
            }
        }

        public static string AutoRunItem
        {
            get
            {
                var key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\HuixStudio\HuiDesktop");
                var val = key.GetValue("AutoRunStrongName");
                if (val == null || !(val is string)) return null;
                return val as string;
            }
            set
            {
                var key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\HuixStudio\HuiDesktop");
                key.SetValue("AutoRunStrongName", value);
            }
        }

        static string[] hiddenStrongNames;

        public static string[] HiddenStrongNames
        {
            get
            {
                if (hiddenStrongNames != null) return hiddenStrongNames;
                string path = ApplicationInfo.RelativePath("HiddenStrongNames.txt");
                if (File.Exists(path) == false) return Array.Empty<string>();
                hiddenStrongNames = File.ReadAllText(ApplicationInfo.RelativePath("HiddenStrongNames.txt")).Split('\n').Except(new string[] { "" }).ToArray();
                return hiddenStrongNames;
            }
            set
            {
                hiddenStrongNames = value;
                File.WriteAllLines(ApplicationInfo.RelativePath("HiddenStrongNames.txt"), value);
            }
        }

        public static bool AutoCheckUpdate
        {
            get
            {
                var key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\HuixStudio\HuiDesktop");
                var val = key.GetValue("AutoCheckUpdate");
                if (val == null || !(val is int)) return false;
                return ((int)val) == 1;
            }
            set
            {
                var key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\HuixStudio\HuiDesktop", true);
                if (value) key.SetValue("AutoCheckUpdate", 1);
                else key.DeleteValue("AutoCheckUpdate", false);
            }
        }
    }
}
