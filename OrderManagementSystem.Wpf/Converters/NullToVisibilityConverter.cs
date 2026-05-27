using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace OrderManagementSystem.Wpf.Converters
{
    public class NullToVisibilityConverter : IValueConverter
    {
        public bool Inverse { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isNullOrEmpty = value == null || (value is string str && string.IsNullOrWhiteSpace(str));

            if (Inverse)
            {
                return isNullOrEmpty ? Visibility.Visible : Visibility.Collapsed;
            }

            return isNullOrEmpty ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }
}
