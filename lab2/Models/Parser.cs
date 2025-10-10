using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace lab2.Models
{
    /// <summary>
    /// Парсер для преобразования текста в программу
    /// </summary>
    /// <remarks>
    /// Этот класс умеет читать текст и создавать из него структуру программы.
    /// Например, превращает текст "x := 5; y := x + 3" в объекты программы.
    /// </remarks>
    public class Parser
    {
        /// <summary>
        /// Разбирает текст и создает оператор программы
        /// </summary>
        /// <param name="input">Текст программы для разбора</param>
        /// <returns>Созданный оператор программы</returns>
        /// <exception cref="ArgumentException">Выбрасывается если текст пустой или неправильный</exception>
        /// <example>
        /// ParseStatement("x := 5") → оператор присваивания
        /// ParseStatement("if (x > 0) { y := 1 } else { y := 0 }") → условный оператор
        /// ParseStatement("x := 1; y := 2") → последовательность операторов
        /// </example>
        public static Statement ParseStatement(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                throw new ArgumentException("Пустой ввод");

            input = input.Trim();

            // Разбор последовательности операторов (разделенных точкой с запятой)
            if (input.Contains(";"))
            {
                var parts = SplitByTopLevelSemicolon(input);
                var statements = new List<Statement>();

                foreach (var part in parts)
                {
                    statements.Add(ParseStatement(part.Trim()));
                }

                return new Sequence(statements);
            }

            // Разбор условного оператора if-else
            if (input.StartsWith("if"))
            {
                return ParseConditional(input);
            }

            // Разбор присваивания
            if (input.Contains(":="))
            {
                return ParseAssignment(input);
            }

            throw new ArgumentException($"Неизвестный тип оператора: {input}");
        }

        /// <summary>
        /// Разбирает условие (логическое выражение)
        /// </summary>
        /// <param name="input">Текст условия</param>
        /// <returns>Созданное условие</returns>
        /// <exception cref="ArgumentException">Выбрасывается если текст пустой или неправильный</exception>
        /// <example>
        /// ParsePredicate("x > 5") → сравнение
        /// ParsePredicate("x > 0 && y < 10") → логическое "И"
        /// ParsePredicate("(x == 0) || (y != 1)") → логическое "ИЛИ"
        /// </example>
        public static Predicate ParsePredicate(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                throw new ArgumentException("Пустой предикат");

            input = input.Trim();

            // Разбор логических операций
            if (input.Contains("&&"))
            {
                var parts = SplitByTopLevelLogicalOperator(input, "&&");
                if (parts.Length == 2)
                {
                    var left = ParsePredicate(parts[0].Trim());
                    var right = ParsePredicate(parts[1].Trim());
                    return new LogicalPredicate(left, "∧", right);
                }
            }

            if (input.Contains("||"))
            {
                var parts = SplitByTopLevelLogicalOperator(input, "||");
                if (parts.Length == 2)
                {
                    var left = ParsePredicate(parts[0].Trim());
                    var right = ParsePredicate(parts[1].Trim());
                    return new LogicalPredicate(left, "∨", right);
                }
            }

            // Убираем внешние скобки если они есть
            if (input.StartsWith("(") && input.EndsWith(")") && IsBalanced(input))
            {
                return ParsePredicate(input.Substring(1, input.Length - 2));
            }

            // Разбор операций сравнения
            var comparisonOps = new[] { ">=", "<=", "==", "!=", ">", "<" };

            foreach (var op in comparisonOps)
            {
                var index = input.IndexOf(op);
                if (index > 0)
                {
                    var leftStr = input.Substring(0, index).Trim();
                    var rightStr = input.Substring(index + op.Length).Trim();

                    var left = ParseExpression(leftStr);
                    var right = ParseExpression(rightStr);

                    return new ComparisonPredicate(left, op, right);
                }
            }

            throw new ArgumentException($"Не удалось разобрать предикат: {input}");
        }

        /// <summary>
        /// Разбирает математическое выражение
        /// </summary>
        /// <param name="input">Текст выражения</param>
        /// <returns>Созданное выражение</returns>
        /// <exception cref="ArgumentException">Выбрасывается если текст пустой или неправильный</exception>
        /// <example>
        /// ParseExpression("5") → число 5
        /// ParseExpression("x") → переменная x
        /// ParseExpression("x + 3") → сложение
        /// ParseExpression("abs(-5)") → модуль числа
        /// ParseExpression("(x + y) * 2") → сложение в скобках и умножение
        /// </example>
        public static Expression ParseExpression(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                throw new ArgumentException("Пустое выражение");

            input = input.Trim();

            // Убираем внешние скобки если они есть
            if (input.StartsWith("(") && input.EndsWith(")") && IsBalanced(input))
            {
                return ParseExpression(input.Substring(1, input.Length - 2));
            }

            // Разбор бинарных операций (с учетом приоритета)
            var binaryOps = new[] { "+", "-", "*", "/" };

            // Обрабатываем операции с меньшим приоритетом первыми
            foreach (var op in new[] { "+", "-" })
            {
                var index = FindTopLevelOperator(input, op);
                if (index > 0)
                {
                    var leftStr = input.Substring(0, index).Trim();
                    var rightStr = input.Substring(index + op.Length).Trim();

                    var left = ParseExpression(leftStr);
                    var right = ParseExpression(rightStr);

                    return new BinaryOperation(left, op, right);
                }
            }

            foreach (var op in new[] { "*", "/" })
            {
                var index = FindTopLevelOperator(input, op);
                if (index > 0)
                {
                    var leftStr = input.Substring(0, index).Trim();
                    var rightStr = input.Substring(index + op.Length).Trim();

                    var left = ParseExpression(leftStr);
                    var right = ParseExpression(rightStr);

                    return new BinaryOperation(left, op, right);
                }
            }

            // Разбор унарных операций
            if (input.StartsWith("abs(") && input.EndsWith(")"))
            {
                var inner = input.Substring(4, input.Length - 5);
                return new UnaryOperation("abs", ParseExpression(inner));
            }

            if (input.StartsWith("-"))
            {
                return new UnaryOperation("-", ParseExpression(input.Substring(1)));
            }

            // Разбор чисел
            if (double.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
            {
                return new Constant(value);
            }

            // Переменная
            if (Regex.IsMatch(input, @"^[a-zA-Z_][a-zA-Z0-9_]*$"))
            {
                return new Variable(input);
            }

            throw new ArgumentException($"Не удалось разобрать выражение: {input}");
        }

        /// <summary>
        /// Разбирает оператор присваивания
        /// </summary>
        /// <param name="input">Текст присваивания в формате "переменная := выражение"</param>
        /// <returns>Оператор присваивания</returns>
        /// <exception cref="ArgumentException">Выбрасывается если формат неправильный</exception>
        /// <example>
        /// ParseAssignment("x := 5") → присваивание x = 5
        /// ParseAssignment("result := x + y * 2") → присваивание с вычислением
        /// </example>
        private static Statement ParseAssignment(string input)
        {
            var index = input.IndexOf(":=");
            if (index <= 0)
                throw new ArgumentException("Неверный формат присваивания");

            var variable = input.Substring(0, index).Trim();
            var expressionStr = input.Substring(index + 2).Trim();

            var expression = ParseExpression(expressionStr);
            return new Assignment(variable, expression);
        }

        /// <summary>
        /// Разбирает условный оператор if-else
        /// </summary>
        /// <param name="input">Текст условия в формате "if (условие) { тогда } else { иначе }"</param>
        /// <returns>Условный оператор</returns>
        /// <exception cref="ArgumentException">Выбрасывается если формат неправильный</exception>
        /// <example>
        /// ParseConditional("if (x > 0) { y := 1 } else { y := 0 }")
        /// </example>
        private static Statement ParseConditional(string input)
        {
            // Упрощенный парсер для if (condition) { statement1 } else { statement2 }
            var match = Regex.Match(input, @"if\s*\(([^)]+)\)\s*\{\s*([^}]+)\s*\}\s*else\s*\{\s*([^}]+)\s*\}");

            if (!match.Success)
                throw new ArgumentException("Неверный формат условного оператора");

            var conditionStr = match.Groups[1].Value.Trim();
            var thenStr = match.Groups[2].Value.Trim();
            var elseStr = match.Groups[3].Value.Trim();

            var condition = ParsePredicate(conditionStr);
            var thenBranch = ParseStatement(thenStr);
            var elseBranch = ParseStatement(elseStr);

            return new Conditional(condition, thenBranch, elseBranch);
        }

        /// <summary>
        /// Разделяет строку на части по точкам с запятой, но только на верхнем уровне
        /// </summary>
        /// <remarks>
        /// Игнорирует точки с запятой внутри скобок и фигурных скобок
        /// </remarks>
        /// <param name="input">Входная строка</param>
        /// <returns>Список частей без точек с запятой</returns>
        /// <example>
        /// "x := 1; y := 2; if (z) { a; b }" → ["x := 1", "y := 2", "if (z) { a; b }"]
        /// </example>
        private static List<string> SplitByTopLevelSemicolon(string input)
        {
            var parts = new List<string>();
            var current = "";
            var depth = 0;

            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];

                if (c == '(' || c == '{')
                    depth++;
                else if (c == ')' || c == '}')
                    depth--;
                else if (c == ';' && depth == 0)
                {
                    parts.Add(current);
                    current = "";
                    continue;
                }

                current += c;
            }

            if (current.Length > 0)
                parts.Add(current);

            return parts;
        }

        /// <summary>
        /// Разделяет логическое выражение на части по логическому оператору
        /// </summary>
        /// <remarks>
        /// Ищет оператор только на верхнем уровне (вне скобок)
        /// </remarks>
        /// <param name="input">Логическое выражение</param>
        /// <param name="op">Логический оператор ("&&" или "||")</param>
        /// <returns>Массив из двух частей или одна часть если оператор не найден</returns>
        /// <example>
        /// SplitByTopLevelLogicalOperator("x > 0 && y < 10", "&&") → ["x > 0", "y < 10"]
        /// </example>
        private static string[] SplitByTopLevelLogicalOperator(string input, string op)
        {
            var depth = 0;
            for (int i = 0; i < input.Length - op.Length + 1; i++)
            {
                if (input[i] == '(')
                    depth++;
                else if (input[i] == ')')
                    depth--;
                else if (depth == 0 && input.Substring(i, op.Length) == op)
                {
                    return new[] { input.Substring(0, i), input.Substring(i + op.Length) };
                }
            }
            return new[] { input };
        }

        /// <summary>
        /// Находит оператор на верхнем уровне в выражении
        /// </summary>
        /// <remarks>
        /// Ищет справа налево для правильной ассоциативности операций
        /// Игнорирует операторы внутри скобок
        /// </remarks>
        /// <param name="input">Математическое выражение</param>
        /// <param name="op">Оператор (+, -, *, /)</param>
        /// <returns>Позиция оператора или -1 если не найден</returns>
        /// <example>
        /// FindTopLevelOperator("x + y * z", "+") → позиция символа '+'
        /// </example>
        private static int FindTopLevelOperator(string input, string op)
        {
            var depth = 0;
            for (int i = input.Length - 1; i >= 0; i--) // Ищем справа для правильной ассоциативности
            {
                if (input[i] == ')')
                    depth++;
                else if (input[i] == '(')
                    depth--;
                else if (depth == 0 && i >= op.Length - 1)
                {
                    if (input.Substring(i - op.Length + 1, op.Length) == op)
                        return i - op.Length + 1;
                }
            }
            return -1;
        }

        /// <summary>
        /// Проверяет правильно ли расставлены скобки в выражении
        /// </summary>
        /// <param name="input">Выражение для проверки</param>
        /// <returns>True если скобки сбалансированы, иначе False</returns>
        /// <example>
        /// IsBalanced("(a + (b * c))") → true
        /// IsBalanced("(a + b))") → false
        /// </example>
        private static bool IsBalanced(string input)
        {
            var depth = 0;
            foreach (char c in input)
            {
                if (c == '(') depth++;
                else if (c == ')') depth--;
                if (depth < 0) return false;
            }
            return depth == 0;
        }
    }
}