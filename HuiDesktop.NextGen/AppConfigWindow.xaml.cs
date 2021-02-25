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

        private void CheckUpdateButtonClicked(object sender, RoutedEventArgs e)
        {

        }
    }
}
