using System;
using System.Collections.Generic;

namespace lab2.Models
{
    // Базовый класс для всех математических выражений
    // Например: x, 5, x + 3, (a * b)
    public abstract class Expression
    {
        // Превращает выражение в текст
        public abstract override string ToString();
        
        // Заменяет переменную на другое выражение
        // Например: x + 3, заменить x на (y * 2) = (y * 2) + 3
        public abstract Expression ReplaceVariable(string variableName, Expression newValue);
        
        // Возвращает список всех переменных в выражении
        public abstract List<string> GetAllVariables();
        
        // Проверяет, можно ли вычислить выражение при данных значениях
        public abstract bool CanCalculate(Dictionary<string, double> variableValues);
    }

    // Класс для переменных (например: x, y, max)
    public class Variable : Expression
    {
        // Имя переменной
        public string Name { get; }

        public Variable(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Имя переменной не может быть пустым");
            Name = name;
        }

        public override string ToString()
        {
            return Name;
        }

        public override Expression ReplaceVariable(string variableName, Expression newValue)
        {
            // Если это та переменная, которую нужно заменить
            if (variableName == Name)
                return newValue;
            else
                return this; // Оставляем как есть
        }

        public override List<string> GetAllVariables()
        {
            return new List<string> { Name };
        }

        public override bool CanCalculate(Dictionary<string, double> variableValues)
        {
            return variableValues.ContainsKey(Name);
        }
    }

    // Класс для чисел (например: 5, 3.14, -10)
    public class Constant : Expression
    {
        // Значение числа
        public double Value { get; }

        public Constant(double value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public override Expression ReplaceVariable(string variableName, Expression newValue)
        {
            // Число не меняется
            return this;
        }

        public override List<string> GetAllVariables()
        {
            // Число не содержит переменных
            return new List<string>();
        }

        public override bool CanCalculate(Dictionary<string, double> variableValues)
        {
            // Число всегда можно вычислить
            return true;
        }
    }

    // Класс для операций с двумя частями (например: x + 3, a * b)
    public class BinaryOperation : Expression
    {
        // Левая часть (например: x в x + 3)
        public Expression Left { get; }
        
        // Операция (+, -, *, /)
        public string Operator { get; }
        
        // Правая часть (например: 3 в x + 3)
        public Expression Right { get; }

        public BinaryOperation(Expression left, string operation, Expression right)
        {
            if (left == null)
                throw new ArgumentException("Левая часть не может быть пустой");
            if (string.IsNullOrEmpty(operation))
                throw new ArgumentException("Операция не может быть пустой");
            if (right == null)
                throw new ArgumentException("Правая часть не может быть пустой");
                
            Left = left;
            Operator = operation;
            Right = right;
        }

        public override string ToString()
        {
            return $"({Left} {Operator} {Right})";
        }

        public override Expression ReplaceVariable(string variableName, Expression newValue)
        {
            // Заменяем переменную в обеих частях
            Expression newLeft = Left.ReplaceVariable(variableName, newValue);
            Expression newRight = Right.ReplaceVariable(variableName, newValue);
            return new BinaryOperation(newLeft, Operator, newRight);
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

        public override bool CanCalculate(Dictionary<string, double> variableValues)
        {
            // Проверяем обе части
            bool leftOk = Left.CanCalculate(variableValues);
            bool rightOk = Right.CanCalculate(variableValues);

            // Проверяем деление на ноль
            if (Operator == "/" && Right is Constant constant && constant.Value == 0)
                return false;

            return leftOk && rightOk;
        }
    }

    // Класс для операций с одной частью (например: -x, abs(y))
    public class UnaryOperation : Expression
    {
        // Операция (-, abs)
        public string Operator { get; }
        
        // Выражение, к которому применяется операция
        public Expression Operand { get; }

        public UnaryOperation(string operation, Expression operand)
        {
            if (string.IsNullOrEmpty(operation))
                throw new ArgumentException("Операция не может быть пустой");
            if (operand == null)
                throw new ArgumentException("Выражение не может быть пустым");
                
            Operator = operation;
            Operand = operand;
        }

        public override string ToString()
        {
            return $"{Operator}({Operand})";
        }

        public override Expression ReplaceVariable(string variableName, Expression newValue)
        {
            // Заменяем переменную внутри выражения
            Expression newOperand = Operand.ReplaceVariable(variableName, newValue);
            return new UnaryOperation(Operator, newOperand);
        }

        public override List<string> GetAllVariables()
        {
            return Operand.GetAllVariables();
        }

        public override bool CanCalculate(Dictionary<string, double> variableValues)
        {
            return Operand.CanCalculate(variableValues);
        }
    }
}
