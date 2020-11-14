using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HuiDesktop.Package
{
    public class V4Package : IExportablePackage
    {
        public string strongName;
        public string friendlyName;
        public string description;
        public List<StartupInfo> startupInfos = new List<StartupInfo>();
        public Dictionary<string, Stream> files = new Dictionary<string, Stream>();
        public bool isLocalPackage = false;

        #region Interface Implmentation
        public int PackageVersion => 4;
        public string StrongName => strongName;
        public string FriendlyName => friendlyName;
        public string Description => description;
        public List<StartupInfo> StartupInfos => startupInfos;
        public Dictionary<string, Stream> Files => files;
        public override string ToString() => $"{friendlyName}({strongName})(V4)";
        #endregion

        public V4Package(string folder)
        {
            isLocalPackage = true;
            if (Directory.Exists(folder) == false) throw new DirectoryNotFoundException(folder);
            if (File.Exists(Path.Combine(folder, "package.json")) == false) throw new FileNotFoundException("package.json");
            var root = JsonConvert.DeserializeObject<JObject>(File.ReadAllText(Path.Combine(folder, "package.json")));
            if (root == null) throw new Exception("package.json");
            strongName = root.Value<string>("strongName");
            friendlyName = root.Value<string>("friendlyName");
            description = root.Value<string>("description");
            var starts = root.Value<JArray>("startInfo");
            foreach (JObject start in starts)
            {
                var deps = new List<string>();
                foreach (string i in start.Value<JArray>("dependencies")) deps.Add(i);
                startupInfos.Add(new StartupInfo
                {
                    dependencies = deps,
                    name = start.Value<string>("name"),
                    url = start.Value<string>("url"),
                    fromPackage = this
                });
            }
            int indexLength = Path.Combine(folder, "files").Length + 1;
            void AddFile(string file)
            {
                file = file.Replace('\\', '/');
                files[file.Substring(indexLength)] = File.OpenRead(file);
            }
            void EnumDir(string dir)
            {
                foreach(var i in Directory.EnumerateDirectories(dir)) EnumDir(i);
                foreach(var i in Directory.EnumerateFiles(dir)) AddFile(i);
            }
            EnumDir(Path.Combine(folder, "files"));
        }

        public V4Package(Stream stream)
        {
            var reader = new BinaryFileHelper(new System.IO.Compression.DeflateStream(stream, System.IO.Compression.CompressionMode.Decompress));
            //var reader = new BinaryFileHelper(stream);
            strongName = reader.ReadString();
            friendlyName = reader.ReadString();
            description = reader.ReadString();

            for (int i = reader.ReadUnsignedInt(); i > 0; --i)
            {
                var name = reader.ReadString();
                var url = reader.ReadString();
                var d = new List<string>();
                for (int ii = reader.ReadUnsignedInt(); ii > 0; --ii) d.Add(reader.ReadString());
                startupInfos.Add(new StartupInfo
                {
                    dependencies = d,
                    name = name,
                    url = url,
                    fromPackage = this
                });
            }

            for (int i = reader.ReadUnsignedInt(); i > 0; --i)
            {
                string name = reader.ReadString();
                int length = reader.ReadUnsignedInt();
                var mem = new MemoryStream();
                reader.CopyTo(mem, length);
                files[name] = mem;
            }
        }

        public void Export(Stream stream)
        {
            var writer = new BinaryFileHelper(stream);
            writer.WriteString("HuiDesktopPackage");
            writer.WriteByte(4);
            //writer = new BinaryFileHelper(new System.IO.Compression.GZipStream(stream, System.IO.Compression.CompressionLevel.Optimal));
            using (writer = new BinaryFileHelper(new System.IO.Compression.DeflateStream(stream, System.IO.Compression.CompressionLevel.Optimal)))
            {
                writer.WriteString(strongName);
                writer.WriteString(friendlyName);
                writer.WriteString(description);

                writer.WriteUnsignedInt(startupInfos.Count);
                foreach (var i in startupInfos)
                {
                    writer.WriteString(i.name);
                    writer.WriteString(i.url);
                    writer.WriteUnsignedInt(i.dependencies.Count);
                    foreach (var ii in i.dependencies) writer.WriteString(ii);
                }

                writer.WriteUnsignedInt(files.Count);
                foreach (var i in files)
                {
                    writer.WriteString(i.Key);
                    writer.WriteUnsignedInt((int)i.Value.Length);
                    i.Value.Position = 0;
                    writer.CopyFrom(i.Value);
                }
            }
        }
    }
}
