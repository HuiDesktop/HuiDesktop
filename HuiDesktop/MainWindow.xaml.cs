using System;
using System.Collections.Generic;
using System.Globalization;
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

namespace HuiDesktop
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private object start;
        private bool showHiddenItems = false;

        private void RefreshList()
        {
            var info = new List<Package.StartupInfo>();
            if (showHiddenItems) foreach (var i in Package.PackageManager.packages) info.AddRange(i.Value.StartupInfos);
            else foreach (var i in Package.PackageManager.packages) foreach (var j in i.Value.StartupInfos)
                        if (!GlobalSettings.HiddenStrongNames.Contains($"{i.Value.StrongName}|{j.name}")) info.Add(j);
            ListView_Cards.ItemsSource = info;
        }

        public MainWindow()
        {
            Task<string> update = null;
            string result;
            if (GlobalSettings.AutoCheckUpdate) update = ServiceConnection.GetUpdate();
            InitializeComponent();
            if (Environment.GetCommandLineArgs().Length == 2 && Environment.GetCommandLineArgs()[1] == "--autorun")
            {
                string s = GlobalSettings.AutoRunItem;
                if (s != null && s.Contains('|'))
                {
                    var ss = s.Split('|');
                    if (Package.PackageManager.packages.ContainsKey(ss[0]))
                    {
                        var found = from e in Package.PackageManager.packages[ss[0]].StartupInfos
                                    where e.name == ss[1]
                                    select e;
                        if (found.Any())
                        {
                            new BasicWindow(found.First()).Show();
                            if (update != null)
                            {
                                result = update.Result;
                                if (result != "Latest" && result != "Error")
                                {
                                    MessageBox.Show("检测到更新, 请打开主软件->设置界面查看", "HuiDesktop");
                                }
                            }
                        }
                    }
                }
                Close();
                return;
            }
            var info = new List<Package.StartupInfo>();
            RefreshList();
            Task.Factory.StartNew(() =>
            {
                result = update.Result;
                if (result != "Latest" && result != "Error")
                {
                    MessageBox.Show("检测到更新, 请打开主软件->设置界面查看", "HuiDesktop");
                }
            });
        }

        private void ListView_Cards_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var item = e.AddedItems[0] as Package.StartupInfo;
                if (item == null) return;
                if (item.isDependencyComplete == false) Label_Tip.Content = "包依赖不全!";
                else Label_Tip.Content = "双击启动\n右键详细信息(没做)";
            }
        }

        private void ListView_Cards_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if ((sender as ListView).SelectedItems.Count > 0 && start == null)
            {
                var item = (sender as ListView).SelectedItems[0] as Package.StartupInfo;
                if (item.isDependencyComplete == false) Label_Tip.Content = "包依赖不全!";
                else
                {
#if !OLD_RENDERING
                    var requestHandler = new RequestHandler();
                    requestHandler.AddPackage(item.fromPackage);
                    foreach (var i in item.dependencies) requestHandler.AddPackage(Package.PackageManager.packages[i]);
                    start = new DirectComposition.CefApplication(item.url, requestHandler);
                    Close();
                    (start as DirectComposition.CefApplication).Run();
#else
                    start = new BasicWindow(item);
                    (start as BasicWindow).Show();
                    Close();
#endif
                }
            }
        }

        private int rightCount;

        private void Button_DevTools_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            rightCount += 1;
            if (rightCount == 5)
            {
                rightCount = 0;
                new DevelopTools().Show();
            }
        }

        private void Button_Settings_Click(object sender, RoutedEventArgs e)
        {
            new SettingsWindow().ShowDialog();
        }

        private void MenuItem_AutoRun_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem && (sender as MenuItem).CommandParameter is Package.StartupInfo)
            {
                e.Handled = true;
                var info = ((sender as MenuItem).CommandParameter as Package.StartupInfo);
                GlobalSettings.AutoRunItem = info.fromPackage.StrongName + '|' + info.name;
            }
        }

        private void MenuItem_SwitchHide_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem && (sender as MenuItem).CommandParameter is Package.StartupInfo)
            {
                e.Handled = true;
                var info = ((sender as MenuItem).CommandParameter as Package.StartupInfo);
                var str = $"{info.fromPackage.StrongName}|{info.name}";
                if (GlobalSettings.HiddenStrongNames.Contains(str)) GlobalSettings.HiddenStrongNames = GlobalSettings.HiddenStrongNames.Except(new string[] { str }).ToArray();
                else GlobalSettings.HiddenStrongNames = GlobalSettings.HiddenStrongNames.Union(new string[] { str }).ToArray();
                RefreshList();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            showHiddenItems ^= true;
            RefreshList();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            new Package.PackageManagerWindow().ShowDialog();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            new ForumWindow().ShowDialog();
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            new AboutWindow().ShowDialog();
        }

#region 彩蛋

        private int _eg = 0;

        private void Label_Tip_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if ((sender as Label).Content.Equals("QwQ") || (sender as Label).Content.Equals("QAQ") || (sender as Label).Content.Equals("@w@"))
            {
                _eg += 1;
                if (_eg == 10) (sender as Label).Content = "QAQ";
                if (_eg == 30) (sender as Label).Content = "@w@";
                if (_eg == 50) (sender as Label).Content = "...";
            }
        }

#endregion
    }

    public class VisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool val = (bool)value;
            return val ? Visibility.Visible : Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class CardColorConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is Package.StartupInfo)) return Brushes.Black;
            var v = value as Package.StartupInfo;
            return GlobalSettings.HiddenStrongNames.Contains($"{v.fromPackage.StrongName}|{v.name}") ? Brushes.Gray : 
                (v.fromPackage is Package.V4Package && (v.fromPackage as Package.V4Package).isLocalPackage ? Brushes.Red : Brushes.Black);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
