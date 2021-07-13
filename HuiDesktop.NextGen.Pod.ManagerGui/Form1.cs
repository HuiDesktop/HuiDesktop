using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HuiDesktop.NextGen.Pod.ManagerGui
{
    public partial class Form1 : Form
    {

        public Form1()
        {
            InitializeComponent();
            rootTextBox.Text = Environment.CurrentDirectory + @"\Pods";
        }

        private void ReadAllPods(object sender, EventArgs e)
        {
            infoTextBox.Text = $"Root = {rootTextBox.Text}\r\n";
            podListBox.Items.Clear();
            if (!Directory.Exists(rootTextBox.Text))
            {
                infoTextBox.Text += $"Directory \"{rootTextBox.Text}\" not found";
                return;
            }
            foreach (var d in Directory.EnumerateDirectories(rootTextBox.Text))
            {
                try
                {
                    infoTextBox.Text += $"Loading {d.Substring(rootTextBox.Text.Length)} ... ";
                    var pod = Pod.LoadPodFromDirectory(d);
                    infoTextBox.Text += "Success\r\n";
                    podListBox.Items.Add(new VisualPod(pod));
                }
                catch (Exception err)
                {
                    infoTextBox.Text += $"Error caught:\r\n{err}\r\n";
                }
            }
            infoTextBox.Text += "Loaded.";
        }

        private void PodListBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            if (podListBox.SelectedItem is VisualPod p)
            {
                infoTextBox.Text = p.GetDetailString();
            }
        }
    }
}
