using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace HuiDesktop
{
    static class ServiceConnection
    {
        public async static Task<string> GetUpdate()
        {
            try
            {
                var client = new HttpClient();
                client.Timeout = new TimeSpan(0, 0, 5);
                var res = await client.GetAsync($"https://desktop.huix.cc/backend/software/{ApplicationInfo.Version}/status");
                if (res.StatusCode == System.Net.HttpStatusCode.OK) return await res.Content.ReadAsStringAsync();
                return "Error";
            }
            catch (Exception)
            {
                return "Error";
            }
        }
    }
}
