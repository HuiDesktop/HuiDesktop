using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
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
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            VersionLabel.Content = $"version: {UpdateService.Version}";

            object operation = null;
            operation = HuiDesktopProtocolHelper.CheckType(Environment.GetCommandLineArgs());
            if (operation != null)
            {
                if (operation is HuiDesktopProtocolHelper.DownloadPackageRequest downloadPackageRequest)
                {
                    var win = new DownloadPackageDialog(downloadPackageRequest.path, downloadPackageRequest.name);
                    if (win.ShowDialog() == true)
                    {
                        Asset.SandboxManager.LoadSandboxes();
                    }
                }
            }
            LoadSandboxes();
            VersionLabel.Content = UpdateService.Version;
            if (AppConfig.Instance.AutoCheckUpdate)
            {
                UpdateNotifyLabel.Content = "正在检测更新...";
                UpdateService.GetLatestVersion().ContinueWith(task =>
                {
                    if (string.IsNullOrEmpty(task.Result))
                    {
                        Dispatcher.Invoke(() => UpdateNotifyLabel.Content = "更新检测失败");
                    }
                    else if (task.Result == UpdateService.Version)
                    {
                        Dispatcher.Invoke(() => UpdateNotifyLabel.Content = "当前为最新版本");
                    }
                    else
                    {
                        Dispatcher.Invoke(() =>
                        {
                            UpdateNotifyLabel.Content = "点击更新：" + task.Result;
                            UpdateNotifyLabel.MouseDown += (_, __) =>
                            {
                                System.Diagnostics.Process.Start(UpdateService.ViewUpdatePage);
                            };
                        });
                    }
                });
            }
        }

        private void LoadSandboxes()
        {
            SandboxWaterfallViewer.Children.Clear();
            foreach (var i in Asset.SandboxManager.Sandboxes)
                foreach(var j in i.GetLaunchInfos())
            {
                SandboxWaterfallViewer.Children.Add(new SandboxPreview(i, j, LoadSandboxes));
            }
        }

        private void ModuleManageButtonClick(object sender, RoutedEventArgs e)
        {
            new ModuleManagerWindow().ShowDialog();
            Asset.ModuleManager.LoadModules();
            Asset.SandboxManager.LoadSandboxes();
            LoadSandboxes();
        }

        private void AppConfigButtonClicked(object sender, RoutedEventArgs e)
        {
            new AppConfigWindow().ShowDialog();
        }

        private void OpenUserDataFolderClicked(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe", FileSystemManager.BasePath);
        }

        private void Window_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effects = DragDropEffects.Copy;
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            //foreach (var file in e.Data.GetData(DataFormats.FileDrop) as string[])
            //{
            //    try
            //    {
            //        using (var fs = File.OpenRead(file))
            //        using (var zip = new ZipArchive(fs, ZipArchiveMode.Read))
            //        {
            //            if (zip.GetEntry("hdt.desc") is var descEntry)
            //            {
            //                using (var desc = descEntry.Open())
            //                using (var sr = new StreamReader(desc))
            //                {
            //                    if (sr.ReadLine() != "HuiDesktop.NextGen Package")
            //                    {
            //                        Debug.WriteLine("Failed: Special line");
            //                        continue;
            //                    }
            //                    switch (sr.ReadLine())
            //                    {
            //                        case "Sandbox":
            //                            var dialog = new CreateSandboxDialog();
            //                    }
            //                }
            //            }
            //        }
            //    }
            //}
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(UpdateService.ViewUpdatePage);
        }
    }
}
