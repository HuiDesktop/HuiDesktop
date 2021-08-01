using System;
using System.Collections.Generic;
using System.Globalization;
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
    /// CreateSandbox.xaml 的交互逻辑
    /// </summary>
    public partial class SandboxManageWindow : Window
    {
        private SandboxManageWindowModel dataContext;

        public SandboxManageWindow(Asset.Sandbox sandbox)
        {
            InitializeComponent();
            dataContext = new SandboxManageWindowModel(sandbox);
            DataContext = dataContext;
        }

        private void SaveButtonClicked(object sender, RoutedEventArgs e)
        {

            DialogResult = true;
        }

        private void OpenFolderButtonClicked(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe", dataContext.Sandbox.BasePath);
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (dataContext.RightSelectedModule is Asset.Module m)
            {
                dataContext.Modules.Add(m);
                dataContext.InvokeModulesChaned();
            }
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            dataContext.Modules.Remove(dataContext.LeftSelectedModule);
            dataContext.InvokeModulesChaned();
        }
    }

    class SandboxManageWindowModel : ModelBase
    {
        public Asset.Sandbox Sandbox { get; }

        private HashSet<object> modules;
        public HashSet<object> Modules
        {
            get => modules;
            set
            {
                modules = value;
                OnPropertyChanged();
            }
        }

        private bool canSave = false;
        public bool CanSave
        {
            get
            {
                foreach (var i in modules)
                {
                    if (i is string)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        public void InvokeModulesChaned()
        {
            Modules = new HashSet<object>(modules);
        }

        private object leftSelectedModule;
        public object LeftSelectedModule
        {
            get => leftSelectedModule;
            set
            {
                leftSelectedModule = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanSave));
                UpdateSelectedModule();
            }
        }

        private object rightSelectedModule;
        public object RightSelectedModule
        {
            get => rightSelectedModule;
            set
            {
                rightSelectedModule = value;
                OnPropertyChanged();
                UpdateSelectedModule();
            }
        }

        private object selectedModule;
        public object SelectedModule
        {
            get => selectedModule;
            set
            {
                selectedModule = value;
                OnPropertyChanged();
            }
        }

        private void UpdateSelectedModule()
        {
            if (leftSelectedModule != null)
            {
                SelectedModule = leftSelectedModule;
                return;
            }
            if (rightSelectedModule != null)
            {
                SelectedModule = rightSelectedModule;
                return;
            }
            SelectedModule = null;
        }

        public SandboxManageWindowModel(Asset.Sandbox sandbox)
        {
            Sandbox = sandbox;
            modules = new HashSet<object>();
            foreach (var i in sandbox.Dependencies)
            {
                var m = Asset.ModuleManager.GetModule(i);
                modules.Add(m ?? (object)$"（未加载）{i}");
            }
        }
    }

    public class UnusedModulesConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var r = new HashSet<Asset.Module>(Asset.ModuleManager.Modules);
            if (value is IEnumerable<object> ls)
            {
                foreach (var i in ls)
                {
                    if (i is Asset.Module m)
                    {
                        r.Remove(m);
                    }
                }
            }
            return r;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class SuggestionsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var r = new List<string>();
            if (value is IEnumerable<object> ls)
            {
                foreach (var i in ls)
                {
                    if (i is Asset.Module m)
                    {
                        foreach (var j in m.Suggestions)
                        {
                            r.Add($"{j.Name} | {j.Message} | 来自模块{m.FriendlyName}");
                        }
                    }
                }
            }
            return r;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class NullDisableConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ModuleDetailConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string) return value;
            if (value is Asset.Module m)
            {
                if (m.Features.Count() == 0)
                {
                    return "(未实现任何特性)";
                }
                var s = new StringBuilder();
                foreach (var i in m.Features)
                {
                    s.Append(i);
                    s.Append(", ");
                }
                s.Length -= 2;
                return s.ToString();
            }
            return "选中一个模块查看其实现的特性。";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
