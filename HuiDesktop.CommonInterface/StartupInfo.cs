using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HuiDesktop.Package
{
    public class StartupInfo
    {
        public IPackage fromPackage;
        public string name;
        public string url;
        public List<string> dependencies;
        public bool isDependencyComplete = true;

        public string Name => name;
        public string Url => url;
        public override string ToString() => name;
    }
}
