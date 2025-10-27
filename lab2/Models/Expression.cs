using System;
using System.Collections.Generic;

namespace lab2.Models
{
    /// <summary>
    /// Базовый класс для всех математических выражений
    /// </summary>
    /// <remarks>
    /// Это как шаблон для любых математических выражений: чисел, переменных, сложения, умножения и т.д.
    /// Например: x, 5, x + 3, (a * b)
    /// </remarks>
    public abstract class Expression
    {
        public abstract override string ToString();
        public abstract Expression ReplaceVariable(string variableName, Expression newValue);
        public abstract List<string> GetAllVariables();
        public abstract bool CanCalculate(Dictionary<string, double> variableValues);
        public abstract Expression Simplify();
    }

    /// <summary>
    /// Представляет переменную в математическом выражении
    /// </summary>
    /// <remarks>
    /// Это как буква в математике: x, y, max, temperature и т.д.
    /// Её значение может меняться.
    /// </remarks>
    public class Variable : Expression
    {
        /// <summary>
        /// Имя переменной
        /// </summary>
        /// <example>"x", "y", "speed"</example>
        public string Name { get; }

        /// <summary>
        /// Создает новую переменную
        /// </summary>
        /// <param name="name">Имя переменной</param>
        /// <exception cref="ArgumentException">Выбрасывается если имя пустое или null</exception>
        public Variable(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Имя переменной не может быть пустым");
            Name = name;
        }

        /// <summary>
        /// Возвращает имя переменной в виде текста
        /// </summary>
        /// <returns>Имя переменной</returns>
        public override string ToString()
        {
            return Name;
        }

        public override Expression Simplify()
        {
            return this;
        }

        /// <summary>
        /// Заменяет эту переменную на другое выражение, если имена совпадают
        /// </summary>
        /// <param name="variableName">Имя переменной для замены</param>
        /// <param name="newValue">Выражение, которое подставится вместо переменной</param>
        /// <returns>
        /// Если это та самая переменная - возвращает newValue
        /// Если это другая переменная - возвращает эту же переменную без изменений
        /// </returns>
        public override Expression ReplaceVariable(string variableName, Expression newValue)
        {
            if (variableName == Name)
                return newValue;
            else
                return this;
        }

        /// <summary>
        /// Возвращает список с одной переменной - именем этой переменной
        /// </summary>
        /// <returns>Список с одним элементом - именем этой переменной</returns>
        public override List<string> GetAllVariables()
        {
            return new List<string> { Name };
        }

        /// <summary>
        /// Проверяет, есть ли значение для этой переменной в словаре
        /// </summary>
        /// <param name="variableValues">Словарь со значениями переменных</param>
        /// <returns>True если в словаре есть значение для этой переменной, иначе False</returns>
        public override bool CanCalculate(Dictionary<string, double> variableValues)
        {
            return variableValues.ContainsKey(Name);
        }
    }

    /// <summary>
    /// Представляет число в математическом выражении
    /// </summary>
    /// <remarks>
    /// Это обычное число: 5, 3.14, -10, 0.5
    /// Его значение всегда известно и не меняется.
    /// </remarks>
    public class Constant : Expression
    {
        /// <summary>
        /// Значение числа
        /// </summary>
        public double Value { get; }

        /// <summary>
        /// Создает новое число
        /// </summary>
        /// <param name="value">Значение числа</param>
        public Constant(double value)
        {
            Value = value;
        }

        /// <summary>
        /// Возвращает число в виде текста
        /// </summary>
        /// <returns>Число как текст</returns>
        public override string ToString()
        {
            return Value.ToString();
        }

        public override Expression Simplify()
        {
            return this;
        }

        /// <summary>
        /// Числа не содержат переменных, поэтому всегда возвращает это же число
        /// </summary>
        /// <param name="variableName">Имя переменной (игнорируется)</param>
        /// <param name="newValue">Новое значение (игнорируется)</param>
        /// <returns>Это же число без изменений</returns>
        public override Expression ReplaceVariable(string variableName, Expression newValue)
        {
            return this;
        }

