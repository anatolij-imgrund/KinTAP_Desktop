using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using KinTAP.Model;

namespace KinTAP.View.Converters
{
    public class StatusToBackgroundConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if(value is TaskStatus status)
            {
                switch(status)
                {
                    case TaskStatus.Pending:
                        return Brushes.WhiteSmoke;
                    case TaskStatus.CompletedByChild:
                        return Brushes.LightGreen;
                    case TaskStatus.Confirmed:
                        return Brushes.SpringGreen;
                    default:
                        return Brushes.Transparent;
                }
            }
            return Brushes.Transparent;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}