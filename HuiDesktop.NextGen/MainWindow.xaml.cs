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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HuiDesktop.NextGen
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            LoadSandboxes();
        }

        private void LoadSandboxes()
        {
            SandboxWaterfallViewer.Children.Clear();
            foreach (var i in SandboxManager.SandboxDictionary)
            {
                SandboxWaterfallViewer.Children.Add(new SandboxPreview(i.Value, LoadSandboxes));
            }
        }

        private void CreateSandboxButtonClick(object sender, RoutedEventArgs e)
        {
            var win = new CreateSandboxDialog();
            win.ShowDialog();
            if (win.DialogResult == true && !string.IsNullOrEmpty(win.SandboxName))
            {
                new SandboxManageWindow(win.SandboxName).ShowDialog();
                SandboxManager.LoadSandboxes();
                LoadSandboxes();
            }
        }

        private void ModuleManageButtonClick(object sender, RoutedEventArgs e)
        {
            new ModuleManagerWindow().ShowDialog();
            ModuleManager.LoadModules();
            SandboxManager.LoadSandboxes();
            LoadSandboxes();
        }
    }
}
