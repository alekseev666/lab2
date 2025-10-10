using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace lab2.ViewModels
{
    // Базовый класс для всех моделей представления
    // Помогает уведомлять интерфейс об изменениях в данных
    public class BaseViewModel : INotifyPropertyChanged
    {
        // Событие, которое сообщает интерфейсу об изменении свойства
        public event PropertyChangedEventHandler? PropertyChanged;

        // Метод для уведомления об изменении свойства
        protected void NotifyPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        // Метод для установки нового значения свойства
        // Возвращает true, если значение изменилось
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            // Проверяем, изменилось ли значение
            if (field == null && value == null)
                return false;
                
            if (field != null && field.Equals(value))
                return false;

            // Устанавливаем новое значение
            field = value;
            
            // Уведомляем интерфейс об изменении
            NotifyPropertyChanged(propertyName);
            
            return true;
        }
    }
}
