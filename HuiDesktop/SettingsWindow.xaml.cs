using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
            TextBox_FrameRate.Text = GlobalSettings.FrameRate.ToString();
            CheckBox_DisableBlackList.IsChecked = GlobalSettings.DisableBlackList;
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

        private void TextBox_FrameRate_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_setup) return;
            (sender as TextBox).BorderThickness = new Thickness(0, 0, 0, 1);
        }

        private void Button_SaveFrameRate_Click(object sender, RoutedEventArgs e)
        {
            int v;
            try
            {
                v = Convert.ToInt32(TextBox_FrameRate.Text);
                if (v < 10 || v > 60) v = 60;
            }
            catch (Exception)
            {
                v = 60;
            }
            GlobalSettings.FrameRate = v;
            TextBox_FrameRate.Text = v.ToString();
            TextBox_FrameRate.BorderThickness = new Thickness(1);
        }

        private void CheckBox_DisableBlackList_Checked(object sender, RoutedEventArgs e)
        {
            if (_setup) return;
            GlobalSettings.DisableBlackList = (sender as CheckBox).IsChecked == null ? false : ((bool)(sender as CheckBox).IsChecked);
        }
    }
}
