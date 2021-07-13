using System;
using System.Text;

namespace HuiDesktop.NextGen.Pod.ManagerGui
{
    public class VisualPod
    {
        public Pod pod;

        public VisualPod(Pod pod)
        {
            this.pod = pod;
        }

        public override string ToString()
        {
            return pod.Name;
        }

        public string GetDetailString() => $"GUID: {pod.Id}\r\nName: {pod.Name}\r\nSetup URL: {pod.SetupUrl}\r\nLaunch URL: {pod.LaunchUrl}\r\nFeatures:\r\n{VisualFeatures}Dependencies:\r\n{VisualDependencies}";

        public string VisualFeatures
        {
            get
            {
                var b = new StringBuilder();
                foreach (var i in pod.Features)
                {
                    b.AppendLine("    " + i);
                }
                return b.ToString();
            }
        }

        public string VisualDependencies
        {
            get
            {
                var b = new StringBuilder();
                foreach (var i in pod.Dependencies)
                {
                    b.AppendLine($"{(i.DependencyLevel == PodDependencyLevel.Optional ? "    [Optional]" : "    [Required]")}{i.FeatureName}={i.DependPod}");
                }
                return b.ToString();
            }
        }
    }

}
