using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace WpfApp_Project_SyncFiles.Converters
{
    [ValueConversion(typeof(string), typeof(BitmapImage))]

    public class PathToBitmapImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                string path = (string)value;

                if (!string.IsNullOrEmpty(path))
                {
                    // Icon image
                    if (File.Exists(path))
                    {
                        return ConvertToBitmapImage(Icon.ExtractAssociatedIcon(path).ToBitmap());
                    }
                    // Folder image
                    else
                    {
                        return new BitmapImage(new Uri(string.Format(@"pack://application:,,,/{0}", path)));
                    }
                }

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private static BitmapImage ConvertToBitmapImage(Bitmap src)
        {
            MemoryStream ms = new MemoryStream();
            // .Png allows for the background of the Bitmap to be transparent
            src.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            image.StreamSource = ms;
            image.EndInit();
            image.Freeze();
            return image;
        }
    }
}
