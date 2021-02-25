using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CefSharp;

namespace HuiDesktop.NextGen
{
    class NextGenRequestHandler : RequestHandler
    {
        private readonly Sandbox sandbox;

        public NextGenRequestHandler(Sandbox sandbox)
        {
            this.sandbox = sandbox;
        }

        protected override IResourceRequestHandler GetResourceRequestHandler(IWebBrowser chromiumWebBrowser,
                                                                             IBrowser browser,
                                                                             IFrame frame,
                                                                             IRequest request,
                                                                             bool isNavigation,
                                                                             bool isDownload,
                                                                             string requestInitiator,
                                                                             ref bool disableDefaultHandling)
        {
            return request.Url.StartsWith("https://huidesktop", StringComparison.OrdinalIgnoreCase)
                ? new NextGenGetResourceRequestHandler(sandbox)
                : base.GetResourceRequestHandler(chromiumWebBrowser, browser, frame, request, isNavigation, isDownload, requestInitiator, ref disableDefaultHandling);
        }
    }

    class NextGenGetResourceRequestHandler : CefSharp.Handler.ResourceRequestHandler
    {
        private readonly Sandbox sandbox;

        private static IResourceHandler NotFound => ResourceHandler.ForErrorMessage("Not found", System.Net.HttpStatusCode.NotFound);
        private static IResourceHandler BadRequest => ResourceHandler.ForErrorMessage("Bad request", System.Net.HttpStatusCode.BadRequest);

        public NextGenGetResourceRequestHandler(Sandbox sandbox)
        {
            this.sandbox = sandbox;
        }

        protected override IResourceHandler GetResourceHandler(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request)
        {
            if (string.Compare(request.Url, "https://huidesktop/config", ignoreCase: true) == 0 && string.Compare(request.Method, "POST", ignoreCase: true) == 0 && request.PostData.Elements.Count > 0)
            {
                File.WriteAllBytes(Path.Combine(sandbox.Path, "config"), request.PostData.Elements[0].Bytes);
                return ResourceHandler.ForErrorMessage(string.Empty, System.Net.HttpStatusCode.NoContent);
            }
            if (string.Compare(request.Url, "https://huidesktop", ignoreCase: true) == 0) return BadRequest;
            if (string.Compare(request.Method, "GET", ignoreCase: true) != 0) return BadRequest;

            var full = request.Url.Substring("https://huidesktop/".Length);
            var dividerPos = full.IndexOf('/');
            var type = dividerPos == -1 ? full : full.Substring(0, dividerPos);
            var path = dividerPos == -1 ? string.Empty : full.Substring(dividerPos + 1);
            switch (type)
            {
                case "sandbox": return GetSafeResourceHandler(Path.Combine(sandbox.Path, "files"), path);
                case "config": return ResourceHandler.FromFilePath(Path.Combine(sandbox.Path, "config"));
            }
            if (Guid.TryParse(type, out var guid) && guid == sandbox.MainModuleGuid)
            {
                return GetSafeResourceHandler(Path.Combine(sandbox.MainModule.Path, "files"), path);
            }
            return BadRequest;
        }

        private IResourceHandler GetSafeResourceHandler(string main, string path)
        {
            //TODO: 我想这里大概没必要搞
            string realPath = Path.Combine(main, path);
            if (!realPath.StartsWith(main)) return BadRequest;
            if (!File.Exists(realPath)) return NotFound;
            return ResourceHandler.FromFilePath(realPath);
        }
    }
}
