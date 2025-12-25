using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nicesoon.Converters
{
    public class ImagePathConverter : IValueConverter
    {
        public string DefaultImage { get; set; } = "default_pic.jpg";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string path && !string.IsNullOrEmpty(path) && File.Exists(path))
            {
                return ImageSource.FromFile(path);
            }

            return ImageSource.FromFile(DefaultImage);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
