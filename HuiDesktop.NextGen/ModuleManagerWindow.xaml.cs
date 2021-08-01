using System;
using System.Collections.Generic;
using System.Globalization;
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

namespace HuiDesktop.NextGen
{
    /// <summary>
    /// ModuleManagerWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ModuleManagerWindow : Window
    {
        ModuleManagerWindowModel dataContext = new ModuleManagerWindowModel();

        public ModuleManagerWindow()
        {
            InitializeComponent();
            DataContext = dataContext;
            LoadList();
        }

        private void LoadList()
        {
            ModuleListBox.ItemsSource = Asset.ModuleManager.Modules;
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dataContext.Module == null)
            {
                MaskGrid.Visibility = Visibility.Visible;
                return;
            }
            MaskGrid.Visibility = Visibility.Hidden;
        }

        private void RefreshListButtonClicked(object sender, RoutedEventArgs e)
        {
            Asset.ModuleManager.LoadModules();
            LoadList();
        }

        private void OpenFolderButtonClicked(object sender, RoutedEventArgs e)
        {
            if (!(ModuleListBox.SelectedItem is Asset.Module module))
            {
                MaskGrid.Visibility = Visibility.Visible;
                return;
            }
            System.Diagnostics.Process.Start("explorer.exe", module.BasePath);
        }


        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            switch (dataContext.ShowingType)
            {
                case ShowingTypeRadioBoxConverter.ShowingTypes.Launch:
                    //DetailListBox.ItemsSource = 
                    break;
                case ShowingTypeRadioBoxConverter.ShowingTypes.Setup:
                    break;
                case ShowingTypeRadioBoxConverter.ShowingTypes.Suggestion:
                    break;
            }
        }
    }

    class VisibleAndHidden<TVisible, THidden>
    {
        public readonly TVisible visible;
        public readonly THidden hidden;

        public VisibleAndHidden(TVisible visible, THidden hidden)
        {
            this.visible = visible;
            this.hidden = hidden;
        }

        public override string ToString()
        {
            return visible.ToString();
        }
    }

    class ModuleManagerWindowModel : ModelBase
    {
        private ShowingTypeRadioBoxConverter.ShowingTypes showingType;
        public ShowingTypeRadioBoxConverter.ShowingTypes ShowingType
        {
            get => showingType;
            set
            {
                showingType = value;
                OnPropertyChanged();
                var r = new List<object>();
                switch (value)
                {
                    case ShowingTypeRadioBoxConverter.ShowingTypes.Launch:
                        foreach (var i in module.LaunchInfos)
                        {
                            r.Add(new VisibleAndHidden<string, Asset.ModuleLaunchInfo>(i.Name, i));
                        }
                        break;
                    case ShowingTypeRadioBoxConverter.ShowingTypes.Setup:
                        foreach (var i in module.SetupInfos)
                        {
                            r.Add(new VisibleAndHidden<string, Asset.ModuleLaunchInfo>(i.Name, i));
                        }
                        break;
                    case ShowingTypeRadioBoxConverter.ShowingTypes.Suggestion:
                        foreach (var i in module.Suggestions)
                        {
                            r.Add(new VisibleAndHidden<string, Asset.ModuleSuggestion>(i.Name, i));
                        }
                        break;
                }
                DetailList = r;
            }
        }

        private List<object> detailList = new List<object>();
        public List<object> DetailList
        {
            get => detailList;
            set
            {
                detailList = value;
                OnPropertyChanged();
            }
        }

        private Asset.Module module;
        public Asset.Module Module
        {
            get => module;
            set
            {
                module = value;
                ShowingType = ShowingTypeRadioBoxConverter.ShowingTypes.Launch;
                OnPropertyChanged();
            }
        }

        private object detail;
        public object Detail
        {
            get => detail;
            set
            {
                detail = value;
                OnPropertyChanged();
            }
        }
    }

    public class ShowingTypeRadioBoxConverter : IValueConverter
    {
        public enum ShowingTypes
        {
            Launch, Setup, Suggestion
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ShowingTypes s = (ShowingTypes)value;
            return s == (ShowingTypes)int.Parse(parameter.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isChecked = (bool)value;
            if (!isChecked) return null;
            return (ShowingTypes)int.Parse(parameter.ToString());
        }
    }

    public class PropertyValueStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? "" : value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class NullVisibleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? Visibility.Visible : Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class DetailConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is VisibleAndHidden<string, Asset.ModuleLaunchInfo> s)
            {
                return $"名称：{s.hidden.Name}\r\nURL：{s.hidden.Url}";
            }
            if (value is VisibleAndHidden<string, Asset.ModuleSuggestion> t)
            {
                return $"推荐实现：{t.hidden.Name}\r\n提示：{t.hidden.Message}";
            }
            return "选中上方列表项目查看详细信息（本框内文本可以复制）";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
