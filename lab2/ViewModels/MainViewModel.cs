using lab2.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace lab2.ViewModels
{
    /// <summary>
    /// Главная ViewModel приложения - связывает интерфейс с логикой вычислений
    /// </summary>
    /// <remarks>
    /// Этот класс отвечает за:
    /// - Хранение введенных пользователем данных
    /// - Управление вычислением weakest precondition
    /// - Отображение результатов и ошибок
    /// - Работу с предустановленными примерами
    /// </remarks>
    public class MainViewModel : BaseViewModel
    {
        private string _codeInput = "";
        private string _postconditionInput = "";
        private string _postconditionHumanInput = "";
        private WpResult _currentResult;
        private bool _isCalculating = false;
        private PresetExample _selectedPreset;

        /// <summary>
        /// Введенный пользователем код программы
        /// </summary>
        /// <example>"x := y + 5; z := x * 2"</example>
        public string CodeInput
        {
            get => _codeInput;
            set => SetProperty(ref _codeInput, value);
        }

        /// <summary>
        /// Введенное постусловие в формате логического выражения
        /// </summary>
        /// <example>"z > 10", "x == y && y > 0"</example>
        public string PostconditionInput
        {
            get => _postconditionInput;
            set => SetProperty(ref _postconditionInput, value);
        }

        /// <summary>
        /// Постусловие в понятном человеческом виде (описание)
        /// </summary>
        /// <example>"результат должен быть больше 10"</example>
        public string PostconditionHumanInput
        {
            get => _postconditionHumanInput;
            set => SetProperty(ref _postconditionHumanInput, value);
        }

        /// <summary>
        /// Текущий результат вычисления weakest precondition
        /// </summary>
        /// <remarks>
        /// Содержит итоговое предусловие, шаги расчета и информацию об ошибках
        /// </remarks>
        public WpResult CurrentResult
        {
            get => _currentResult;
            set => SetProperty(ref _currentResult, value);
        }

        /// <summary>
        /// Флаг, указывающий что идет процесс вычисления
        /// </summary>
        /// <remarks>
        /// Используется для блокировки интерфейса во время расчетов
        /// </remarks>
        public bool IsCalculating
        {
            get => _isCalculating;
            set => SetProperty(ref _isCalculating, value);
        }

        /// <summary>
        /// Выбранный предустановленный пример
        /// </summary>
        /// <remarks>
        /// При выборе примера автоматически заполняются поля ввода
        /// </remarks>
        public PresetExample SelectedPreset
        {
            get => _selectedPreset;
            set
            {
                if (SetProperty(ref _selectedPreset, value) && value != null)
                {
                    LoadPreset(value);
                }
            }
        }

        /// <summary>
        /// Коллекция предустановленных примеров для быстрого выбора
        /// </summary>
        public ObservableCollection<PresetExample> Presets { get; }

        /// <summary>
        /// Команда для запуска вычисления weakest precondition
        /// </summary>
        public ICommand CalculateWpCommand { get; }

        /// <summary>
        /// Команда для показа триады Хоара
        /// </summary>
        public ICommand ShowTripleCommand { get; }

        /// <summary>
        /// Команда для очистки всех полей
        /// </summary>
        public ICommand ClearCommand { get; }

        /// <summary>
        /// Команда для показа справки
        /// </summary>
        public ICommand ShowHelpCommand { get; }

        /// <summary>
        /// Создает новый экземпляр MainViewModel и инициализирует команды
        /// </summary>
        /// <remarks>
        /// В конструкторе:
        /// - Загружаются предустановленные примеры
        /// - Создаются команды для кнопок интерфейса
        /// - Выбирается первый пример по умолчанию
        /// </remarks>
        public MainViewModel()
        {
            Presets = new ObservableCollection<PresetExample>(PresetExample.GetDefaultPresets());

            CalculateWpCommand = new RelayCommand(CalculateWp, CanCalculateWp);
            ShowTripleCommand = new RelayCommand(ShowTriple, () => CurrentResult != null && !CurrentResult.HasErrors);
            ClearCommand = new RelayCommand(Clear);
            ShowHelpCommand = new RelayCommand(ShowHelp);

            // Загружаем первый пример по умолчанию
            if (Presets.Any())
            {
                SelectedPreset = Presets.First();
            }
        }

        /// <summary>
        /// Загружает выбранный предустановленный пример в поля ввода
        /// </summary>
        /// <param name="preset">Пример для загрузки</param>
        /// <remarks>
        /// Автоматически вызывается при выборе примера из списка
        /// </remarks>
        private void LoadPreset(PresetExample preset)
        {
            CodeInput = preset.Code;
            PostconditionInput = preset.Postcondition;
            PostconditionHumanInput = preset.Description;
        }

        /// <summary>
        /// Проверяет, можно ли запустить вычисление weakest precondition
        /// </summary>
        /// <returns>
        /// True если вычисление можно запустить, False если нет
        /// </returns>
        /// <remarks>
        /// Для запуска вычисления нужно:
        /// - Не быть в процессе вычисления
        /// - Иметь непустой код программы
        /// - Иметь непустое постусловие
        /// </remarks>
        private bool CanCalculateWp()
        {
            return !IsCalculating &&
                   !string.IsNullOrWhiteSpace(CodeInput) &&
                   !string.IsNullOrWhiteSpace(PostconditionInput);
        }

        /// <summary>
        /// Основной метод вычисления weakest precondition
        /// </summary>
        /// <remarks>
        /// Этот метод:
        /// 1. Парсит введенный код и постусловие
        /// 2. Вычисляет weakest precondition с пошаговой трассировкой
        /// 3. Сохраняет результаты и шаги расчета
        /// 4. Обрабатывает ошибки парсинга и вычислений
        /// </remarks>
        private void CalculateWp()
        {
            IsCalculating = true;
            CurrentResult = null;

            try
            {
                var result = new WpResult
                {
                    OriginalCode = CodeInput,
                    OriginalPostcondition = PostconditionInput
                };

                // Парсим код и постусловие
                var statement = Parser.ParseStatement(CodeInput);
                var postcondition = Parser.ParsePredicate(PostconditionInput);

                result.Steps.Add(new WpCalculationStep(
                    "Исходные данные",
                    $"Программа: {CodeInput}",
                    $"Постусловие: {PostconditionInput}",
                    $"Постусловие: {postcondition.ToHumanReadable()}"
                ));

                // Вычисляем wp с пошаговым трейсом
                var finalPrecondition = CalculateWpWithTrace(statement, postcondition, result.Steps);

                result.FinalPrecondition = finalPrecondition;

                result.Steps.Add(new WpCalculationStep(
                    "Итоговое предусловие",
                    "Завершение вычисления",
                    finalPrecondition.ToString(),
                    $"Финальное предусловие: {finalPrecondition.ToHumanReadable()}"
                ));

                CurrentResult = result;
            }
            catch (Exception ex)
            {
                CurrentResult = new WpResult
                {
                    HasErrors = true,
                    ErrorMessage = ex.Message,
                    OriginalCode = CodeInput,
                    OriginalPostcondition = PostconditionInput
                };
            }
            finally
            {
                IsCalculating = false;
            }
        }

        /// <summary>
        /// Вычисляет weakest precondition с сохранением шагов расчета
        /// </summary>
        /// <param name="statement">Оператор для обработки</param>
        /// <param name="postcondition">Постусловие для этого оператора</param>
        /// <param name="steps">Список для сохранения шагов расчета</param>
        /// <returns>Предусловие для данного оператора и постусловия</returns>
        /// <remarks>
        /// Рекурсивно обрабатывает разные типы операторов:
        /// - Присваивания: делают подстановку переменной
        /// - Последовательности: обрабатывают операторы с конца
        /// - Условные операторы: объединяют условия из обеих веток
        /// </remarks>
        private Predicate CalculateWpWithTrace(Statement statement, Predicate postcondition, List<WpCalculationStep> steps)
        {
            switch (statement)
            {
                case Assignment assignment:
                    steps.Add(new WpCalculationStep(
                        "Обработка присваивания",
                        assignment.ToString(),
                        $"Заменяем {assignment.Variable} на {assignment.Expression} в {postcondition}",
                        $"Применяем правило для присваивания: {assignment.ToHumanReadable()}"
                    ));

                    var result = assignment.WeakestPrecondition(postcondition);

                    steps.Add(new WpCalculationStep(
                        "Результат подстановки",
                        assignment.ToString(),
                        result.ToString(),
                        result.ToHumanReadable()
                    ));

                    return result;

                case Sequence sequence:
                    steps.Add(new WpCalculationStep(
                        "Начало обработки последовательности",
                        sequence.ToString(),
                        postcondition.ToString(),
                        "Обрабатываем операторы с конца к началу"
                    ));

                    var currentPredicate = postcondition;

                    // Обрабатываем операторы с конца
                    for (int i = sequence.Statements.Count - 1; i >= 0; i--)
                    {
                        var stmt = sequence.Statements[i];

                        steps.Add(new WpCalculationStep(
                            $"Обработка оператора {sequence.Statements.Count - i} с конца",
                            stmt.ToString(),
                            currentPredicate.ToString(),
                            $"Текущее условие: {currentPredicate.ToHumanReadable()}"
                        ));

                        currentPredicate = CalculateWpWithTrace(stmt, currentPredicate, steps);
                    }

                    return currentPredicate;

                case Conditional conditional:
                    steps.Add(new WpCalculationStep(
                        "Начало обработки условного оператора",
                        conditional.ToString(),
                        postcondition.ToString(),
                        "Применяем правило для if-else: (B ∧ wp(S1,R)) ∨ (¬B ∧ wp(S2,R))"
                    ));

                    // Обрабатываем then-ветку
                    steps.Add(new WpCalculationStep(
                        "Обработка THEN-ветки",
                        conditional.ThenBranch.ToString(),
                        postcondition.ToString(),
                        "Вычисляем wp для ветки 'тогда'"
                    ));

                    var thenWp = CalculateWpWithTrace(conditional.ThenBranch, postcondition, steps);

                    // Обрабатываем else-ветку  
                    steps.Add(new WpCalculationStep(
                        "Обработка ELSE-ветки",
                        conditional.ElseBranch.ToString(),
                        postcondition.ToString(),
                        "Вычисляем wp для ветки 'иначе'"
                    ));

                    var elseWp = CalculateWpWithTrace(conditional.ElseBranch, postcondition, steps);

                    // Формируем итоговое условие
                    var condition = conditional.Condition;
                    var notCondition = new NotPredicate(condition);
                    var thenCondition = new LogicalPredicate(condition, "∧", thenWp);
                    var elseCondition = new LogicalPredicate(notCondition, "∧", elseWp);
                    var finalCondition = new LogicalPredicate(thenCondition, "∨", elseCondition);

                    steps.Add(new WpCalculationStep(
                        "Формирование итогового условия",
                        conditional.ToString(),
                        finalCondition.ToString(),
                        $"Объединяем ветки: ({condition.ToHumanReadable()} И {thenWp.ToHumanReadable()}) ИЛИ (НЕ({condition.ToHumanReadable()}) И {elseWp.ToHumanReadable()})"
                    ));

                    return finalCondition;

                default:
                    throw new ArgumentException($"Неизвестный тип оператора: {statement.GetType().Name}");
            }
        }

        /// <summary>
        /// Показывает триаду Хоара в отдельном окне сообщения
        /// </summary>
        /// <remarks>
        /// Триада Хоара отображается в формате: {предусловие} программа {постусловие}
        /// </remarks>
        private void ShowTriple()
        {
            if (CurrentResult != null && !CurrentResult.HasErrors)
            {
                System.Windows.MessageBox.Show(
                    CurrentResult.GetHoareTriple(),
                    "Триада Хоара",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Information
                );
            }
        }

        /// <summary>
        /// Очищает все поля ввода и результаты
        /// </summary>
        private void Clear()
        {
            CodeInput = "";
            PostconditionInput = "";
            PostconditionHumanInput = "";
            CurrentResult = null;
            SelectedPreset = null;
        }

        /// <summary>
        /// Показывает окно справки с информацией о weakest precondition
        /// </summary>
        private void ShowHelp()
        {
            var helpWindow = new lab2.Views.HelpWindow();
            helpWindow.ShowDialog();
        }
    }
}