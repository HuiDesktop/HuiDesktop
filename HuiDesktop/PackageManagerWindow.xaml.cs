using System;
using System.Windows;
using System.Windows.Controls;

namespace HuiDesktop.Package
{
    /// <summary>
    /// PackageManagerWindow.xaml 的交互逻辑
    /// </summary>
    public partial class PackageManagerWindow : Window
    {
        public PackageManagerWindow()
        {
            InitializeComponent();
            ListBox_Packages.ItemsSource = PackageManager.packages.Values;
        }

        private void ListBox_Packages_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((ListBox)sender).SelectedItem is IPackage)
            {
                var selected = ((ListBox)sender).SelectedItem as IPackage;
                TextBox_Package_Version.Text = selected.PackageVersion.ToString();
                TextBox_Package_StrongName.Text = selected.StrongName;
                TextBox_Package_FriendlyName.Text = selected.FriendlyName;
                TextBox_Package_Description.Text = selected.Description;
                ListBox_StartupInfo.ItemsSource = selected.StartupInfos;
            }
        }

        private void Label_PreviewDragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effects = DragDropEffects.Link;
        }

        private void Label_Drop(object sender, DragEventArgs e)
        {
            try
            {
                string s = (e.Data.GetData(DataFormats.FileDrop) as string[])[0];
                if (System.IO.File.Exists(s)) PackageManager.LoadPackage(s);
                ListBox_Packages.ItemsSource = null;
                ListBox_Packages.ItemsSource = PackageManager.packages.Values;
            }
            catch (Exception) { }
        }

        private void ListBox_StartupInfo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((ListBox)sender).SelectedItem is StartupInfo)
            {
                var selected = ((ListBox)sender).SelectedItem as StartupInfo;
                TextBox_StartupInfo_Name.Text = selected.Name;
                TextBox_StartupInfo_Url.Text = selected.Url;
                object[] vs = new object[selected.dependencies.Count];
                for (int i = 0; i < vs.Length; ++i)
                {
                    vs[i] = selected.dependencies[i];
                    if (PackageManager.packages.ContainsKey(vs[i] as string)) vs[i] = PackageManager.packages[vs[i] as string];
                    else vs[i] = "(不存在)" + (vs[i] as string);
                }
                ListBox_StartupInfo_Deps.ItemsSource = vs;
            }
        }

        private void Button_OpenDir_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo("Explorer.exe");
            psi.Arguments = "/e,/root," + ApplicationInfo.RelativePath("packages");
            System.Diagnostics.Process.Start(psi);
        }
    }
}
