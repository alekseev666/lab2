using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lab2.Models
{
    /// <summary>
    /// Один шаг в процессе вычисления weakest precondition
    /// </summary>
    /// <remarks>
    /// Хранит информацию о том, что происходило на каждом этапе расчета.
    /// Это как "кадр фильма" - показывает состояние расчета в конкретный момент.
    /// </remarks>
    public class WpCalculationStep
    {
        /// <summary>
        /// Описание того, что происходит на этом шаге
        /// </summary>
        /// <example>
        /// "Обработка присваивания", "Вычисление weakest precondition для условия", "Подстановка переменной"
        /// </example>
        public string Description { get; set; }

        /// <summary>
        /// Оператор, который обрабатывается на этом шаге
        /// </summary>
        /// <example>"x := y + 5", "if (x > 0) { ... } else { ... }"</example>
        public string StatementProcessed { get; set; }

        /// <summary>
        /// Промежуточное условие после обработки оператора
        /// </summary>
        /// <example>"y + 5 > 10", "(x > 0 ∧ y == 1) ∨ (x ≤ 0 ∧ y == 0)"</example>
        public string IntermediatePredicate { get; set; }

        /// <summary>
        /// Промежуточное условие в понятном человеческом языке
        /// </summary>
        /// <example>"y + 5 больше 10", "если x больше 0, то y равно 1, иначе y равно 0"</example>
        public string HumanReadable { get; set; }

        /// <summary>
        /// Создает новый шаг расчета
        /// </summary>
        /// <param name="description">Описание шага</param>
        /// <param name="statement">Обрабатываемый оператор</param>
        /// <param name="predicate">Промежуточное условие</param>
        /// <param name="humanReadable">Условие в понятной форме</param>
        public WpCalculationStep(string description, string statement, string predicate, string humanReadable)
        {
            Description = description;
            StatementProcessed = statement;
            IntermediatePredicate = predicate;
            HumanReadable = humanReadable;
        }
    }

    /// <summary>
    /// Результат вычисления weakest precondition
    /// </summary>
    /// <remarks>
    /// Содержит итоговое предусловие и всю историю расчета.
    /// Это как "отчет" о том, как вычислялось weakest precondition.
    /// </remarks>
    public class WpResult
    {
        /// <summary>
        /// Итоговое предусловие, которое должно выполняться до программы
        /// </summary>
        /// <example>
        /// Для программы "x := y + 5" и постусловия "x > 10" 
        /// FinalPrecondition будет "y + 5 > 10"
        /// </example>
        public Predicate FinalPrecondition { get; set; }

        /// <summary>
        /// Список всех шагов расчета от постусловия к предусловию
        /// </summary>
        /// <remarks>
        /// Позволяет проследить, как условие "протягивалось" через программу назад
        /// </remarks>
        public List<WpCalculationStep> Steps { get; set; }

        /// <summary>
        /// Исходный код программы, для которой вычислялось weakest precondition
        /// </summary>
        public string OriginalCode { get; set; }

        /// <summary>
        /// Исходное постусловие, которое должно выполняться после программы
        /// </summary>
        public string OriginalPostcondition { get; set; }

        /// <summary>
        /// Были ли ошибки при вычислении
        /// </summary>
        /// <example>true если были ошибки разбора или вычисления</example>
        public bool HasErrors { get; set; }

        /// <summary>
        /// Сообщение об ошибке, если вычисление не удалось
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Создает новый пустой результат
        /// </summary>
        public WpResult()
        {
            Steps = new List<WpCalculationStep>();
            HasErrors = false;
        }

        /// <summary>
        /// Создает триаду Хоара в красивом формате
        /// </summary>
        /// <returns>
        /// Триада Хоара в формате: {предусловие} программа {постусловие}
        /// </returns>
        /// <remarks>
        /// Триада Хоара - это стандартный способ записи условий корректности программы.
        /// Формат: {P} S {Q}, где P - предусловие, S - программа, Q - постусловие.
        /// Это означает: "если P истинно до выполнения S, то Q будет истинно после выполнения S".
        /// </remarks>
        /// <example>
        /// {y + 5 > 10} x := y + 5 {x > 10}
        /// </example>
        public string GetHoareTriple()
        {
            if (HasErrors || FinalPrecondition == null)
                return "Невозможно построить триаду Хоара из-за ошибок";

            return $"{{ {FinalPrecondition.ToHumanReadable()} }} {OriginalCode} {{ {GetPostconditionHumanReadable()} }}";
        }

        /// <summary>
        /// Преобразует постусловие в понятный человеческий язык
        /// </summary>
        /// <returns>Постусловие простыми словами</returns>
        private string GetPostconditionHumanReadable()
        {
            // Простая попытка сделать постусловие читаемым
            return OriginalPostcondition?.Replace("&&", " И ")
                                        ?.Replace("||", " ИЛИ ")
                                        ?.Replace("==", " равно ")
                                        ?.Replace(">=", " больше или равно ")
                                        ?.Replace("<=", " меньше или равно ")
                                        ?.Replace(">", " больше ")
                                        ?.Replace("<", " меньше ")
                                        ?? "неизвестное постусловие";
        }
    }

    /// <summary>
    /// Предустановленный пример для демонстрации weakest precondition
    /// </summary>
    /// <remarks>
    /// Содержит готовые примеры программ с постусловиями для обучения и тестирования.
    /// Это как "заготовки" для быстрого запуска примеров.
    /// </remarks>
    public class PresetExample
    {
        /// <summary>
        /// Название примера
        /// </summary>
        /// <example>"Максимум из двух", "Деление с проверкой"</example>
        public string Name { get; set; }

        /// <summary>
        /// Подробное описание того, что делает пример
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Код программы для анализа
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Постусловие, которое должно выполняться после программы
        /// </summary>
        public string Postcondition { get; set; }

        /// <summary>
        /// Создает новый предустановленный пример
        /// </summary>
        /// <param name="name">Название примера</param>
        /// <param name="description">Описание примера</param>
        /// <param name="code">Код программы</param>
        /// <param name="postcondition">Постусловие</param>
        public PresetExample(string name, string description, string code, string postcondition)
        {
            Name = name;
            Description = description;
            Code = code;
            Postcondition = postcondition;
        }

        /// <summary>
        /// Возвращает список стандартных примеров для демонстрации
        /// </summary>
        /// <returns>Список предустановленных примеров</returns>
        /// <remarks>
        /// Эти примеры показывают разные аспекты weakest precondition:
        /// - Подстановка в присваиваниях
        /// - Условные операторы
        /// - Последовательности операторов
        /// - Проверки определенности
        /// </remarks>
        public static List<PresetExample> GetDefaultPresets()
        {
            return new List<PresetExample>
            {
                new PresetExample(
                    "Максимум из двух",
                    "Находит максимум из двух переменных x1 и x2",
                    "if (x1 >= x2) { max := x1 } else { max := x2 }",
                    "max > 100"
                ),
                new PresetExample(
                    "Последовательность присваиваний",
                    "Демонстрация протягивания условия через цепочку присваиваний",
                    "x := x + 10; y := x + 1",
                    "y == x - 9 && x > 15"
                ),
                new PresetExample(
                    "Квадратное уравнение (упрощенное)",
                    "Вычисление корня квадратного уравнения при положительном дискриминанте",
                    "if (d >= 0) { root := (-b + d) / (2 * a) } else { root := -999 }",
                    "root != -999"
                ),
                new PresetExample(
                    "Деление с проверкой",
                    "Демонстрация условий определенности для деления",
                    "if (y != 0) { result := x / y } else { result := 0 }",
                    "result > 5"
                ),
                new PresetExample(
                    "Простое присваивание",
                    "Базовый случай для понимания подстановки",
                    "x := 2 * x + 5",
                    "x > 15"
                )
            };
        }
    }
}