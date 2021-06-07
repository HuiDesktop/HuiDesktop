using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace HuiDesktop.NextGen
{
    class FileDownloadProgress : IProgress<(int, int)>
    {
        private readonly DownloadPackageDialog dialog;

        public FileDownloadProgress(DownloadPackageDialog downloadPackageDialog)
        {
            dialog = downloadPackageDialog ?? throw new ArgumentNullException();
        }

        public void Report((int, int) value)
        {
            dialog.Dispatcher.Invoke(() =>
            {
                dialog.SpeedLabel.Content = $"{value.Item1} / {value.Item2}";
                dialog.ProgressProgressBar.Value = ((double)value.Item2) / value.Item1;
            });
        }
    }
    /// <summary>
    /// DownloadPackageDialog.xaml 的交互逻辑
    /// </summary>
    public partial class DownloadPackageDialog : Window
    {
        string path;
        string name;
        CancellationTokenSource cancellationTokenSource;

        public DownloadPackageDialog(string path, string name)
        {
            this.path = path;
            this.name = name;
            InitializeComponent();
            PathTextBox.Text = path;
        }

        private async void OkButton_Click(object sender, RoutedEventArgs e) // 这里写void不知可不可
        {
            var ask = new CreateSandboxDialog(name);
            if (ask.ShowDialog() == true)
            {
                cancellationTokenSource = new CancellationTokenSource();
                OkButton.IsEnabled = false;
                name = ask.SandboxName;
                var dest = SandboxManager.CreateEmptySandbox(name);
                Title = "下载至：" + name;
                SpeedLabel.Content = "0 / 0";
                string tmp;
                try
                {
                    tmp = await PackageDownloadManager.DownloadPackage(path, cancellationTokenSource.Token, new FileDownloadProgress(this));
                }
                catch (OperationCanceledException)
                {
                    cancellationTokenSource.Dispose();
                    cancellationTokenSource = null;
                    DialogResult = false;
                    return;
                }
                HuiDesktopProtocolHelper.UnzipTo(tmp, dest);
                cancellationTokenSource.Dispose();
                cancellationTokenSource = null;
                DialogResult = true;
                return;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();
                cancellationTokenSource.Dispose();
                cancellationTokenSource = null;
            }
        }
    }
}
