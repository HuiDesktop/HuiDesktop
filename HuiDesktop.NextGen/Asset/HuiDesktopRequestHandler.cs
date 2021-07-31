using CefSharp;
using System;
using System.Collections.Generic;
using System.IO;

namespace HuiDesktop.NextGen.Asset
{
    public class HuiDesktopRequestHandler : CefSharp.Handler.RequestHandler
    {
        public const string URL_HEAD = "https://huidesktop";
        public const string SANDBOX_HEAD = URL_HEAD + "/sandbox/";
        public const string MODULE_ROOT_HEAD = URL_HEAD + "/module/";
        public const string MODULE_STORAGE_HEAD = URL_HEAD + "/storage/";

        NotFoundResourceRequestHandler notFoundResourceRequestHandler = new NotFoundResourceRequestHandler();
        Dictionary<Guid, string> modules = new Dictionary<Guid, string>();
        readonly string sandboxPath;

        public HuiDesktopRequestHandler(Sandbox sandbox)
        {
            if (sandbox.CheckDependencies() != Guid.Empty) throw new ArgumentException("Sandbox should be dependency-satisified.");
            sandboxPath = sandbox.BasePath;
            if (!sandboxPath.EndsWith("/") && !sandboxPath.EndsWith("\\")) sandboxPath += '\\';
            foreach (var i in sandbox.Dependencies)
            {
                var m = ModuleManager.GetModule(i);
                if (m == null) throw new Exception("Failed to load a module, maybe it has been detached.");
                modules[i] = Path.Combine(m.BasePath, "Root") + '\\';
                Directory.CreateDirectory(sandboxPath + i.ToString());
            }
        }

        protected override IResourceRequestHandler GetResourceRequestHandler(IWebBrowser wb, IBrowser b, IFrame f, IRequest request, bool n, bool d, string i, ref bool df)
        {
            if (!request.Url.StartsWith(URL_HEAD, StringComparison.OrdinalIgnoreCase))
            {
                return base.GetResourceRequestHandler(wb, b, f, request, n, d, i, ref df);
            }
            if (request.Url.StartsWith(SANDBOX_HEAD))
            {
                return new FileSystemResourceRequestHandler(sandboxPath + request.Url.Substring(SANDBOX_HEAD.Length), true);
            }
            if (request.Url.StartsWith(MODULE_ROOT_HEAD))
            {
                return new FileSystemResourceRequestHandler(sandboxPath + request.Url.Substring(MODULE_ROOT_HEAD.Length), false);
            }
            if (request.Url.StartsWith(MODULE_STORAGE_HEAD))
            {
                return new FileSystemResourceRequestHandler(sandboxPath + request.Url.Substring(MODULE_STORAGE_HEAD.Length), true);
            }
            return notFoundResourceRequestHandler;
        }
    }

    class NotFoundResourceRequestHandler : CefSharp.Handler.ResourceRequestHandler
    {
        protected override IResourceHandler GetResourceHandler(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request)
        {
            return ResourceHandler.ForErrorMessage("Not found", System.Net.HttpStatusCode.NotFound);
        }
    }

    class FileSystemResourceRequestHandler : CefSharp.Handler.ResourceRequestHandler
    {
        private readonly string file;
        private readonly bool allowWrite; //TODO:

        public FileSystemResourceRequestHandler(string file, bool allowWrite)
        {
            if (!file.EndsWith("/") && !file.EndsWith("\\"))
            {
                file += '\\';
            }
            this.file = file;
            this.allowWrite = allowWrite;
        }

        protected override IResourceHandler GetResourceHandler(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request)
        {
            try
            {
                var s = File.OpenRead(file);
                return ResourceHandler.FromStream(s, System.Web.MimeMapping.GetMimeMapping(file), true); //TODO: can replace MimeMapping?
            }
            catch
            {
                return ResourceHandler.ForErrorMessage("Not found", System.Net.HttpStatusCode.NotFound);
            }
        }
    }
}