        /// <summary>
        /// Числа не содержат переменных
        /// </summary>
        /// <returns>Пустой список</returns>
        public override List<string> GetAllVariables()
        {
            return new List<string>();
        }

        /// <summary>
        /// Число всегда можно вычислить - оно уже известно
        /// </summary>
        /// <param name="variableValues">Словарь со значениями (игнорируется)</param>
        /// <returns>Всегда True</returns>
        public override bool CanCalculate(Dictionary<string, double> variableValues)
        {
            return true;
        }
    }

    /// <summary>
    /// Представляет операцию с двумя частями
    /// </summary>
    /// <remarks>
    /// Это операции типа: сложение, вычитание, умножение, деление
    /// Например: x + 3, a * b, (x - y) / 2
    /// </remarks>
    public class BinaryOperation : Expression
    {
        /// <summary>
        /// Левая часть операции
        /// </summary>
        /// <example>В "x + 3" левая часть - переменная x</example>
        public Expression Left { get; }

        /// <summary>
        /// Операция: +, -, *, /
        /// </summary>
        public string Operator { get; }

        /// <summary>
        /// Правая часть операции
        /// </summary>
        /// <example>В "x + 3" правая часть - число 3</example>
        public Expression Right { get; }

        /// <summary>
        /// Создает новую операцию с двумя частями
        /// </summary>
        /// <param name="left">Левая часть операции</param>
        /// <param name="operation">Операция: +, -, *, /</param>
        /// <param name="right">Правая часть операции</param>
        /// <exception cref="ArgumentException">Выбрасывается если какая-то часть отсутствует</exception>
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

        /// <summary>
        /// Возвращает операцию в виде текста в скобках
        /// </summary>
        /// <returns>Операция как текст, например "(x + 3)"</returns>
        public override string ToString()
        {
            return $"({Left} {Operator} {Right})";
        }

        public override Expression Simplify()
        {
            var l = Left.Simplify();
            var r = Right.Simplify();
            if (l is Constant lc && r is Constant rc)
            {
                double val = Operator switch
                {
                    "+" => lc.Value + rc.Value,
                    "-" => lc.Value - rc.Value,
                    "*" => lc.Value * rc.Value,
                    "/" => rc.Value == 0 ? double.NaN : lc.Value / rc.Value,
                    _ => double.NaN
                };
                if (!double.IsNaN(val)) return new Constant(val);
            }
            // Algebraic identities
            if (Operator == "+")
            {
                if (l is Constant lc2 && lc2.Value == 0) return r;
                if (r is Constant rc2 && rc2.Value == 0) return l;
            }
            if (Operator == "-")
            {
                if (r is Constant rc3 && rc3.Value == 0) return l;
            }
            if (Operator == "*")
            {
                if ((l is Constant lc3 && lc3.Value == 0) || (r is Constant rc4 && rc4.Value == 0)) return new Constant(0);
                if (l is Constant lc4 && lc4.Value == 1) return r;
                if (r is Constant rc5 && rc5.Value == 1) return l;
            }
            if (Operator == "/")
            {
                if (r is Constant rc6 && rc6.Value == 1) return l;
            }
            return new BinaryOperation(l, Operator, r);
        }

        /// <summary>
        /// Заменяет переменную в обеих частях операции
        /// </summary>
        /// <param name="variableName">Имя переменной для замены</param>
        /// <param name="newValue">Выражение, которое подставится вместо переменной</param>
        /// <returns>Новая операция с замененными переменными</returns>
        public override Expression ReplaceVariable(string variableName, Expression newValue)
        {
            Expression newLeft = Left.ReplaceVariable(variableName, newValue);
            Expression newRight = Right.ReplaceVariable(variableName, newValue);
            return new BinaryOperation(newLeft, Operator, newRight);
        }

