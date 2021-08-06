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
        private readonly Action reloadRequest;
        private readonly Action<string> onStart;

        public SandboxPreview(Asset.Sandbox sandbox, Action reloadRequest, Action<string> OnStart)
        {
            InitializeComponent();
            this.sandbox = sandbox;
            this.reloadRequest = reloadRequest;
            onStart = OnStart;
            SandboxName.Text = $"{sandbox.Name}";
            if (sandbox.CheckDependencies() != Guid.Empty)
            {
                FailedToLoadBadge.Visibility = Visibility.Visible;
                var stackPanel = new StackPanel();
                stackPanel.Children.Add(new TextBlock() { Text = "无法加载某个模块" });
                FailedToLoadBadge.ToolTip = new ToolTip
                {
                    Content = stackPanel
                };
            }
            var items = new List<object>(sandbox.GetLaunchInfos());
            items.Add("---");
            //TODO
            //items.AddRange(sandbox.GetSetupLaunchInfos());
            //items.Add("---");
            items.Add("[打开沙盒设置]");
            items.Add("[删除沙盒]");
            StartInfoComboBox.ItemsSource = items;
            StartInfoComboBox.SelectedIndex = -1;
        }

        private void Grid_MouseEnter(object sender, MouseEventArgs e)
        {
            MainGrid.Background = new SolidColorBrush(Color.FromRgb(200, 200, 200));
        }

        private void Grid_MouseLeave(object sender, MouseEventArgs e)
        {
            MainGrid.Background = new SolidColorBrush(Color.FromRgb(238, 238, 238));
        }

        private void Wpf(Asset.ModuleLaunchInfo startInfo)
        {
            sandbox.LaunchWpf(startInfo.Url);
            Application.Current.MainWindow.Close();
        }

        //private void Dc()
        //{
        //    var requestHandler = new NextGenRequestHandler(startInfo);
        //    var start = new DirectComposition.CefApplication(startInfo.MainModule.Entry, requestHandler);
        //    start.Run();
        //}

        private void ManageButtonClicked(object sender, RoutedEventArgs e)
        {
            new SandboxManageWindow(sandbox).ShowDialog();
            Asset.SandboxManager.LoadSandboxes();
            reloadRequest();
        }

        private void StartInfoComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (StartInfoComboBox.SelectedItem == null) return;
            if (StartInfoComboBox.SelectedItem is Asset.ModuleLaunchInfo f)
            {
                if (StartInfoComboBox.SelectedIndex < sandbox.GetLaunchInfos().Count())
                {
                    onStart(f.Url);
                    Wpf(f);
                }
                else
                {
                    MessageBox.Show("暂不支持，请等待更新版本的HuiDesktop!");
                }
            }
            else if (StartInfoComboBox.SelectedItem is string s)
            {
                switch (s)
                {
                    case "[打开沙盒设置]":
                        new SandboxManageWindow(sandbox).ShowDialog();
                        Asset.SandboxManager.LoadSandboxes();
                        reloadRequest();
                        break;
                    case "[删除沙盒]":
                        if (MessageBox.Show("确认吗？此操作不可撤回！", "提示", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                        {
                            sandbox.Remove();
                            Asset.SandboxManager.LoadSandboxes();
                            reloadRequest();
                        }
                        break;
                }
            }
        }
    }
}
