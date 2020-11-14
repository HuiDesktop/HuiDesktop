using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HuiDesktop.Package
{
    public class PackageManager
    {
        public static Dictionary<string, IPackage> packages = new Dictionary<string, IPackage>();

        public static IPackage LoadPackage(string filename)
        {
            var fs = File.OpenRead(filename);
            var s = new BinaryFileHelper(fs);
            try
            {
                if (s.ReadString() != "HuiDesktopPackage")
                    throw new InvalidDataException();
                var version = s.ReadByte();
                switch (version)
                {
                    case 3:
                        return new V3Package(fs);
                    case 4:
                        return new V4Package(fs);
                    default:
                        throw new NotSupportedException();
                }
            }
            catch (EndOfStreamException)
            {
                throw new InvalidDataException();
            }
        }

        public static void LoadPackages(string basePath)
        {
            var failed = new List<string>();
            void Load(string dir)
            {
                foreach (var i in Directory.EnumerateFiles(dir))
                {
                    try
                    {
                        var package = LoadPackage(i);
                        packages[package.StrongName] = package;
                    }
                    catch (Exception)
                    {
                        failed.Add(i);
                    }
                }
                foreach (var i in Directory.EnumerateDirectories(dir)) Load(i);
            }
            Load(basePath);
            foreach (var i in packages)
                foreach (var j in i.Value.StartupInfos)
                    foreach (var k in j.dependencies)
                        if (packages.ContainsKey(k) == false)
                            j.isDependencyComplete = false;
        }

        public static void LoadLocalPackages(string basePath)
        {
            foreach (var i in Directory.EnumerateDirectories(basePath))
                if (File.Exists(Path.Combine(i, "package.json")))
                    try
                    {
                        var package = new V4Package(i);
                        packages[package.strongName] = package;
                    }
                    catch(Exception)
                    { }
            foreach (var i in packages)
                foreach (var j in i.Value.StartupInfos)
                    foreach (var k in j.dependencies)
                    {
                        j.isDependencyComplete = true;
                        if (packages.ContainsKey(k) == false)
                            j.isDependencyComplete = false;
                    }
        }
    }
}
