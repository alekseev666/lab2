using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace lab2
{
    // Класс с разными конвертерами для интерфейса
    public static class Converters
    {
        // Конвертер true/false -> видимо/скрыто
        public static readonly IValueConverter BoolToVisibilityConverter = new BoolToVisibilityValueConverter();
        
        // Конвертер true/false -> скрыто/видимо (обратный)
        public static readonly IValueConverter InverseBoolToVisibilityConverter = new InverseBoolToVisibilityValueConverter();
        
        // Конвертер null/не-null -> скрыто/видимо
        public static readonly IValueConverter NullToVisibilityConverter = new NullToVisibilityValueConverter();
        
        // Конвертер true/false -> false/true (обратный)
        public static readonly IValueConverter InverseBoolConverter = new InverseBoolValueConverter();
    }

    // Конвертер: превращает true в "видимо", false в "скрыто"
    public class BoolToVisibilityValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Проверяем, что значение - это bool
            if (value is bool isTrue)
            {
                if (isTrue)
                    return Visibility.Visible;  // Показываем
                else
                    return Visibility.Collapsed; // Скрываем
            }

            return Visibility.Collapsed; // По умолчанию скрываем
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Обратное преобразование: "видимо" -> true, остальное -> false
            if (value is Visibility visibility)
                return visibility == Visibility.Visible;
            else
                return false;
        }
    }

    // Конвертер: превращает true в "скрыто", false в "видимо" (обратный)
    public class InverseBoolToVisibilityValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Проверяем, что значение - это bool
            if (value is bool isTrue)
            {
                if (isTrue)
                    return Visibility.Collapsed; // Скрываем
                else
                    return Visibility.Visible;   // Показываем
            }

            return Visibility.Visible; // По умолчанию показываем
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Обратное преобразование: "скрыто" -> true, остальное -> false
            if (value is Visibility visibility)
                return visibility == Visibility.Collapsed;
            else
                return false;
        }
    }

    // Конвертер: превращает null в "скрыто", не-null в "видимо"
    public class NullToVisibilityValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Проверяем, нужно ли обращение
            bool shouldInverse = false;
            if (parameter != null && parameter.ToString() == "Inverse")
                shouldInverse = true;
                
            bool isEmpty = (value == null);

            // Обычная логика
            if (!shouldInverse)
            {
                if (isEmpty)
                    return Visibility.Collapsed; // Пусто - скрываем
                else
                    return Visibility.Visible;   // Не пусто - показываем
            }
            // Обращенная логика
            else
            {
                if (isEmpty)
                    return Visibility.Visible;   // Пусто - показываем
                else
                    return Visibility.Collapsed; // Не пусто - скрываем
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("Обратное преобразование не поддерживается");
        }
    }

    // Конвертер: превращает true в false и наоборот
    public class InverseBoolValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Проверяем, что значение - это bool
            if (value is bool isTrue)
            {
                return !isTrue; // Меняем на противоположное
            }

            return true; // По умолчанию true
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Обратное преобразование - тоже меняем на противоположное
            if (value is bool isTrue)
            {
                return !isTrue;
            }

            return false; // По умолчанию false
        }
    }
}
