using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ShieldVSExtension.Common.Converters;

public class BooleanToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is true
            ? new SolidColorBrush(Colors.MediumSeaGreen) { Opacity = 0.7 }
            : new SolidColorBrush(Colors.Red) { Opacity = 0.7 };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}