using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace OrderManagementSystem.Wpf.Converters
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool state)
                return state ? Visibility.Visible : Visibility.Collapsed;

            // No valid input — signal no value (alternative: return Visibility.Collapsed)
            return DependencyProperty.UnsetValue;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is Visibility v)
                return v == Visibility.Visible;

            // No valid input for ConvertBack — indicate no action (alternative: return false)
            return Binding.DoNothing;
        }
    }
}
