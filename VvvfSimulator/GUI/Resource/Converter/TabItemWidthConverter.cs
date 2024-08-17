using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace VvvfSimulator.GUI.Resource.Converter
{
    public class TabItemWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            TabControl? tabControl = value as TabControl;
            if (tabControl != null)
            {
                double width = tabControl.ActualWidth / tabControl.Items.Count;
                //Subtract 1, otherwise we could overflow to two rows.
                return (width <= 1) ? 0 : (width - 1);
            }
            return 10;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
