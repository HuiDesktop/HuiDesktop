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
using CefSharp;

namespace HuiDesktop
{
    /// <summary>
    /// ForumWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ForumWindow : Window
    {
        public ForumWindow()
        {
            InitializeComponent();
            browser.LoadHandler = new LoadHandler(TextBox_Address, browser);
            browser.DownloadHandler = new DownloadHandler();
            browser.Address = "https://desktop.huix.cc";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (browser.Address == TextBox_Address.Text) browser.Reload();
            else browser.Address = TextBox_Address.Text;
        }

        private void Button_Forward_Click(object sender, RoutedEventArgs e)
        {
            if (browser.CanGoForward) browser.Forward();
        }

        private void Button_Backward_Click(object sender, RoutedEventArgs e)
        {
            if (browser.CanGoBack) browser.Back();
        }
    }

    public class LoadHandler : ILoadHandler
    {
        private TextBox _tb;
        private IWebBrowser _browser;

        public LoadHandler(TextBox tb, IWebBrowser browser) { _tb = tb; _browser = browser; }

        public void OnFrameLoadEnd(IWebBrowser chromiumWebBrowser, FrameLoadEndEventArgs frameLoadEndArgs)
        {
        }

        public void OnFrameLoadStart(IWebBrowser chromiumWebBrowser, FrameLoadStartEventArgs frameLoadStartArgs)
            => _tb.Dispatcher.Invoke(() => {
                _tb.Text = _browser.Address;
            });

        public void OnLoadError(IWebBrowser chromiumWebBrowser, LoadErrorEventArgs loadErrorArgs)
        {
        }

        public void OnLoadingStateChange(IWebBrowser chromiumWebBrowser, LoadingStateChangedEventArgs loadingStateChangedArgs)
        {
        }
    }

    public class DownloadHandler : IDownloadHandler
    {
        private Dictionary<int, DownloadStatus> downloads = new Dictionary<int, DownloadStatus>();

        public void OnBeforeDownload(IWebBrowser chromiumWebBrowser, IBrowser browser, DownloadItem downloadItem, IBeforeDownloadCallback callback)
        {
            //if (downloadItem.SuggestedFileName.EndsWith(".hdtpkg4") == false) return;
            if (!callback.IsDisposed)
                using (callback)
                {
                    string file = ApplicationInfo.RelativePath("packages", downloadItem.SuggestedFileName);
                    if (System.IO.File.Exists(file))
                    {
                        if (System.IO.Directory.Exists(ApplicationInfo.RelativePath("backupPackages")) == false) System.IO.Directory.CreateDirectory(ApplicationInfo.RelativePath("backupPackages"));
                        System.IO.File.Move(file, ApplicationInfo.RelativePath("backupPackages", Guid.NewGuid().ToString() + downloadItem.SuggestedFileName));
                    }
                    downloads[downloadItem.Id] = new DownloadStatus($"packages/{downloadItem.SuggestedFileName}", downloadItem.TotalBytes);
                    downloads[downloadItem.Id].Show();
                    callback.Continue(file, false);
                }
        }

        public void OnDownloadUpdated(IWebBrowser chromiumWebBrowser, IBrowser browser, DownloadItem downloadItem, IDownloadItemCallback callback)
        {
            if (downloads.ContainsKey(downloadItem.Id) == false) return;
            downloads[downloadItem.Id].Update(downloadItem.ReceivedBytes, downloadItem.CurrentSpeed);
            if (downloadItem.IsComplete) downloads[downloadItem.Id].Finish();
            if (downloads[downloadItem.Id].Abort) callback.Cancel();
        }
    }
}
