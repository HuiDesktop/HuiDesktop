using System;
using System.Collections.Generic;
using System.IO;

namespace HuiDesktop.Package
{
    public class V3Package : IPackage
    {
        Stream stream;

        public string Name { get; private set; }
        public Guid Guid { get; private set; }
        public Dictionary<string, (long, int)> FileIndex { get; } = new Dictionary<string, (long, int)>();
        public List<(Guid, string)> Dependency { get; } = new List<(Guid, string)>();
        public List<string> StartupUrl { get; } = new List<string>();
        public bool IncompleteDependency { get; set; } = false;
        public string Description { get; private set; }

        public int PackageVersion => 3;
        public string StrongName => Guid.ToString();
        public string FriendlyName => Name;
        public List<StartupInfo> StartupInfos { get; private set; } = new List<StartupInfo>();
        public Dictionary<string, Stream> Files { get; private set; } = new Dictionary<string, Stream>();

        public override string ToString() => $"{FriendlyName}({Guid.ToString().Split('-')[4]})(V3)";

        public V3Package(Stream stream)
        {
            this.stream = new MemoryStream();
            using (var gz = new System.IO.Compression.GZipStream(stream, System.IO.Compression.CompressionMode.Decompress)) gz.CopyTo(this.stream);
            this.stream.Position = 0;
            var s = new BinaryFileHelper(this.stream);
            Guid = new Guid(s.ReadFully(16));
            Name = s.ReadString();
            Description = s.ReadString();
            int fileCount = s.ReadUnsignedInt();
            while (fileCount-- > 0)
            {
                string filename = s.ReadString();
                int length = s.ReadUnsignedInt();
                FileIndex[filename] = (this.stream.Position, length);
                var mem = new MemoryStream();
                s.CopyTo(mem, length);
                Files[filename] = mem;
            }
            int depCount = s.ReadUnsignedInt();
            var new_dep = new List<string>();
            while (depCount-- > 0)
            {
                var g = new Guid(s.ReadFully(16));
                Dependency.Add((g, s.ReadString()));
                new_dep.Add(g.ToString());
            }
            int startCount = s.ReadUnsignedInt();
            while (startCount-- > 0)
            {
                string url = s.ReadString();
                StartupUrl.Add(url);
                StartupInfos.Add(new StartupInfo { dependencies = new_dep, name = $"将在下一次更新废弃V3包 {Name}-启动{startCount}", url= url, fromPackage = this });
            }
        }
        //public V3Package(string basePath)
        //{
        //    stream = new MemoryStream();
        //    if (File.Exists(Path.Combine(basePath, "name.txt"))) Name = File.ReadAllText(Path.Combine(basePath, "name.txt")); else Name = "Untitled";
        //    if (File.Exists(Path.Combine(basePath, "guid.txt"))) Guid = Guid.Parse(File.ReadAllText(Path.Combine(basePath, "guid.txt"))); else Guid = Guid.NewGuid();
        //    if (File.Exists(Path.Combine(basePath, "dependency.txt")))
        //    {
        //        var lines = File.ReadAllLines(Path.Combine(basePath, "dependency.txt"));
        //        for (int i = 0; i < lines.Length; i += 2) Dependency.Add((Guid.Parse(lines[i]), lines[i | 1]));
        //    }
        //    if (File.Exists(Path.Combine(basePath, "start.txt"))) StartupUrl = File.ReadAllLines(Path.Combine(basePath, "start.txt")).ToList();
        //    if (File.Exists(Path.Combine(basePath, "description.txt"))) Description = File.ReadAllText(Path.Combine(basePath, "description.txt")); else Description = "null...";
        //    int baseLen = Path.Combine(basePath, "files").Length + 1;
        //    if (Directory.Exists(Path.Combine(basePath, "files")))
        //    {
        //        void AddFile(string file)
        //        {
        //            using (var fs = File.OpenRead(file))
        //            {
        //                FileIndex.Add(file.Substring(baseLen).Replace('\\', '/'), (stream.Position, (int)fs.Length));
        //                fs.CopyTo(stream);
        //            }
        //        }
        //        void Enum(string dir)
        //        {
        //            foreach (var i in Directory.EnumerateDirectories(dir)) Enum(i);
        //            foreach (var i in Directory.EnumerateFiles(dir)) AddFile(i);
        //        }
        //        Enum(Path.Combine(basePath, "files"));
        //    }
        //}
        //
        //public static Stream MakePackage(Guid guid, string name, string description, string basePath, List<string> files, List<IPackage> dependency, List<string> startupUrl)
        //{
        //    var mss = new MemoryStream();
        //    var s = new BinaryFileHelper(mss);
        //    s.WriteString("HuiDesktopPackage");
        //    s.WriteByte(3);
        //    using (var ms = new System.IO.Compression.GZipStream(mss, System.IO.Compression.CompressionLevel.Optimal, true))
        //    {
        //        s = new BinaryFileHelper(ms);
        //        s.WriteFully(guid.ToByteArray());
        //        s.WriteString(name);
        //        s.WriteString(description);
        //        s.WriteUnsignedInt(files.Count);
        //        int baseLength = basePath.Length + 1;
        //        if (basePath.EndsWith("/") || basePath.EndsWith("\\"))
        //            baseLength -= 1;
        //        foreach (var i in files)
        //        {
        //            s.WriteString(i.Substring(baseLength));
        //            using (var fs = File.OpenRead(i))
        //            {
        //                s.WriteUnsignedInt((int)fs.Length);
        //                fs.CopyTo(ms);
        //            }
        //        }
        //        s.WriteUnsignedInt(dependency.Count);
        //        foreach (var i in dependency)
        //        {
        //            s.WriteFully(i.Guid.ToByteArray());
        //            s.WriteString(i.Name);
        //        }
        //        s.WriteUnsignedInt(startupUrl.Count);
        //        foreach (var i in startupUrl)
        //            s.WriteString(i);
        //    }
        //    mss.Position = 0;
        //    return mss;
        //}

        //public Stream GetStream(string filename)
        //{
        //    byte[] buffer = new byte[1024 * 1024];
        //    var ms = new MemoryStream();
        //    stream.Position = FileIndex[filename].Item1;
        //    int wait = FileIndex[filename].Item2;
        //    while (wait > 0)
        //    {
        //        int read = stream.Read(buffer, 0, Math.Min(buffer.Length, wait));
        //        if (read == 0) throw new EndOfStreamException();
        //        wait -= read;
        //        ms.Write(buffer, 0, read);
        //    }
        //    ms.Position = 0;
        //    return ms;
        //}

        //~V3Package() => stream?.Dispose();
    }
}
