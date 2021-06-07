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
        public class DownloadPackageRequest
        {
            readonly public string path;
            readonly public string name;

            public DownloadPackageRequest(string path, string name)
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
                case "download-package":
                    if (uri.AbsolutePath.Length == 1)
                    {
                        throw new FormatException("Path argument is not given for download-package");
                    }
                    if (uri.Query.Length <= 1)
                    {
                        throw new FormatException("Name argument is not given for download-package");
                    }
                    return new DownloadPackageRequest(uri.AbsolutePath.Substring(1), Uri.UnescapeDataString(uri.Query.Substring(1)));
                default:
                    return null;
            }
        }

        static public void UnzipTo(string src, string dest)
        {
            var d = Path.GetTempFileName();
            File.Delete(d);
            Directory.CreateDirectory(d);
            ZipFile.ExtractToDirectory(src, d);
            var sub = Directory.GetDirectories(d);
            if (sub.Length == 1)
            {
                Copy(sub[0], dest.TrimEnd('/', '\\'));
            }
        }

        static private void Copy(string src, string dest)
        {
            foreach (var i in Directory.EnumerateDirectories(src))
            {
                Directory.CreateDirectory(dest + i.Substring(src.Length));
                Copy(i, dest + i.Substring(src.Length));
            }
            foreach (var i in Directory.EnumerateFiles(src))
            {
                File.Copy(i, dest + i.Substring(src.Length));
            }
        }
    }

    static class PackageDownloadManager
    {
        static Lazy<HttpClient> httpClient = new Lazy<HttpClient>();

        public static async Task<string> DownloadPackage(string path, CancellationToken cancellationToken, IProgress<(int, int)> progress)
        {
            var client = httpClient.Value;
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
