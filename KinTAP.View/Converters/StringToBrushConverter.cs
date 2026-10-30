using System;
using System.Diagnostics;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace KinTAP.View.Converters
{
    public class StringToBrushConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if(value is string colorKeyOrHex && Application.Current != null)
            {
                if(Application.Current.Resources.TryGetResource(colorKeyOrHex, Application.Current.ActualThemeVariant, out var resource) && resource is IBrush brush)
                {
                    return brush;
                }

                try
                {
                    return new SolidColorBrush(Color.Parse(colorKeyOrHex));
                }
                catch(FormatException fe)
                {
                    Debug.WriteLine($"{fe.Message}");
                }
                catch(Exception e)
                {
                    Debug.WriteLine($"{e.Message}");
                }
            }

            if(Application.Current != null && Application.Current.Resources.TryGetResource("ThemeBrushDefault", Application.Current.ActualThemeVariant, out var defaultResource) && defaultResource is IBrush defaultBrush)
            {
                return defaultBrush;
            }

            return Brushes.Transparent;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}