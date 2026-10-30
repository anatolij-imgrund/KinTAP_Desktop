using System;
using System.Globalization;
using Avalonia.Data.Converters;
using KinTAP.Model;

namespace KinTAP.View.Converters
{
    public class TaskToOverdueVisibilityConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType,object? parameter, CultureInfo culture)
        {
            if (value is TaskItem task && task.DueDate.HasValue && task.DueDate.Value.Date < DateTime.Today && task.Status == TaskStatus.Pending)
            {
                return true;
            }

            return false;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}