using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace HuiDesktop.NextGen.Asset
{
    /// <summary>
    /// 模块启动信息
    /// 可以不用额外记录一个Module了，好耶
    /// </summary>
    public class ModuleLaunchInfo
    {
        public Module Module { get; }
        public string Url { get; }
        public string Name { get; }

        public ModuleLaunchInfo(Module module, string url, string name)
        {
            Module = module;
            Url = url;
            Name = name;
        }
    }

    /// <summary>
    /// 模块名称
    /// 自带序列化（<c>.ctor</c>）与反序列化（<c>ToString</c>）
    /// </summary>
    public struct ModuleName
    {
        private static readonly Regex NamePattern = new Regex("^[a-z]+(.[a-z0-9]+)+[a-z0-9]+$");

        public string Name { get; }
        public uint Version { get; }

        public ModuleName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Empty name:version", nameof(name));
            int p = name.IndexOf(':');
            if (p == -1) throw new ArgumentException("No version seperator.", nameof(name));
            if (p == 0) throw new ArgumentException("No name(\":version\" like)", nameof(name));
            if (p == name.Length - 1) throw new ArgumentException("No version(\"name:\" like)", nameof(name));
            Name = name.Substring(0, p);
            Version = Convert.ToUInt32(name.Substring(p + 1));
            if (!NamePattern.IsMatch(Name)) throw new ArgumentException("Invalid name", nameof(name));
        }

        public override string ToString()
        {
            return $"{Name}:{Version}";
        }

        public bool CanBeReplaced(ModuleName higher)
        {
            return Name == higher.Name && Version <= higher.Version;
        }

        public class JsonConverter : JsonConverter<ModuleName>
        {
            public override ModuleName ReadJson(JsonReader reader, Type objectType, ModuleName existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                if (reader.Value is string s)
                {
                    return new ModuleName(s);
                }
                throw new FormatException("Module name should be deserialized from a string.");
            }

            public override void WriteJson(JsonWriter writer, ModuleName value, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }
        }

        #region Boring matters
        public override bool Equals(object obj)
        {
            if (obj is ModuleName m)
            {
                return string.Equals(m.Name, Name) && m.Version == Version;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode() ^ Version.GetHashCode();
        }

        public static bool operator ==(ModuleName left, ModuleName right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ModuleName left, ModuleName right)
        {
            return !(left == right);
        }
        #endregion
    }

    public class ModuleSuggestion
    {
        public string Name { get; set; }
        public string Message { get; set; }
    }

    public class Module
    {
        private readonly string[] features;
        private readonly ModuleLaunchInfo[] launchInfos;
        private readonly ModuleLaunchInfo[] setupInfos;
        private readonly ModuleSuggestion[] suggestions;

        public string BasePath { get; }
        public Guid Id { get; }
        public ModuleName Name { get; }
        public string FriendlyName { get; }
        public IEnumerable<string> Features => features;
        public IEnumerable<ModuleLaunchInfo> LaunchInfos => launchInfos;
        public IEnumerable<ModuleLaunchInfo> SetupInfos => setupInfos;
        public IEnumerable<ModuleSuggestion> Suggestions => suggestions;

        private Module(string basePath, Guid id, ModuleName name, string friendlyName, string[] features, ModuleLaunchInfoJson[] launchInfos, ModuleLaunchInfoJson[] setupInfos, ModuleSuggestion[] suggestions)
        {
            BasePath = basePath;
            Id = id;
            Name = name;
            FriendlyName = friendlyName;
            this.features = features ?? Array.Empty<string>();
            this.suggestions = suggestions ?? Array.Empty<ModuleSuggestion>();

            this.launchInfos = new ModuleLaunchInfo[launchInfos.Length];
            for (uint i = 0; i < launchInfos.Length; ++i)
            {
                this.launchInfos[i] = new ModuleLaunchInfo(this, launchInfos[i].Url, launchInfos[i].Name);
            }

            this.setupInfos = new ModuleLaunchInfo[setupInfos.Length];
            for (uint i = 0; i < setupInfos.Length; ++i)
            {
                this.setupInfos[i] = new ModuleLaunchInfo(this, setupInfos[i].Url, setupInfos[i].Name);
            }
        }

        public override string ToString()
        {
            return $"{FriendlyName} ({Name})";
        }

        class ModuleLaunchInfoJson
        {
            public string Url { get; set; }
            public string Name { get; set; }
        }


        class ModuleDeclareJson
        {
            public Guid Id { get; set; }
            [JsonConverter(typeof(ModuleName.JsonConverter))]
            public ModuleName Name { get; set; }
            public string[] Featrues { get; set; }
            public ModuleLaunchInfoJson[] Launch { get; set; }
            public ModuleLaunchInfoJson[] Setup { get; set; }
            public ModuleSuggestion[] Suggestions { get; set; }
        }

        public static Module LoadFromDirectory(string directory)
        {
            var s = JsonConvert.DeserializeObject<ModuleDeclareJson>(File.ReadAllText(Path.Combine(directory, "declare.json")));
            if (s is null) throw new ArgumentException("Failed to deserialize declare.json", nameof(directory));
            if (s.Id == Guid.Empty) throw new FormatException("Module's id should not be empty");
            return new Module(directory, s.Id, s.Name, Path.GetFileName(directory.TrimEnd('/', '\\')), s.Featrues, s.Launch, s.Setup, s.Suggestions);
        }
    }
}
