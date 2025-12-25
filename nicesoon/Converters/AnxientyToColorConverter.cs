using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using nicesoon.Models;
namespace nicesoon.Converters
{
    public class AnxietyToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is AnxietyLevel anxietyLevel)
            {
                return anxietyLevel switch
                {
                    AnxietyLevel.Low => Color.FromArgb("#4CAF50"),      // Зеленый
                    AnxietyLevel.Medium => Color.FromArgb("#FF9800"),   // Оранжевый
                    AnxietyLevel.High => Color.FromArgb("#F44336"),     // Красный
                    _ => Color.FromArgb("#9E9E9E")                      // Серый
                };
            }

            if (value is NightmareRecord record)
            {
                return record.RecordAnxietyLevel switch
                {
                    AnxietyLevel.Low => Color.FromArgb("#4CAF50"),
                    AnxietyLevel.Medium => Color.FromArgb("#FF9800"),
                    AnxietyLevel.High => Color.FromArgb("#F44336"),
                    _ => Color.FromArgb("#9E9E9E")
                };
            }

            return Color.FromArgb("#9E9E9E");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // Отдельный конвертер для фильтров (кнопок)
    public class AnxietyFilterColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var selectedFilter = value as AnxietyLevel?;
            var targetLevel = parameter as AnxietyLevel?;

            // Если фильтр активен для этой кнопки - выделяем
            if (selectedFilter.HasValue && targetLevel.HasValue && selectedFilter.Value == targetLevel.Value)
            {
                return targetLevel.Value switch
                {
                    AnxietyLevel.Low => Color.FromArgb("#4CAF50"),
                    AnxietyLevel.Medium => Color.FromArgb("#FF9800"),
                    AnxietyLevel.High => Color.FromArgb("#F44336"),
                    _ => Colors.LightGray
                };
            }

            return Colors.LightGray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

