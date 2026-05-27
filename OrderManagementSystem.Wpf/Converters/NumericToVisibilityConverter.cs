using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace OrderManagementSystem.Wpf.Converters
{
    public class NumericToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int count)
            {
                // إذا كنا نريد إظهار الـ Empty State عندما تكون القائمة فارغة تماماً
                if (parameter is string paramStr && paramStr.Equals("Empty", StringComparison.OrdinalIgnoreCase))
                {
                    return count == 0 ? Visibility.Visible : Visibility.Collapsed;
                }

                // الحالة الافتراضية: إذا كان أكبر من 0 يظهر، وإذا كان 0 يختفي
                return count > 0 ? Visibility.Visible : Visibility.Collapsed;
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }
}
