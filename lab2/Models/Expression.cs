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
        /// <summary>
        /// Превращает выражение в текст, чтобы можно было его прочитать
        /// </summary>
        /// <returns>Выражение в виде текста, например "x + 5"</returns>
        public abstract override string ToString();

        /// <summary>
        /// Заменяет переменную на другое выражение
        /// </summary>
        /// <param name="variableName">Имя переменной, которую нужно заменить</param>
        /// <param name="newValue">Новое выражение, которое подставится вместо переменной</param>
        /// <returns>Новое выражение с замененной переменной</returns>
        /// <example>
        /// Было: x + 3, заменили x на (y * 2)
        /// Стало: (y * 2) + 3
        /// </example>
        public abstract Expression ReplaceVariable(string variableName, Expression newValue);

        /// <summary>
        /// Находит все переменные, которые есть в этом выражении
        /// </summary>
        /// <returns>Список имен всех переменных в выражении</returns>
        /// <example>
        /// Для выражения "x + y * z" вернет список: ["x", "y", "z"]
        /// </example>
        public abstract List<string> GetAllVariables();

        /// <summary>
        /// Проверяет, можно ли вычислить это выражение
        /// </summary>
        /// <param name="variableValues">Словарь со значениями переменных: имя переменной → её значение</param>
        /// <returns>True если выражение можно вычислить, False если нельзя</returns>
        /// <remarks>
        /// Проверяет, что все переменные есть в словаре и нет деления на ноль
        /// </remarks>
        public abstract bool CanCalculate(Dictionary<string, double> variableValues);
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