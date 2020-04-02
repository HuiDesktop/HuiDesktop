using System.IO;
using System.Windows;

namespace HuiDesktop
{
    /// <summary>
    /// DevelopTools.xaml 的交互逻辑
    /// </summary>
    public partial class DevelopTools : Window
    {
        public DevelopTools()
        {
            InitializeComponent();
        }

        private void Label_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effects = DragDropEffects.Copy;
            e.Handled = true;
        }

        private void Label_OpenFile_Drop(object sender, DragEventArgs e)
        {
            foreach (string i in e.Data.GetData(DataFormats.FileDrop) as string[])
            {
                new FileEditor(i).Show();
            }
        }

        private void Label_Pack_Drop(object sender, DragEventArgs e)
        {
            var root = (e.Data.GetData(DataFormats.FileDrop) as string[])[0];
            if (File.Exists(Path.Combine(root, "package.json")) == false) return;
            var dialog = new System.Windows.Forms.SaveFileDialog();
            dialog.InitialDirectory = Path.Combine(Path.GetDirectoryName(GetType().Assembly.CodeBase), "packages");
            dialog.Filter = "HuiDesktop V4 Package|*.hdtpkg4";
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                using (var fs = File.Open(dialog.FileName, FileMode.Create))
                {
                    var package = new Package.V4Package(root);
                    package.Export(fs);
                }
            }
        }

        private void Button_NewLocalPackage_Click(object sender, RoutedEventArgs e)
        {
            string basePath = ApplicationInfo.RelativePath("localPackages", System.Guid.NewGuid().ToString());
            Directory.CreateDirectory(basePath);
            Directory.CreateDirectory(Path.Combine(basePath, "files"));
            File.WriteAllText(Path.Combine(basePath, "package.json"), Properties.Resources.DefaultPackageInfoJson);
        }
    }
}
