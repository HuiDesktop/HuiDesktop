using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HuiDesktop.NextGen.Pod
{
    public enum PodDependencyLevel
    {
        Required,
        Optional
    }

    public struct PodDependency
    {
        public string FeatureName { get; set; }
        public Guid DependPod { get; set; }
        public PodDependencyLevel DependencyLevel { get; set; }

        public PodDependency(string featureName, Guid dependPod, PodDependencyLevel dependencyLevel)
        {
            FeatureName = featureName;
            DependPod = dependPod;
            DependencyLevel = dependencyLevel;
        }

        public override bool Equals(object obj)
        {
            if (obj is PodDependency d)
                return string.Equals(d.FeatureName, FeatureName) && d.DependPod == DependPod && d.DependencyLevel == DependencyLevel;
            return false;
        }

        public override int GetHashCode()
        {
            return unchecked(FeatureName.GetHashCode() ^ DependPod.GetHashCode() ^ DependencyLevel.GetHashCode());
        }

        public static bool operator ==(PodDependency left, PodDependency right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(PodDependency left, PodDependency right)
        {
            return !(left == right);
        }
    }
}
