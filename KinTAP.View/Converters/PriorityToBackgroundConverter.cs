using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using KinTAP.Model;

namespace KinTAP.View.Converters
{
    public class PriorityToBackgriundConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var importantBrush = Brushes.LightSalmon;
            var defaultBrush = Brushes.WhiteSmoke;

            if(value is TaskPriority priority)
            {
                if(priority == TaskPriority.Important)
                {
                    return importantBrush;
                }

                return defaultBrush;
            }

            return defaultBrush;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}