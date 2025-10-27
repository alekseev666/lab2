using System;
using System.Collections.Generic;

namespace lab2.Models
{
    /// <summary>
    /// Базовый класс для всех условий в программе
    /// </summary>
    /// <remarks>
    /// Это как шаблон для любых проверок и условий: сравнения, логические операции и т.д.
    /// Например: x > 5, y == 10, a > 0 И b < 20
    /// </remarks>
    public abstract class Predicate
    {
        public abstract override string ToString();
        public abstract Predicate ReplaceVariable(string variableName, Expression newValue);
        public abstract string ToHumanReadable();
        public abstract List<string> GetAllVariables();
        public abstract Predicate Simplify();
    }

    /// <summary>
    /// Класс для простых сравнений двух выражений
    /// </summary>
    /// <remarks>
    /// Используется для сравнений типа: больше, меньше, равно, не равно и т.д.
    /// Например: x > 5, y == 10, a + b <= 20
    /// </remarks>
    public class ComparisonPredicate : Predicate
    {
        /// <summary>
        /// Левая часть сравнения
        /// </summary>
        /// <example>В "x > 5" левая часть - переменная x</example>
        public Expression Left { get; }

        /// <summary>
        /// Знак сравнения: >, <, ==, !=, >=, <=
        /// </summary>
        public string Operator { get; }

        /// <summary>
        /// Правая часть сравнения
        /// </summary>
        /// <example>В "x > 5" правая часть - число 5</example>
        public Expression Right { get; }

        /// <summary>
        /// Создает новое условие сравнения
        /// </summary>
        /// <param name="left">Левая часть сравнения</param>
        /// <param name="comparison">Знак сравнения</param>
        /// <param name="right">Правая часть сравнения</param>
        /// <exception cref="ArgumentException">Выбрасывается если какая-то часть отсутствует</exception>
        public ComparisonPredicate(Expression left, string comparison, Expression right)
        {
            if (left == null)
                throw new ArgumentException("Левая часть не может быть пустой");
            if (string.IsNullOrEmpty(comparison))
                throw new ArgumentException("Знак сравнения не может быть пустым");
            if (right == null)
                throw new ArgumentException("Правая часть не может быть пустой");

            Left = left;
            Operator = comparison;
            Right = right;
        }

        /// <summary>
        /// Возвращает сравнение в виде текста
        /// </summary>
        /// <returns>Сравнение как текст, например "x > 5"</returns>
        public override string ToString()
        {
            return $"{Left} {Operator} {Right}";
        }

        public override Predicate Simplify()
        {
            var l = Left.Simplify();
            var r = Right.Simplify();
            if (l is Constant lc && r is Constant rc)
            {
                bool res = Operator switch
                {
                    ">" => lc.Value > rc.Value,
                    "<" => lc.Value < rc.Value,
                    ">=" => lc.Value >= rc.Value,
                    "<=" => lc.Value <= rc.Value,
                    "==" => Math.Abs(lc.Value - rc.Value) < 1e-9,
                    "!=" => Math.Abs(lc.Value - rc.Value) >= 1e-9,
                    _ => false
                };
                return res ? TruePredicate.Instance : FalsePredicate.Instance;
            }
            return new ComparisonPredicate(l, Operator, r);
        }

        /// <summary>
        /// Заменяет переменную в обеих частях сравнения
        /// </summary>
        /// <param name="variableName">Имя переменной для замены</param>
        /// <param name="newValue">Выражение, которое подставится вместо переменной</param>
        /// <returns>Новое сравнение с замененными переменными</returns>
        public override Predicate ReplaceVariable(string variableName, Expression newValue)
        {
            Expression newLeft = Left.ReplaceVariable(variableName, newValue);
            Expression newRight = Right.ReplaceVariable(variableName, newValue);
            return new ComparisonPredicate(newLeft, Operator, newRight);
        }

