using System;
using System.Globalization;
using Avalonia.Data.Converters;
using KinTAP.Model;

namespace KinTAP.View.Converters
{
    public class TaskToAssignmentTypeStringConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is TaskItem task)
            {
                if (task.SourceTemplateId != null && task.AssignedChildId != null)
                {
                    if (task.DueDate.HasValue)
                    {
                        if (task.DueDate.Value.Date == DateTime.Today)
                        {
                            return $"Für heute\n{task.DueDate.Value:dd.MM.yyyy}";
                        }
                        else
                        {
                            return $"Für {task.DueDate.Value:dd.MM.yyyy}";
                        }
                    }
                    else
                    {
                        if (task.SpecificDueDate.HasValue || task.RecurringDaysOfWeek == 0)
                        {
                            return "Ohne Fälligkeitsdatum";
                        }
                        return "Abo-Aufgabe\n(wird entsprechend den Einstellungen neue Aufgaben generieren)";
                    }
                }
                else
                {
                    return null;
                }
            }
            return string.Empty;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}