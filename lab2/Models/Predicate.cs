using System;
using System.Collections.Generic;

namespace lab2.Models
{
    // Базовый класс для всех условий
    // Например: x > 5, y == 10, a > 0 И b < 20
    public abstract class Predicate
    {
        // Превращает условие в текст
        public abstract override string ToString();
        
        // Заменяет переменную на выражение
        // Например: x > 5, заменить x на (y + 2) = (y + 2) > 5
        public abstract Predicate ReplaceVariable(string variableName, Expression newValue);
        
        // Превращает условие в понятный человеческий язык
        public abstract string ToHumanReadable();
        
        // Возвращает список всех переменных в условии
        public abstract List<string> GetAllVariables();
    }

    // Класс для условий сравнения (например: x > 5, y == 10)
    public class ComparisonPredicate : Predicate
    {
        // Левая часть сравнения (например: x в x > 5)
        public Expression Left { get; }
        
        // Знак сравнения (>, <, ==, !=, >=, <=)
        public string Operator { get; }
        
        // Правая часть сравнения (например: 5 в x > 5)
        public Expression Right { get; }

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

        public override string ToString()
        {
            return $"{Left} {Operator} {Right}";
        }

        public override Predicate ReplaceVariable(string variableName, Expression newValue)
        {
            // Заменяем переменную в обеих частях
            Expression newLeft = Left.ReplaceVariable(variableName, newValue);
            Expression newRight = Right.ReplaceVariable(variableName, newValue);
            return new ComparisonPredicate(newLeft, Operator, newRight);
        }

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

    // Класс для логических операций (например: x > 5 И y < 10)
    public class LogicalPredicate : Predicate
    {
        // Левое условие
        public Predicate Left { get; }
        
        // Логическая операция (И, ИЛИ)
        public string Operator { get; }
        
        // Правое условие
        public Predicate Right { get; }

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

        public override string ToString()
        {
            return $"({Left} {Operator} {Right})";
        }

        public override Predicate ReplaceVariable(string variableName, Expression newValue)
        {
            // Заменяем переменную в обоих условиях
            Predicate newLeft = Left.ReplaceVariable(variableName, newValue);
            Predicate newRight = Right.ReplaceVariable(variableName, newValue);
            return new LogicalPredicate(newLeft, Operator, newRight);
        }

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

    // Класс для отрицания (например: НЕ(x > 5))
    public class NotPredicate : Predicate
    {
        // Условие, которое отрицаем
        public Predicate Operand { get; }

        public NotPredicate(Predicate condition)
        {
            if (condition == null)
                throw new ArgumentException("Условие не может быть пустым");
                
            Operand = condition;
        }

        public override string ToString()
        {
            return $"¬({Operand})";
        }

        public override Predicate ReplaceVariable(string variableName, Expression newValue)
        {
            // Заменяем переменную внутри условия
            Predicate newOperand = Operand.ReplaceVariable(variableName, newValue);
            return new NotPredicate(newOperand);
        }

        public override string ToHumanReadable()
        {
            return $"НЕ ({Operand.ToHumanReadable()})";
        }

        public override List<string> GetAllVariables()
        {
            return Operand.GetAllVariables();
        }
    }

    // Класс для условия "Истина" (всегда выполняется)
    public class TruePredicate : Predicate
    {
        // Один экземпляр на всё приложение
        public static readonly TruePredicate Instance = new TruePredicate();

        private TruePredicate() { }

        public override string ToString()
        {
            return "true";
        }

        public override Predicate ReplaceVariable(string variableName, Expression newValue)
        {
            // Истина никогда не меняется
            return this;
        }

        public override string ToHumanReadable()
        {
            return "истина";
        }

        public override List<string> GetAllVariables()
        {
            // Истина не содержит переменных
            return new List<string>();
        }
    }

    // Класс для условия "Ложь" (никогда не выполняется)
    public class FalsePredicate : Predicate
    {
        // Один экземпляр на всё приложение
        public static readonly FalsePredicate Instance = new FalsePredicate();

        private FalsePredicate() { }

        public override string ToString()
        {
            return "false";
        }

        public override Predicate ReplaceVariable(string variableName, Expression newValue)
        {
            // Ложь никогда не меняется
            return this;
        }

        public override string ToHumanReadable()
        {
            return "ложь";
        }

        public override List<string> GetAllVariables()
        {
            // Ложь не содержит переменных
            return new List<string>();
        }
    }
}
