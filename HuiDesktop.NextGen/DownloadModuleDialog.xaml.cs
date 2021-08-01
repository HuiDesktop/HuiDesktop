using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Path = System.IO.Path;

namespace HuiDesktop.NextGen
{
    /// <summary>
    /// DownloadModuleDialog.xaml 的交互逻辑
    /// </summary>
    public partial class DownloadModuleDialog : Window
    {
        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        Exception err;

        public DownloadModuleDialog(string source, string name)
        {
            InitializeComponent();
            SourceTextBox.Text = source;
            NameTextBox.Text = name;
        }

        private async void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            DownloadButton.IsEnabled = false;
            string tmp;
            try
            {
                tmp = await PackageDownloadManager.DownloadPackage(SourceTextBox.Text, cancellationTokenSource.Token, new FileDownloadProgress1(DownloadProgressBar));
            }
            catch (OperationCanceledException)
            {
                cancellationTokenSource.Dispose();
                cancellationTokenSource = null;
                Dispatcher.Invoke(() => { DialogResult = false; });
                return;
            }
            HuiDesktopProtocolHelper.UnzipTo(tmp, Path.Combine(FileSystemManager.NextGenModulePath, NameTextBox.Text));
            Dispatcher.Invoke(() => { DialogResult = true; });
        }

        private void NameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (DownloadButton != null)
            {
                if (NameTextBox.Text.Intersect(Path.GetInvalidFileNameChars()).Any() || Directory.Exists(Path.Combine(FileSystemManager.NextGenModulePath, NameTextBox.Text)))
                {
                    DownloadButton.IsEnabled = false;

                }
                else
                {
                    DownloadButton.IsEnabled = true;
                }
            }
        }

    }

    class FileDownloadProgress1 : IProgress<(int, int)>
    {
        private readonly ProgressBar progressBar;

        public FileDownloadProgress1(ProgressBar progressBar)
        {
            this.progressBar = progressBar;
        }

        public void Report((int, int) value)
        {
            progressBar.Dispatcher.Invoke(() => { progressBar.Value = ((double)value.Item2) / value.Item1; });
        }
    }
}
