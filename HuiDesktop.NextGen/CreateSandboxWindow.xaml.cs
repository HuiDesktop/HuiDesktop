using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace HuiDesktop.NextGen
{
    /// <summary>
    /// CreateSandbox.xaml 的交互逻辑
    /// </summary>
    public partial class CreateSandboxWindow : Window
    {
        public CreateSandboxWindow()
        {
            InitializeComponent();
            StartupModules.ItemsSource = new List<string> { "Browser模式（HTTP）", "Browser模式（本地文件）" };
        }
    }
}
