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
    /// ModuleManagerWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ModuleManagerWindow : Window
    {
        public ModuleManagerWindow()
        {
            InitializeComponent();
            LoadList();
        }

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

        private void LoadList()
        {
            var list = new List<ModuleForList>();
            foreach(var i in ModuleManager.ModuleDictionary)
            {
                list.Add(new ModuleForList(i.Value));
            }
            ModuleListBox.ItemsSource = list;
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!(ModuleListBox.SelectedItem is ModuleForList module))
            {
                MaskGrid.Visibility = Visibility.Visible;
                return;
            }
            MaskGrid.Visibility = Visibility.Hidden;
            ModuleNameTextBox.Text = module.ToString();
            EntryTextBox.Text = module.module.Entry;
            GuidTextBox.Text = module.module.Guid.ToString();
            if (EntryTextBox.Text.StartsWith($"hdt://{GuidTextBox.Text}/", StringComparison.OrdinalIgnoreCase))
            {
                EntryTextBox.Text = "<ModuleRoot>/" + EntryTextBox.Text.Substring($"hdt://{GuidTextBox.Text}/".Length);
            }
        }

        private void RefreshListButtonClicked(object sender, RoutedEventArgs e)
        {
            ModuleManager.LoadModules(FileSystemManager.ModulePath);
            LoadList();
        }

        private void OpenFolderButtonClicked(object sender, RoutedEventArgs e)
        {
            if (!(ModuleListBox.SelectedItem is ModuleForList module))
            {
                MaskGrid.Visibility = Visibility.Visible;
                return;
            }
            System.Diagnostics.Process.Start("explorer.exe", module.module.Path);
        }
    }
}