        /// <summary>
        /// Превращает сравнение в понятный человеческий текст
        /// </summary>
        /// <returns>Сравнение простыми словами на русском</returns>
        public override string ToHumanReadable()
        {
            string leftText = Left.ToString();
            string rightText = Right.ToString();

            // Простое преобразование знаков сравнения
            if (Operator == ">")
                return $"{leftText} больше {rightText}";
            else if (Operator == "<")
                return $"{leftText} меньше {rightText}";
            else if (Operator == ">=")
                return $"{leftText} больше или равно {rightText}";
            else if (Operator == "<=")
                return $"{leftText} меньше или равно {rightText}";
            else if (Operator == "==")
                return $"{leftText} равно {rightText}";
            else if (Operator == "!=")
                return $"{leftText} не равно {rightText}";
            else
                return $"{leftText} {Operator} {rightText}";
        }

        /// <summary>
        /// Находит все переменные в левой и правой части сравнения
        /// </summary>
        /// <returns>Список всех уникальных переменных из обеих частей</returns>
        public override List<string> GetAllVariables()
        {
            // Собираем переменные из обеих частей
            List<string> variables = new List<string>();
            variables.AddRange(Left.GetAllVariables());
            variables.AddRange(Right.GetAllVariables());

            // Убираем повторы
            List<string> uniqueVariables = new List<string>();
            foreach (string variable in variables)
            {
                if (!uniqueVariables.Contains(variable))
                    uniqueVariables.Add(variable);
            }

            return uniqueVariables;
        }
    }

    /// <summary>
    /// Класс для объединения двух условий логическими операциями
    /// </summary>
    /// <remarks>
    /// Используется для создания сложных условий с помощью И/ИЛИ
    /// Например: x > 5 И y < 10, a == 0 ИЛИ b != 1
    /// </remarks>
    public class LogicalPredicate : Predicate
    {
        /// <summary>
        /// Левое условие
        /// </summary>
        /// <example>В "x > 5 И y < 10" левое условие - "x > 5"</example>
        public Predicate Left { get; }

        /// <summary>
        /// Логическая операция: И (&&, ∧) или ИЛИ (||, ∨)
        /// </summary>
        public string Operator { get; }

        /// <summary>
        /// Правое условие
        /// </summary>
        /// <example>В "x > 5 И y < 10" правое условие - "y < 10"</example>
        public Predicate Right { get; }

        /// <summary>
        /// Создает новое логическое условие
        /// </summary>
        /// <param name="left">Левое условие</param>
        /// <param name="logicalOperation">Логическая операция</param>
        /// <param name="right">Правое условие</param>
        /// <exception cref="ArgumentException">Выбрасывается если какое-то условие отсутствует</exception>
        public LogicalPredicate(Predicate left, string logicalOperation, Predicate right)
        {
            if (left == null)
                throw new ArgumentException("Левое условие не может быть пустым");
            if (string.IsNullOrEmpty(logicalOperation))
                throw new ArgumentException("Логическая операция не может быть пустой");
            if (right == null)
                throw new ArgumentException("Правое условие не может быть пустым");

            Left = left;
            Operator = logicalOperation;
            Right = right;
        }

        /// <summary>
        /// Возвращает логическое условие в виде текста в скобках
        /// </summary>
        /// <returns>Логическое условие как текст, например "(x > 5 ∧ y < 10)"</returns>
        public override string ToString()
        {
            return $"({Left} {Operator} {Right})";
        }

        public override Predicate Simplify()
        {
            var l = Left.Simplify();
            var r = Right.Simplify();
            bool isAnd = Operator == "&&" || Operator == "∧";
            bool isOr = Operator == "||" || Operator == "∨";

            if (isAnd)
            {
                if (l is FalsePredicate || r is FalsePredicate) return FalsePredicate.Instance;
                if (l is TruePredicate) return r;
                if (r is TruePredicate) return l;
            }
            if (isOr)
            {
                if (l is TruePredicate || r is TruePredicate) return TruePredicate.Instance;
                if (l is FalsePredicate) return r;
                if (r is FalsePredicate) return l;
            }
            return new LogicalPredicate(l, Operator, r);
        }

