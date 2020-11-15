using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace HuiDesktop
{
    public static class ServiceConnection
    {
        public async static Task<string> GetUpdate()
        {
            return "Latest";
            var client = new HttpClient();
            var res = await client.GetAsync("https://desktop.huix.cc/api/update/1.0.0.0/check");
            if (res.StatusCode == System.Net.HttpStatusCode.OK) return await res.Content.ReadAsStringAsync();
            return "Error";
        }
    }
}
