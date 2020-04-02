using System;
using System.Collections.Generic;
using System.IO;

namespace HuiDesktop.Package
{
    public interface IPackage
    {
        int PackageVersion { get; }
        string StrongName { get; }
        string FriendlyName { get; }
        string Description { get; }
        List<StartupInfo> StartupInfos { get; }
        Dictionary<string, Stream> Files { get; }
    }

    public interface IExportablePackage : IPackage
    {
        void Export(Stream stream);
    }
}