        /// <summary>
        /// Заменяет переменную в обоих условиях
        /// </summary>
        /// <param name="variableName">Имя переменной для замены</param>
        /// <param name="newValue">Выражение, которое подставится вместо переменной</param>
        /// <returns>Новое логическое условие с замененными переменными</returns>
        public override Predicate ReplaceVariable(string variableName, Expression newValue)
        {
            Predicate newLeft = Left.ReplaceVariable(variableName, newValue);
            Predicate newRight = Right.ReplaceVariable(variableName, newValue);
            return new LogicalPredicate(newLeft, Operator, newRight);
        }

        /// <summary>
        /// Превращает логическое условие в понятный человеческий текст
        /// </summary>
        /// <returns>Логическое условие простыми словами на русском</returns>
        public override string ToHumanReadable()
        {
            string leftText = Left.ToHumanReadable();
            string rightText = Right.ToHumanReadable();

            // Простое преобразование логических операций
            if (Operator == "&&" || Operator == "∧")
                return $"({leftText}) И ({rightText})";
            else if (Operator == "||" || Operator == "∨")
                return $"({leftText}) ИЛИ ({rightText})";
            else
                return $"({leftText}) {Operator} ({rightText})";
        }

        /// <summary>
        /// Находит все переменные в левом и правом условии
        /// </summary>
        /// <returns>Список всех уникальных переменных из обоих условий</returns>
        public override List<string> GetAllVariables()
        {
            // Собираем переменные из обоих условий
            List<string> variables = new List<string>();
            variables.AddRange(Left.GetAllVariables());
            variables.AddRange(Right.GetAllVariables());

            // Убираем повторы
            List<string> uniqueVariables = new List<string>();
            foreach (string variable in variables)
            {
                if (!uniqueVariables.Contains(variable))
                    uniqueVariables.Add(variable);
            }

            return uniqueVariables;
        }
    }

    /// <summary>
    /// Класс для отрицания условия
    /// </summary>
    /// <remarks>
    /// Используется когда нужно проверить, что условие НЕ выполняется
    /// Например: НЕ(x > 5) означает, что x не больше 5 (x <= 5)
    /// </remarks>
    public class NotPredicate : Predicate
    {
        /// <summary>
        /// Условие, которое мы отрицаем
        /// </summary>
        public Predicate Operand { get; }

        /// <summary>
        /// Создает новое условие отрицания
        /// </summary>
        /// <param name="condition">Условие для отрицания</param>
        /// <exception cref="ArgumentException">Выбрасывается если условие отсутствует</exception>
        public NotPredicate(Predicate condition)
        {
            if (condition == null)
                throw new ArgumentException("Условие не может быть пустым");

            Operand = condition;
        }

        /// <summary>
        /// Возвращает отрицание в виде текста
        /// </summary>
        /// <returns>Отрицание как текст, например "¬(x > 5)"</returns>
        public override string ToString()
        {
            return $"¬({Operand})";
        }

        public override Predicate Simplify()
        {
            var inner = Operand.Simplify();
            if (inner is TruePredicate) return FalsePredicate.Instance;
            if (inner is FalsePredicate) return TruePredicate.Instance;
            if (inner is NotPredicate notInner) return notInner.Operand.Simplify();
            return new NotPredicate(inner);
        }

        /// <summary>
        /// Заменяет переменную внутри отрицаемого условия
        /// </summary>
        /// <param name="variableName">Имя переменной для замены</param>
        /// <param name="newValue">Выражение, которое подставится вместо переменной</param>
        /// <returns>Новое отрицание с замененной переменной</returns>
        public override Predicate ReplaceVariable(string variableName, Expression newValue)
        {
            Predicate newOperand = Operand.ReplaceVariable(variableName, newValue);
            return new NotPredicate(newOperand);
        }

