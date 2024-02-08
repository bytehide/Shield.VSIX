using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ShieldVSExtension.Common.Converters
{
    public class BooleanToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter), @"Parameter cannot be null");
            }

            var colors = parameter.ToString().Split(',');

            if (colors.Length != 2)
            {
                throw new ArgumentException(@"Parameter must contain two colors separated by a comma",
                    nameof(parameter));
            }

            var trueColor = (Color)ColorConverter.ConvertFromString(colors[0].Trim())!;
            var falseColor = (Color)ColorConverter.ConvertFromString(colors[1].Trim())!;

            return (value is true)
                ? new SolidColorBrush(trueColor) { Opacity = 0.7 }
                : new SolidColorBrush(falseColor) { Opacity = 0.7 };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}