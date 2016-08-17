using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace ArcGISControl.PropertyControl
{
    /// <summary>
    /// Interaction logic for VisibilityComboBox.xaml
    /// </summary>
    public partial class VisibilityComboBox : UserControl
    {
        public static readonly DependencyProperty ComboBoxStyleProperty
            = DependencyProperty.Register(
                "ComboBoxStyle",
                typeof(Style),
                typeof(VisibilityComboBox));

        public Style ComboBoxStyle
        {
            get { return (Style)this.GetValue(ComboBoxStyleProperty); }
            set { this.SetValue(ComboBoxStyleProperty, value); }
        }

        public static readonly DependencyProperty IsVisibleSelectedProperty
            = DependencyProperty.Register(
                "IsVisibleSelected",
                typeof(bool?),
                typeof(VisibilityComboBox),
                new PropertyMetadata(true));

        public bool? IsVisibleSelected
        {
            get { return (bool?)this.GetValue(IsVisibleSelectedProperty); }
            set { this.SetValue(IsVisibleSelectedProperty, value); }
        }

        public VisibilityComboBox()
        {
            InitializeComponent();
        }
    }

    public class IsVisibleSelectedToSelectedIndexConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object paramater, CultureInfo culture)
        {
            if (value == null) return -1;

            if (!(value is bool))
                throw new InvalidOperationException();
            var isVisibleSelected = (bool)value;
            return isVisibleSelected ? 0 : 1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is int))
                throw new InvalidOperationException();
            var selectedIndex = (int)value;
            return selectedIndex == 0;
        }
    }
}
