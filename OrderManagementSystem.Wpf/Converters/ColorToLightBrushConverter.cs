using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;

namespace OrderManagementSystem.Wpf.Converters
{
    public class ColorToLightBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is SolidColorBrush brush)
            {
                Color originalColor = brush.Color;

                // نقوم بإنشاء لون جديد بنفس الدرجة ولكن بشفافية منخفضة (مثلاً 40 من 255)
                // يمكنك تغيير الرقم 40 لزيادة أو تقليل درجة الفاتح
                Color lightColor = Color.FromArgb(40, originalColor.R, originalColor.G, originalColor.B);

                return new SolidColorBrush(lightColor);
            }

            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
