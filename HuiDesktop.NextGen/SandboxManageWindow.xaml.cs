using System;
using System.Collections.Generic;
using System.IO;
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
using Path = System.IO.Path;

namespace HuiDesktop.NextGen
{
    /// <summary>
    /// CreateSandbox.xaml 的交互逻辑
    /// </summary>
    public partial class SandboxManageWindow : Window
    {
        private readonly string name, basePath, filesPath;
        private readonly Dictionary<Guid, ModuleForList> mainModules = new Dictionary<Guid, ModuleForList>();

        private string MainModuleFile => Path.Combine(basePath, "mainModule");
        private string ConfigFile => Path.Combine(basePath, "config");

        public SandboxManageWindow(string name)
        {
            this.name = name;
            InitializeComponent();
            LoadMainModules();
            SandboxNameLabel.Content = name;
            basePath = Path.Combine(FileSystemManager.SandboxPath, name);
            filesPath = Path.Combine(basePath, "files");
            if (!Directory.Exists(basePath)) Directory.CreateDirectory(basePath);
            if (!Directory.Exists(filesPath)) Directory.CreateDirectory(filesPath);
            if (File.Exists(MainModuleFile))
            {
                string guidStr = File.ReadAllText(MainModuleFile);
                if (Guid.TryParse(guidStr, out var guid))
                {
                    if (mainModules.TryGetValue(guid, out var val))
                    {
                        MainModulesComboBox.SelectedItem = val;
                    }
                    else
                    {
                        FailedToLoadBadge.Visibility = Visibility.Visible;
                        var stackPanel = new StackPanel();
                        stackPanel.Children.Add(new TextBlock() { Text = "无法加载启动模块" });
                        stackPanel.Children.Add(new TextBlock() { Text = "GUID:" + guidStr });
                        FailedToLoadBadge.ToolTip = new ToolTip
                        {
                            Content = stackPanel
                        };
                    }
                }
            }
            if (File.Exists(ConfigFile))
            {
                Editor.Text = File.ReadAllText(ConfigFile, Encoding.UTF8);
            }
        }

        private void SaveButtonClicked(object sender, RoutedEventArgs e)
        {
            if (!(MainModulesComboBox.SelectedItem is var selectedModule) || !(selectedModule is ModuleForList))
            {
                MainModulesComboBox.BorderBrush = new SolidColorBrush(Color.FromRgb(255, 0, 0));
                return;
            }
            File.WriteAllText(MainModuleFile, (selectedModule as ModuleForList).module.Guid.ToString());
            File.WriteAllText(ConfigFile, Editor.Text, Encoding.UTF8);
            DialogResult = true;
        }

        private void MainModulesComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MainModulesComboBox.BorderBrush = new SolidColorBrush(Color.FromRgb(211, 211, 211));
        }

        private void LoadMainModules()
        {
            var list = new List<ModuleForList>();
            foreach (var i in ModuleManager.ModuleDictionary)
            {
                var module = new ModuleForList(i.Value);
                list.Add(module);
                mainModules.Add(i.Key, module);
            }
            MainModulesComboBox.ItemsSource = list;
        }

        private void OpenFolderButtonClicked(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe", filesPath);
        }
    }
}