        /// <summary>
        /// Превращает отрицание в понятный человеческий текст
        /// </summary>
        /// <returns>Отрицание простыми словами на русском</returns>
        public override string ToHumanReadable()
        {
            return $"НЕ ({Operand.ToHumanReadable()})";
        }

        /// <summary>
        /// Находит все переменные внутри отрицаемого условия
        /// </summary>
        /// <returns>Список всех переменных из внутреннего условия</returns>
        public override List<string> GetAllVariables()
        {
            return Operand.GetAllVariables();
        }
    }

    /// <summary>
    /// Класс для условия, которое всегда выполняется
    /// </summary>
    /// <remarks>
    /// Это как "да" или "всегда правда". Используется когда условие всегда должно проходить.
    /// Например, в упрощенных проверках или как значение по умолчанию.
    /// </remarks>
    public class TruePredicate : Predicate
    {
        /// <summary>
        /// Единственный экземпляр этого класса на всю программу
        /// </summary>
        /// <remarks>
        /// Используем один экземпляр, потому что все "истины" одинаковы
        /// </remarks>
        public static readonly TruePredicate Instance = new TruePredicate();

        // Скрытый конструктор, чтобы нельзя было создать новый экземпляр
        private TruePredicate() { }

        /// <summary>
        /// Возвращает "true" как текст
        /// </summary>
        /// <returns>Всегда "true"</returns>
        public override string ToString()
        {
            return "true";
        }

        /// <summary>
        /// Истина никогда не меняется при замене переменных
        /// </summary>
        /// <param name="variableName">Имя переменной (игнорируется)</param>
        /// <param name="newValue">Новое значение (игнорируется)</param>
        /// <returns>Всегда эту же истину</returns>
        public override Predicate ReplaceVariable(string variableName, Expression newValue)
        {
            return this;
        }

        public override Predicate Simplify()
        {
            return this;
        }

        /// <summary>
        /// Превращает истину в понятный человеческий текст
        /// </summary>
        /// <returns>Всегда "истина"</returns>
        public override string ToHumanReadable()
        {
            return "истина";
        }

        /// <summary>
        /// Истина не содержит переменных
        /// </summary>
        /// <returns>Всегда пустой список</returns>
        public override List<string> GetAllVariables()
        {
            return new List<string>();
        }
    }

    /// <summary>
    /// Класс для условия, которое никогда не выполняется
    /// </summary>
    /// <remarks>
    /// Это как "нет" или "всегда ложь". Используется когда условие никогда не должно проходить.
    /// Например, в упрощенных проверках или как значение по умолчанию.
    /// </remarks>
    public class FalsePredicate : Predicate
    {
        /// <summary>
        /// Единственный экземпляр этого класса на всю программу
        /// </summary>
        /// <remarks>
        /// Используем один экземпляр, потому что все "лжи" одинаковы
        /// </remarks>
        public static readonly FalsePredicate Instance = new FalsePredicate();

        // Скрытый конструктор, чтобы нельзя было создать новый экземпляр
        private FalsePredicate() { }

        /// <summary>
        /// Возвращает "false" как текст
        /// </summary>
        /// <returns>Всегда "false"</returns>
        public override string ToString()
        {
            return "false";
        }

        /// <summary>
        /// Ложь никогда не меняется при замене переменных
        /// </summary>
        /// <param name="variableName">Имя переменной (игнорируется)</param>
        /// <param name="newValue">Новое значение (игнорируется)</param>
        /// <returns>Всегда эту же ложь</returns>
        public override Predicate ReplaceVariable(string variableName, Expression newValue)
        {
            return this;
        }

        public override Predicate Simplify()
        {
            return this;
        }

        /// <summary>
        /// Превращает ложь в понятный человеческий текст
        /// </summary>
        /// <returns>Всегда "ложь"</returns>
        public override string ToHumanReadable()
        {
            return "ложь";
        }

        /// <summary>
        /// Ложь не содержит переменных
        /// </summary>
        /// <returns>Всегда пустой список</returns>
        public override List<string> GetAllVariables()
        {
            return new List<string>();
        }
    }
}