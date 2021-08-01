using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HuiDesktop.NextGen
{
    static class HuiDesktopProtocolHelper
    {
        public class DownloadModuleRequest
        {
            readonly public string path;
            readonly public string name;

            public DownloadModuleRequest(string path, string name)
            {
                this.path = path;
                this.name = name;
            }
        }

        static public object CheckType(string[] args)
        {
            if (args.Length < 3 || args[1] != "--hdt-url")
            {
                return null;
            }
            var uri = new Uri(string.Join(" ", args.Skip(2).ToArray()));
            switch (uri.Host)
            {
                case "download-module":
                    if (uri.AbsolutePath.Length == 1)
                    {
                        throw new FormatException("Path argument is not given for download-module");
                    }
                    if (uri.Query.Length <= 1)
                    {
                        throw new FormatException("Name argument is not given for download-module");
                    }
                    return new DownloadModuleRequest(uri.AbsolutePath.Substring(1), Uri.UnescapeDataString(uri.Query.Substring(1)));
                default:
                    return null;
            }
        }

        static public void UnzipTo(string src, string dest)
        {
            Directory.CreateDirectory(dest);
            ZipFile.ExtractToDirectory(src, dest);
        }
    }

    static class PackageDownloadManager
    {
        class ProgressMessageHandler : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }
        }

        public static async Task<string> DownloadPackage(string path, CancellationToken cancellationToken, IProgress<(int, int)> progress)
        {
            //这实现，有问题
            var client = new HttpClient();
            var response = await client.GetAsync(path, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return string.Empty;
            }
            var tmp = Path.GetTempFileName();
            using (var remote = await response.Content.ReadAsStreamAsync())
            {
                var total = (int)remote.Length;
                var done = 0;
                using (var f = File.OpenWrite(tmp))
                {
                    byte[] buffer = new byte[1024];
                    int length;
                    while((length = await remote.ReadAsync(buffer, 0, 1024, cancellationToken)) != 0)
                    {
                        await f.WriteAsync(buffer, 0, length);
                        done += length;
                        progress.Report((total, done));
                    }
                }
            }
            return tmp;
        }
    }
}
