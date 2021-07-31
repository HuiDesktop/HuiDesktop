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
    /// SandboxPreview.xaml 的交互逻辑
    /// </summary>
    public partial class SandboxPreview : UserControl
    {
        private readonly Asset.Sandbox sandbox;
        private readonly Asset.ModuleLaunchInfo startInfo;
        private readonly Action reloadRequest;

        public SandboxPreview(Asset.Sandbox sandbox, Asset.ModuleLaunchInfo startInfo, Action reloadRequest)
        {
            InitializeComponent();
            this.sandbox = sandbox;
            this.startInfo = startInfo;
            this.reloadRequest = reloadRequest;
            SandboxName.Text = $"{sandbox.Name}\r\n{startInfo.Name}";
            if (sandbox.CheckDependencies() != Guid.Empty)
            {
                RunButton.IsEnabled = false;
                FailedToLoadBadge.Visibility = Visibility.Visible;
                var stackPanel = new StackPanel();
                stackPanel.Children.Add(new TextBlock() { Text = "无法加载某个模块" });
                FailedToLoadBadge.ToolTip = new ToolTip
                {
                    Content = stackPanel
                };
            }
        }

        private void Grid_MouseEnter(object sender, MouseEventArgs e)
        {
            MainGrid.Background = new SolidColorBrush(Color.FromRgb(200, 200, 200));
        }

        private void Grid_MouseLeave(object sender, MouseEventArgs e)
        {
            MainGrid.Background = new SolidColorBrush(Color.FromRgb(238, 238, 238));
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void RunButtonClicked(object sender, RoutedEventArgs e)
        {
            if (sandbox.CheckDependencies() == Guid.Empty)
            {
                Wpf();
                Application.Current.MainWindow.Close();
            }
        }

        private void Wpf()
        {
            var win = new BasicWindow(new Asset.HuiDesktopRequestHandler(sandbox), startInfo.Url, AppConfig.Instance.ForceWebGL);
            win.Show();
        }

        //private void Dc()
        //{
        //    var requestHandler = new NextGenRequestHandler(startInfo);
        //    var start = new DirectComposition.CefApplication(startInfo.MainModule.Entry, requestHandler);
        //    start.Run();
        //}

        private void ManageButtonClicked(object sender, RoutedEventArgs e)
        {
            //new SandboxManageWindow(startInfo.SandboxName).ShowDialog();
            //SandboxManager.LoadSandboxes();
            //reloadRequest();
        }
    }
}
