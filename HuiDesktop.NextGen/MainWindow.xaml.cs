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
using Path = System.IO.Path;

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

            VersionLabel.Content = $"version: {UpdateService.Version} ({UpdateService.GitCommitId.Substring(0, 6)})";

            object operation = null;
            operation = HuiDesktopProtocolHelper.CheckType(Environment.GetCommandLineArgs());
            if (operation != null)
            {
                if (operation is HuiDesktopProtocolHelper.DownloadModuleRequest downloadPackageRequest)
                {
                    var win = new DownloadModuleDialog(downloadPackageRequest.path, downloadPackageRequest.name);
                    if (win.ShowDialog() == true)
                    {
                        Asset.ModuleManager.LoadModules();
                    }
                }
                else if (operation is HuiDesktopProtocolHelper.CreateSandboxRequest createSandboxRequest)
                {
                    var b = new StringBuilder("某个链接唤起了HuiDesktop并请求创建沙盒，且将在沙盒中写入以下文件：\r\n");
                    foreach (var i in createSandboxRequest.files)
                    {
                        b.AppendLine(i.Item1);
                    }
                    b.AppendLine("调用方提示：");
                    b.AppendLine(createSandboxRequest.recommendation);
                    b.Append("需要自己在创建沙盒的界面选择加入沙盒的模块，您应该了解了做法。\r\n是否打开沙盒创建界面？");
                    if (MessageBox.Show(b.ToString(), "提示", MessageBoxButton.YesNo, MessageBoxImage.Asterisk) == MessageBoxResult.Yes)
                    {
                        var win = new CreateSandboxDialog(createSandboxRequest.sandboxName);
                        if (win.ShowDialog() == true)
                        {
                            var path = Asset.Sandbox.Create(win.SandboxName) + "\\Root\\";
                            foreach (var i in createSandboxRequest.files)
                            {
                                string dest = Path.GetFullPath(Path.Combine(path, i.Item1));
                                if (dest.StartsWith(path))
                                {
                                    if (i.Item2 is string s)
                                    {
                                        File.WriteAllText(dest, s);
                                    }
                                    else if (i.Item2 is byte[] bs)
                                    {
                                        File.WriteAllBytes(dest, bs);
                                    }
                                }
                            }
                            Asset.SandboxManager.LoadSandboxes();
                            var sandbox = Asset.SandboxManager.Sandboxes.FirstOrDefault(x => x.Name == win.SandboxName);
                            if (sandbox != default(Asset.Sandbox))
                            {
                                new SandboxManageWindow(sandbox).ShowDialog();
                                Asset.SandboxManager.LoadSandboxes();
                            }
                        }
                    }
                    else
                    {
                        Environment.Exit(1);
                    }
                }
            }
            LoadSandboxes();
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
            {
                SandboxWaterfallViewer.Children.Add(new SandboxPreview(i, LoadSandboxes));
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

        private void CreateSandboxButtonClicked(object sender, RoutedEventArgs e)
        {
            var win = new CreateSandboxDialog();
            if (win.ShowDialog() == true)
            {
                Asset.Sandbox.Create(win.SandboxName);
                Asset.SandboxManager.LoadSandboxes();
                LoadSandboxes();
            }
        }
    }
}
