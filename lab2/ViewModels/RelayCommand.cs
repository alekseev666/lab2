using System;
using System.Windows.Input;

namespace lab2.ViewModels
{
    // Класс для создания команд (кнопок) в интерфейсе
    public class RelayCommand : ICommand
    {
        // Метод, который выполняется при нажатии кнопки
        private readonly Action executeAction;
        
        // Метод для проверки, можно ли выполнить команду
        private readonly Func<bool>? canExecuteAction;

        // Конструктор для создания команды
        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            if (execute == null)
                throw new ArgumentException("Метод выполнения не может быть пустым");
                
            executeAction = execute;
            canExecuteAction = canExecute;
        }

        // Проверяет, можно ли выполнить команду
        public bool CanExecute(object? parameter)
        {
            if (canExecuteAction == null)
                return true;
                
            return canExecuteAction();
        }

        // Выполняет команду
        public void Execute(object? parameter)
        {
            executeAction();
        }

        // Событие, которое сообщает об изменении возможности выполнения команды
        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}
