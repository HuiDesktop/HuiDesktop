using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;

namespace HuiDesktop
{
    static class ApplicationInfo
    {
        internal static string BaseFolder => AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
        internal static string CefSharpFolder => Path.Combine(BaseFolder, Environment.Is64BitProcess ? "x64" : "x86");

        internal static string RelativePath(params string[] paths)
        {
            string[] full = new string[paths.Length + 1];
            paths.CopyTo(full, 1);
            full[0] = BaseFolder;
            return Path.Combine(full);
        }

        internal static int FrameRate => 60;
    }

    public class Permission
    {
        string[] allowed;

        public Permission(IEnumerable<string> ts)
        {
            allowed = ts.ToArray();
        }

        public bool Query(string permission)
        {
            var prefix = from e in allowed where permission.StartsWith(e.EndsWith("*") ? e.Substring(0, e.Length-1) : e) select e;
            return prefix.Any();
        }
    }
}
