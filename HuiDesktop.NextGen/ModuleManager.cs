using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HuiDesktop.NextGen
{

    [Obsolete]
    public class Module
    {
        public Guid Guid { get; }
        public string Name { get; }
        public string FriendlyName { get; }
        public string Entry { get; set; }
        public string Path { get; }

        public Module(Guid guid, string name, string friendlyName, string entry, string path)
        {
            Guid = guid;
            Name = name;
            FriendlyName = friendlyName;
            Entry = entry;
            Path = path;
        }
    }

    [Obsolete]
    class ModuleForList
    {
        public readonly Module module;

        public ModuleForList(Module module)
        {
            this.module = module;
        }

        public override string ToString()
        {
            return $"{module.FriendlyName}({module.Name})";
        }
    }

    [Obsolete]
    public static class ModuleManager
    {
        public static Dictionary<Guid, Module> ModuleDictionary { get; private set; } = new Dictionary<Guid, Module>();

        sealed class Logger : IDisposable
        {
            readonly FileStream stream;
            readonly StreamWriter writer;
            private bool disposedValue;

            public Logger(string basePath)
            {
                stream = File.Open(Path.Combine(basePath, "load.log"), FileMode.Append);
                writer = new StreamWriter(stream, Encoding.UTF8);
                writer.WriteLine("\nStart loading modules @ " + DateTime.Now.ToString("u"));
            }

            private void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        writer?.Dispose();
                        stream?.Dispose();
                    }

                    disposedValue = true;
                }
            }

            public void Dispose()
            {
                Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }

            public void WriteLine(string line)
            {
                writer.WriteLine(line);
            }
        }

        public static void LoadModules()
        {
            ModuleDictionary = new Dictionary<Guid, Module>();
            var logger = new Logger(FileSystemManager.ModulePath);
            void TryLoadModule(string path)
            {
                var configFile = Path.Combine(path, "config.json");
                if (File.Exists(configFile))
                {
                    var root = JObject.Parse(File.ReadAllText(configFile));
                    if (root == null)
                    {
                        logger.WriteLine("Failed to load module from " + path + " (Failed to parse config file)");
                        return;
                    }

                    var formatVersionToken = root["formatVersion"];
                    var guidToken = root["guid"];
                    var nameToken = root["name"];
                    var friendlyNameToken = root["friendlyName"];
                    var entryToken = root["entry"];
                    string name, friendlyName, entry;
                    Guid guid;

                    if (formatVersionToken == null || formatVersionToken.Type != JTokenType.Integer || formatVersionToken.Value<int>() != 5)
                    {
                        logger.WriteLine("Failed to load module from " + path + " (Invalid format version)");
                        return;
                    }

                    if (guidToken == null || guidToken.Type != JTokenType.String)
                    {
                        logger.WriteLine("Failed to load module from " + path + " (Invalid GUID)");
                        return;
                    }
                    if (!Guid.TryParse(guidToken.Value<string>(), out guid) || ModuleDictionary.ContainsKey(guid))
                    {
                        logger.WriteLine("Failed to load module from " + path + " (GUID Exists)");
                        return;
                    }

                    if (nameToken == null || nameToken.Type != JTokenType.String)
                    {
                        logger.WriteLine("Failed to load module from " + path + " (Invalid module name)");
                        return;
                    }
                    name = nameToken.Value<string>();

                    if (friendlyNameToken == null)
                    {
                        logger.WriteLine("Using default friendly name while loading module from " + path);
                        friendlyName = name;
                    }
                    else if (friendlyNameToken.Type == JTokenType.String)
                    {
                        friendlyName = friendlyNameToken.Value<string>();
                    }
                    else
                    {
                        logger.WriteLine("Failed to load module from " + path + " (Invalid friendly name)");
                        return;
                    }

                    if (entryToken == null || entryToken.Type != JTokenType.String)
                    {
                        logger.WriteLine("Failed to load module from " + path + " (Invalid entry)");
                        return;
                    }
                    entry = entryToken.Value<string>();

                    ModuleDictionary.Add(guid, new Module(guid, name, friendlyName, entry, path));
                    logger.WriteLine($"Module from {path} loaded successfully. name={name}, friendlyName={friendlyName}, GUID={guid}");
                }
                else
                {
                    logger.WriteLine("Failed to load module from " + path + " (Config file not found)");
                    return;
                }
            }
            try
            {
                if (Directory.Exists(FileSystemManager.ModulePath))
                {
                    foreach (var path in Directory.EnumerateDirectories(FileSystemManager.ModulePath))
                    {
                        TryLoadModule(path);
                    }
                }
                else
                {
                    Console.WriteLine($"{FileSystemManager.ModulePath} not exists.");
                }
            }
            catch (Exception e)
            {
                logger.WriteLine(e.ToString());
            }
            finally
            {
                logger.Dispose();
            }
        }
    }
}
