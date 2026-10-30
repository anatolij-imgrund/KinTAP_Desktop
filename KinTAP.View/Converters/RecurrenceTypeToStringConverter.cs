using System;
using System.Globalization;
using Avalonia.Data.Converters;
using KinTAP.Model;

namespace KinTAP.View.Converters
{
    public class RecurrenceTypeToStringConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if(value is TaskRecurrenceType type)
            {
                switch(type)
                {
                    case TaskRecurrenceType.None: return "Einmalig";
                    case TaskRecurrenceType.Daily: return "Täglich";
                    case TaskRecurrenceType.Weekly: return "Wochentlich";
                    default: return type.ToString();
                }
            }
            return "Unbekant";
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}