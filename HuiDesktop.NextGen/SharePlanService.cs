using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace HuiDesktop.NextGen
{
    static class SharePlanService
    {
        static Lazy<Guid> session = new Lazy<Guid>(() => Guid.NewGuid());

        public async static Task UploadAtLaunch(IEnumerable<Guid> guids)
        {
            var sb = new StringBuilder();
            foreach (var i in guids)
            {
                sb.Append(i);
                sb.Append(';');
            }
            File.WriteAllLines(Path.Combine(FileSystemManager.SharePlanLogPath, "l " + DateTime.Now.ToString("yyMMdd HHmmss ffff") + ".log"),
                            new string[] { $"SESSION:{session.Value}", "Launch:" + sb.ToString() });
            using (var cli = new HttpClient())
            {
                await cli.GetAsync($"https://desktop.huix.cc/api/stat/online?session={HttpUtility.UrlEncode(session.Value.ToString())}&typ=launch&ids={HttpUtility.UrlEncode(sb.ToString())}");
            }
        }

        public async static Task UploadAtExe()
        {
            var ver = UpdateService.Version;
            var cid = UpdateService.GitCommitId;
            var win = GetOSVersion();
            var cpu = GetCpuInfo();
            var gpu = GetGpuInfo();
            var mem = GetTotalPhysicalMemory().ToString() + '/' + GetAvailablePhysicalMemory().ToString();
            File.WriteAllLines(Path.Combine(FileSystemManager.SharePlanLogPath, "e " + DateTime.Now.ToString("yyMMdd HHmmss ffff") + ".log"),
                new string[] { $"SESSION:{session.Value}", $"VERSION:{ver}({cid})", "WIN:" + win, "CPU:" + cpu, "GPU:" + gpu, "MEM:" + mem });
            var query =
                $"?session={HttpUtility.UrlEncode(session.Value.ToString())}" +
                $"&typ=exe" +
                $"&ver={HttpUtility.UrlEncode(ver)}" +
                $"&cid={HttpUtility.UrlEncode(cid)}" +
                $"&win={HttpUtility.UrlEncode(win)}" +
                $"&cpu={HttpUtility.UrlEncode(cpu)}" +
                $"&gpu={HttpUtility.UrlEncode(gpu)}" +
                $"&mem={HttpUtility.UrlEncode(mem)}";
            using (var cli = new HttpClient())
            {
                await cli.GetAsync("https://desktop.huix.cc/api/stat/online" + query);
            }
        }

        private static string GetCpuInfo()
        {
            try
            {
                var r = "";
                using (var mos = new ManagementClass("Win32_Processor").GetInstances())
                {
                    foreach (var mo in mos)
                    {
                        r += mo["Name"].ToString() + ';';
                    }
                }
                return r;
            }
            catch
            {
                return "";
            }
        }

        private static string GetGpuInfo()
        {
            try
            {
                var r = "";
                using (var mos = new ManagementClass("Win32_VideoController").GetInstances())
                {
                    foreach (var mo in mos)
                    {
                        r += mo["Name"].ToString() + ';';
                    }
                }
                return r;
            }
            catch
            {
                return "";
            }
        }

        private static long GetTotalPhysicalMemory()
        {
            try
            {
                long capacity = 0;
                using (var mos = new ManagementClass("Win32_PhysicalMemory").GetInstances())
                {
                    foreach (var mo in mos)
                    {
                        capacity += long.Parse(mo.Properties["Capacity"].Value.ToString());
                    }
                }
                return capacity;
            }
            catch
            {
                return -1;
            }
        }

        private static long GetAvailablePhysicalMemory()
        {
            try
            {
                long capacity = 0;
                using (var mos = new ManagementClass("Win32_PerfFormattedData_PerfOS_Memory").GetInstances())
                {
                    foreach (var mo in mos)
                    {
                        capacity += long.Parse(mo.Properties["AvailableBytes"].Value.ToString());
                    }
                }
                return capacity;
            }
            catch
            {
                return -1;
            }
        }

        private static string GetOSVersion()
        {
            try
            {
                var version = "";
                using (var mos = new ManagementClass("Win32_OperatingSystem").GetInstances())
                {
                    foreach (var mo in mos)
                    {
                        version += mo.Properties["Version"].Value.ToString();
                    }
                }
                return version;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
