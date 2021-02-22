﻿using System;
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
        }

        private void CreateSandboxButtonClick(object sender, RoutedEventArgs e)
        {
            new CreateSandboxWindow().ShowDialog();
        }

        private void ModuleManageButtonClick(object sender, RoutedEventArgs e)
        {
            new ModuleManagerWindow().ShowDialog();
        }
    }
}
