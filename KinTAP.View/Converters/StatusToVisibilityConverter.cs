using System;
using System.Globalization;
using Avalonia.Data.Converters;
using KinTAP.Model;

namespace KinTAP.View.Converters
{
    public class StatusToVisibilityConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if(value is TaskStatus currentStatus && parameter is string targetStatusString)
            {
                if(Enum.TryParse<TaskStatus>(targetStatusString, true, out var targetStatus))
                {
                    return currentStatus == targetStatus;
                }
            }
            return false;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
