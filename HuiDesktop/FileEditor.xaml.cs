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

namespace HuiDesktop
{
    /// <summary>
    /// FileEditor.xaml 的交互逻辑
    /// </summary>
    public partial class FileEditor : Window
    {
        public FileEditor(string path)
        {
            if (path == null) throw new ArgumentException("Path should not be null.", nameof(path));
            InitializeComponent();
            Label_Path.Content = path;
            Textbox_Content.Text = File.ReadAllText(path);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            File.WriteAllText(Label_Path.Content as string, Textbox_Content.Text, Encoding.UTF8);
            Button_Save.IsEnabled = false;
        }

        private void Textbox_Content_TextChanged(object sender, TextChangedEventArgs e)
        {
            Button_Save.IsEnabled = true;
        }
    }
}
