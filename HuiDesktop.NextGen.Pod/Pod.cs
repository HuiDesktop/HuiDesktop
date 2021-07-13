using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HuiDesktop.NextGen.Pod
{
    public class Pod
    {
        /// <summary>
        /// 此Pod的ID
        /// </summary>
        public Guid Id { get; }
        /// <summary>
        /// 此Pod的名称
        /// 此名称非唯一标识符
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// 配置URL（可空）
        /// 使用此URL启动不会关闭HuiDesktop窗口，且将使用普通窗口启动，用于配置此Pod
        /// </summary>
        public string SetupUrl { get; }
        /// <summary>
        /// 启动URL（可空）
        /// 使用此URL启动将关闭HuiDesktop窗口，且将使用透明窗口启动，用于实际启动
        /// </summary>
        public string LaunchUrl { get; }
        /// <summary>
        /// 此Pod在文件系统的根
        /// </summary>
        public string Root { get; }
        /// <summary>
        /// 此Pod实现的feature
        /// CA1819:属性不应返回数组
        /// </summary>
        private HashSet<string> features;
        /// <summary>
        /// 此Pod的依赖
        /// CA1819:属性不应返回数组
        /// </summary>
        private PodDependency[] dependencies;

        /// <summary>
        /// 查询Feature是否被此Pod实现
        /// </summary>
        /// <param name="featureName">feature名称</param>
        /// <returns>此Pod是否指定feature</returns>
        public bool IsFeatureImplemented(string featureName) => features.Contains(featureName);

        /// <summary>
        /// 便于枚举此Pod实现的feature
        /// </summary>
        public IEnumerator<string> Features => features.GetEnumerator();

        /// <summary>
        /// 便于枚举此Pod的依赖
        /// </summary>
        public IEnumerator<PodDependency> Dependencies => dependencies.AsEnumerable().GetEnumerator();

        public Pod(Guid id, string name, string setupUrl, string launchUrl, string root, string[] features, PodDependency[] dependencies)
        {
            Id = id;
            Name = name;
            SetupUrl = setupUrl;
            LaunchUrl = launchUrl;
            Root = root;
            this.features = new HashSet<string>(features);
            this.dependencies = dependencies;
        }

        private class PodJson
        {
            public Guid id;
            public string name;
            public string setup;
            public string launch;
            public string[] features;
            public string[] dependencies;
        }

        /// <summary>
        /// 从文件夹加载Pod
        /// </summary>
        /// <param name="podRootDirectory">文件夹根</param>
        /// <returns>以此根为Root的Pod</returns>
        public static Pod LoadPodFromDirectory(string podRootDirectory)
        {
            var path = Path.Combine(podRootDirectory, "pod.json");
            var r = JsonConvert.DeserializeObject<PodJson>(File.ReadAllText(path));
            if (r != null)
            {
                try
                {
                    PodDependency[] dep;
                    var depStrs = r.dependencies;
                    if (depStrs != null)
                    {
                        dep = new PodDependency[depStrs.Length];
                        for (int i = 0; i < dep.Length; ++i)
                        {
                            var divided = depStrs[i].Split('=');
                            if (divided.Length != 2) throw new FormatException("Cannot format dependency string: " + depStrs[i]);
                            if (!Guid.TryParse(divided[1], out var depGuid)) throw new FormatException("Cannot format depend pod's GUID: " + divided[1]);
                            string featureName = divided[0];
                            bool isOptional = featureName.EndsWith("?");
                            if (isOptional) featureName = featureName.Substring(0, featureName.Length - 1);
                            dep[i] = new PodDependency(featureName, depGuid, isOptional ? PodDependencyLevel.Optional : PodDependencyLevel.Required);
                        }
                    }
                    else
                    {
                        throw new FormatException("Cannot find dep property");
                    }
                    if (r.features == null)
                    {
                        r.features = Array.Empty<string>();
                    }
                    return new Pod(r.id, r.name, r.setup, r.launch, podRootDirectory, r.features, dep);
                }
                catch (Exception e)
                {
                    throw new FormatException("Failed to format pod.json", e);
                }
            }
            throw new FileNotFoundException("Cannot find pod.json", path);
        }
    }
}
