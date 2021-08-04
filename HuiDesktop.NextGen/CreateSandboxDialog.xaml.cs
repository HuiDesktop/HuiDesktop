using System;
using System.Collections.Generic;
using System.IO;
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
using Path = System.IO.Path;

namespace HuiDesktop.NextGen
{
    /// <summary>
    /// CreateSandboxDialog.xaml 的交互逻辑
    /// </summary>
    public partial class CreateSandboxDialog : Window
    {
        public string SandboxName => SandboxNameTextBox.Text;

        public CreateSandboxDialog(string defaultName = "")
        {
            InitializeComponent();
            SandboxNameTextBox.Text = defaultName;
            Height = 23 + 40 + SystemParameters.CaptionHeight; // 23: TextBox | 20: Blank
        }

        private bool IsOk()
        {
            var content = SandboxNameTextBox.Text;
            var invalid = Path.GetInvalidFileNameChars();
            foreach (var i in content)
            {
                if (invalid.Contains(i))
                {
                    return false;
                }
            }
            if (string.IsNullOrEmpty(content) ||Directory.Exists(Path.Combine(FileSystemManager.NextGenSandboxPath, content)))
            {
                return false;
            }
            return true;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            OkButton.IsEnabled = IsOk();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
