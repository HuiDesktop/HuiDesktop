using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HuiDesktop.NextGen
{
    static class UpdateService
    {
        public const string ViewUpdatePage = "https://stable-service.huix.cc/update/huidesktop/redirect.html";

        public static string Version => System.Diagnostics.FileVersionInfo.GetVersionInfo(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName).FileVersion;

        public static string GitCommitId
        {
            get
            {
                Assembly _assembly = Assembly.GetExecutingAssembly();
                string aname = Assembly.GetExecutingAssembly().GetName().Name.ToString();
                Stream stream = _assembly.GetManifestResourceStream(aname + ".GitCommitId");
                StreamReader reader = new StreamReader(stream);
                string str = reader.ReadToEnd();
                return str;
            }
        }

        public static async Task<string> GetLatestVersion()
        {
            var client = new HttpClient();
            var res = await client.GetAsync("https://stable-service.huix.cc/update/huidesktop/latest.html");
            if (res.IsSuccessStatusCode) return (await res.Content.ReadAsStringAsync()).Trim();
            return string.Empty;
        }
    }
}
