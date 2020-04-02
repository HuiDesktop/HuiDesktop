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

namespace HuiDesktop
{
    /// <summary>
    /// SettingsWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SettingsWindow : Window
    {
        bool _setup = true;
        CefSharp.Wpf.ChromiumWebBrowser browser;

        public SettingsWindow()
        {
            InitializeComponent();
            CheckBox_AutoRun.IsChecked = GlobalSettings.AutoRun;
            CheckBox_AutoCheckUpdate.IsChecked = GlobalSettings.AutoCheckUpdate;
            TextBox_AutoRun.Text = GlobalSettings.AutoRunItem;
            if (string.IsNullOrEmpty(TextBox_AutoRun.Text)) TextBox_AutoRun.Text = "(暂未设置)";
            _setup = false;
        }

        private void CheckBox_AutoRun_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (_setup) return;
            GlobalSettings.AutoRun = CheckBox_AutoRun.IsChecked == null ? false : (bool)CheckBox_AutoRun.IsChecked;
        }

        private async void Button_CheckUpdate_Click(object sender, RoutedEventArgs e)
        {
            var res = ServiceConnection.GetUpdate();
            Button_CheckUpdate.IsEnabled = false;
            Button_CheckUpdate.Content = "正在检测更新...";
            if (await res == "Error")
            {
                Button_CheckUpdate.IsEnabled = true;
                Button_CheckUpdate.Content = "加载出错";
            }
            else if (await res == "Latest")
            {
                Button_CheckUpdate.Content = "最新版本";
            }
            else
            {
                if (browser == null)
                {
                    browser = new CefSharp.Wpf.ChromiumWebBrowser();
                    Grid_Web.Children.Add(browser);
                }
                browser.Address = await res;
                Button_CheckUpdate.IsEnabled = true;
                Button_CheckUpdate.Content = "检测更新";
            }
        }

        private void CheckBox_AutoUpdate_Click(object sender, RoutedEventArgs e) => GlobalSettings.AutoCheckUpdate = ((CheckBox)sender).IsChecked == null ? false : (bool)((CheckBox)sender).IsChecked;
    }
}
