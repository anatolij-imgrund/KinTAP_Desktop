using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace KinTAP.View.Converters
{
    public class StringToBitmapConverter : IValueConverter
    {
        public static StringToBitmapConverter Instance { get; } = new StringToBitmapConverter();
        private static readonly string _defaultAvatarPath = Path.Combine(AppContext.BaseDirectory, "Assets", "default_avatar.png");

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if(value is string imagePathOrName && !string.IsNullOrWhiteSpace(imagePathOrName))
            {
                try
                {                   
                    if (imagePathOrName.StartsWith("/Assets/") || imagePathOrName.StartsWith("avares://"))
                    {
                        Uri uri;

                        if (imagePathOrName.StartsWith("avares://"))
                        {
                            uri = new Uri(imagePathOrName, UriKind.Absolute);
                        }
                        else
                        {
                            string assemblyName = "KinTAP.View";
                            string assetPath = imagePathOrName.TrimStart('/');
                            uri = new Uri($"avares://{assemblyName}/{assetPath}");
                        }

                        if (AssetLoader.Exists(uri))
                        {
                            using (var stream = AssetLoader.Open(uri))
                            {
                                return new Bitmap(stream);
                            }
                        }
                        else
                        {
                            Debug.WriteLine($"Asset wurde nicht gefunden: {uri}");
                            return _defaultAvatarPath;
                        }
                    }
                    else
                    {
                        string basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                        "KinTAP",
                                        "Images",
                                        "Children");
                        string fullPath = Path.Combine(basePath, imagePathOrName);

                        if(File.Exists(fullPath))
                        {
                            return new Bitmap(fullPath);
                        }
                        else
                        {
                            Debug.WriteLine($"Bilddatei wurde nicht gefunden: {fullPath}");
                            return null;
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine($"Feher beim Bildhochladen: {e.Message}");
                    return null;
                }
            }
            return null;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}