using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace ShieldVSExtension.Common.Converters
{
    /// <summary>
    /// Converter to chain together multiple converters.
    /// </summary>
    public class ValueConverterGroup : List<IValueConverter>,
                                       IValueConverter
    {
        public object Convert(object value,
                              Type targetType,
                              object parameter,
                              CultureInfo language)
        {
            var curValue = value;
            for (var i = 0; i < Count; i++)
            {
                curValue = this[i].Convert(curValue, targetType, parameter, language);
            }

            return (curValue);
        }

        public object ConvertBack(object value,
                                  Type targetType,
                                  object parameter,
                                  CultureInfo culture)
        {
            var curValue = value;
            for (var i = (Count - 1); i >= 0; i--)
            {
                curValue = this[i].ConvertBack(curValue, targetType, parameter, culture);
            }

            return (curValue);
        }

    }

    /// <summary>
    /// Inverts a boolean value.
    /// </summary>
    public class InvertBoolConverter : IValueConverter
    {
        public object Convert(object value,
                              Type targetType,
                              object parameter,
                              CultureInfo language)
        {
            if (value is bool b)
            {
                return (!b);
            }

            throw new ArgumentException("Value must be of the type bool");
        }

        public object ConvertBack(object value,
                                  Type targetType,
                                  object parameter,
                                  CultureInfo language)
        {
            if (value is bool b)
            {
                return (!b);
            }

            throw new ArgumentException();
        }
    }
}
