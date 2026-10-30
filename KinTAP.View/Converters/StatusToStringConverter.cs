using System;
using System.Globalization;
using Avalonia.Data.Converters;
using KinTAP.Model;

namespace KinTAP.View.Converters
{
    public class StatusToStringConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if(value is TaskStatus status)
            {
                switch(status)
                {
                    case TaskStatus.Pending:
                        return "In Bearbeitung";
                    case TaskStatus.CompletedByChild:
                        return "Erledigt. Muss bestätigt werden.";
                    case TaskStatus.Confirmed:
                        return "Bestätigt.";
                    default:
                        return status.ToString();
                }
            }
            return string.Empty;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}