        /// <summary>
        /// Находит все переменные в левой и правой части
        /// </summary>
        /// <returns>Список всех уникальных переменных из обеих частей</returns>
        public override List<string> GetAllVariables()
        {
            List<string> variables = new List<string>();
            variables.AddRange(Left.GetAllVariables());
            variables.AddRange(Right.GetAllVariables());

            List<string> uniqueVariables = new List<string>();
            foreach (string variable in variables)
            {
                if (!uniqueVariables.Contains(variable))
                    uniqueVariables.Add(variable);
            }

            return uniqueVariables;
        }

        /// <summary>
        /// Проверяет, можно ли вычислить всю операцию
        /// </summary>
        /// <param name="variableValues">Словарь со значениями переменных</param>
        /// <returns>
        /// True если:
        /// - обе части можно вычислить
        /// - нет деления на ноль
        /// </returns>
        public override bool CanCalculate(Dictionary<string, double> variableValues)
        {
            bool leftOk = Left.CanCalculate(variableValues);
            bool rightOk = Right.CanCalculate(variableValues);

            if (Operator == "/" && Right is Constant constant && constant.Value == 0)
                return false;

            return leftOk && rightOk;
        }
    }

    /// <summary>
    /// Представляет операцию с одной частью
    /// </summary>
    /// <remarks>
    /// Это операции типа: отрицание, модуль
    /// Например: -x, abs(y), -(x + 3)
    /// </remarks>
    public class UnaryOperation : Expression
    {
        /// <summary>
        /// Операция: - (отрицание), abs (модуль) и т.д.
        /// </summary>
        public string Operator { get; }

        /// <summary>
        /// Выражение, к которому применяется операция
        /// </summary>
        public Expression Operand { get; }

        /// <summary>
        /// Создает новую операцию с одной частью
        /// </summary>
        /// <param name="operation">Операция: -, abs и т.д.</param>
        /// <param name="operand">Выражение для операции</param>
        /// <exception cref="ArgumentException">Выбрасывается если операция или выражение отсутствуют</exception>
        public UnaryOperation(string operation, Expression operand)
        {
            if (string.IsNullOrEmpty(operation))
                throw new ArgumentException("Операция не может быть пустой");
            if (operand == null)
                throw new ArgumentException("Выражение не может быть пустым");

            Operator = operation;
            Operand = operand;
        }

        /// <summary>
        /// Возвращает операцию в виде текста
        /// </summary>
        /// <returns>Операция как текст, например "-(x)" или "abs(y)"</returns>
        public override string ToString()
        {
            return $"{Operator}({Operand})";
        }

        public override Expression Simplify()
        {
            var op = Operand.Simplify();
            if (Operator == "-")
            {
                if (op is Constant c) return new Constant(-c.Value);
            }
            if (Operator == "abs")
            {
                if (op is Constant c2) return new Constant(Math.Abs(c2.Value));
            }
            return new UnaryOperation(Operator, op);
        }

        /// <summary>
        /// Заменяет переменную внутри выражения
        /// </summary>
        /// <param name="variableName">Имя переменной для замены</param>
        /// <param name="newValue">Выражение, которое подставится вместо переменной</param>
        /// <returns>Новая операция с замененной переменной</returns>
        public override Expression ReplaceVariable(string variableName, Expression newValue)
        {
            Expression newOperand = Operand.ReplaceVariable(variableName, newValue);
            return new UnaryOperation(Operator, newOperand);
        }

        /// <summary>
        /// Находит все переменные внутри выражения
        /// </summary>
        /// <returns>Список всех переменных из внутреннего выражения</returns>
        public override List<string> GetAllVariables()
        {
            return Operand.GetAllVariables();
        }

        /// <summary>
        /// Проверяет, можно ли вычислить внутреннее выражение
        /// </summary>
        /// <param name="variableValues">Словарь со значениями переменных</param>
        /// <returns>True если внутреннее выражение можно вычислить</returns>
        public override bool CanCalculate(Dictionary<string, double> variableValues)
        {
            return Operand.CanCalculate(variableValues);
        }
    }
}