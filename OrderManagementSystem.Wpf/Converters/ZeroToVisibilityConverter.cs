using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace OrderManagementSystem.Wpf.Converters
{
    // هذا الكلاس يحول القيمة (O) الى visible و اي قيمة ثانية الى Collapsed 
    public class ZeroToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // إذا كان العدد 0، نرجع Visible حتى تظهر رسالة "لا يوجد عملاء"
            if (value is int count && count == 0)
                return Visibility.Visible;

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
