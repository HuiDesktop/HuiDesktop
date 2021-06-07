using System;
using System.Collections.Generic;
using System.Configuration;
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
    /// AppConfigWindow.xaml 的交互逻辑
    /// </summary>
    public partial class AppConfigWindow : Window
    {
        public AppConfigWindow()
        {
            DataContext = AppConfig.Instance;
            InitializeComponent();
        }

        private void SaveButtonClicked(object sender, RoutedEventArgs e)
        {
            AppConfig.Instance.Save();
            DialogResult = true;
        }

        async private void CheckUpdateButtonClicked(object sender, RoutedEventArgs e)
        {
            CheckUpdateButton.IsEnabled = false;
            UpdateInfoLabel.Content = "检查中...";
            try
            {
                var r = await UpdateService.GetLatestVersion();
                if (!string.IsNullOrEmpty(r) && r != UpdateService.Version)
                {
                    UpdateInfoLabel.Content = "检测到最新版，正在打开浏览器";
                    System.Diagnostics.Process.Start(UpdateService.ViewUpdatePage);
                }
                else if (r == UpdateService.Version)
                {
                    UpdateInfoLabel.Content = "是最新版，好耶ヾ(✿ﾟ▽ﾟ)ノ";
                }
                else
                {
                    UpdateInfoLabel.Content = "检查失败";
                }
            }
            catch
            {
                
            }
            finally
            {
                CheckUpdateButton.IsEnabled = true;
            }
        }
    }
}
