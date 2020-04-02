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
    /// DownloadStatus.xaml 的交互逻辑
    /// </summary>
    public partial class DownloadStatus : Window
    {
        private long _size;
        private bool _finish = false;
        public bool Abort { get; private set; } = false;

        public DownloadStatus(string name, long size)
        {
            _size = size;
            InitializeComponent();
            Label_FilePath.Content = name;
            ProgressBar_Progress.Maximum = size;
            Label_Size.Content = $"0/{size}";
        }

        public void Update(long current, long speed)
        {
            if (System.Threading.Thread.CurrentThread.ManagedThreadId != Dispatcher.Thread.ManagedThreadId)
            {
                Dispatcher.Invoke(() => Update(current, speed));
                return;
            }
            ProgressBar_Progress.Value = current;
            Label_Size.Content = $"{current}/{_size}";
            Label_Speed.Content = $"{speed}B/s";
        }

        public void Finish()
        {
            _finish = true;
            Label_Size.Content = "Finished";
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Abort = !_finish;
        }
    }
}
