using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lab2.Models
{
    /// <summary>
    /// Базовый класс для всех операторов программы
    /// </summary>
    /// <remarks>
    /// Это как шаблон для любых команд в программе: присваивание, условия, последовательности.
    /// Каждый оператор знает, как вычислить для себя weakest precondition (наименьшую предусловие).
    /// </remarks>
    public abstract class Statement
    {
        /// <summary>
        /// Превращает оператор в текст программы
        /// </summary>
        /// <returns>Оператор в виде текста</returns>
        public abstract override string ToString();

        /// <summary>
        /// Вычисляет weakest precondition (наименьшую предусловие) для этого оператора
        /// </summary>
        /// <param name="postcondition">Постусловие, которое должно выполняться после оператора</param>
        /// <returns>Предусловие, которое должно выполняться до оператора</returns>
        /// <remarks>
        /// Weakest precondition - это самое слабое условие, которое должно быть истинным ДО выполнения оператора,
        /// чтобы гарантировать, что ПОСЛЕ оператора будет истинно заданное постусловие.
        /// </remarks>
        public abstract Predicate WeakestPrecondition(Predicate postcondition);

        /// <summary>
        /// Превращает оператор в понятный человеческий язык
        /// </summary>
        /// <returns>Оператор простыми словами</returns>
        public abstract string ToHumanReadable();
    }

    /// <summary>
    /// Оператор присваивания значения переменной
    /// </summary>
    /// <remarks>
    /// Это команда "положить значение в переменную".
    /// Например: x := 5, result := x + y * 2
    /// </remarks>
    public class Assignment : Statement
    {
        /// <summary>
        /// Имя переменной, которой присваивается значение
        /// </summary>
        /// <example>"x", "result", "counter"</example>
        public string Variable { get; }

        /// <summary>
        /// Выражение, значение которого присваивается переменной
        /// </summary>
        /// <example>5, x + 3, a * b</example>
        public Expression Expression { get; }

        /// <summary>
        /// Создает новый оператор присваивания
        /// </summary>
        /// <param name="variable">Имя переменной</param>
        /// <param name="expression">Выражение для присваивания</param>
        /// <exception cref="ArgumentNullException">Выбрасывается если переменная или выражение пустые</exception>
        public Assignment(string variable, Expression expression)
        {
            Variable = variable ?? throw new ArgumentNullException(nameof(variable));
            Expression = expression ?? throw new ArgumentNullException(nameof(expression));
        }

        /// <summary>
        /// Возвращает присваивание в виде текста программы
        /// </summary>
        /// <returns>Присваивание как текст, например "x := 5"</returns>
        public override string ToString()
        {
            return $"{Variable} := {Expression}";
        }

        /// <summary>
        /// Превращает присваивание в понятный человеческий текст
        /// </summary>
        /// <returns>Присваивание простыми словами</returns>
        /// <example>"присвоить переменной x значение 5"</example>
        public override string ToHumanReadable()
        {
            return $"присвоить переменной {Variable} значение {Expression}";
        }

        /// <summary>
        /// Вычисляет weakest precondition для присваивания
        /// </summary>
        /// <param name="postcondition">Постусловие, которое должно быть истинно после присваивания</param>
        /// <returns>Предусловие, которое должно быть истинно до присваивания</returns>
        /// <remarks>
        /// Формула: wp(x := e, R) = R[x/e] ∧ "e определено"
        /// Это означает: заменить в постусловии R все вхождения x на e, и добавить проверку что e определено
        /// </remarks>
        /// <example>
        /// Постусловие: x > 0
        /// Присваивание: x := y + 1
        /// Weakest precondition: (y + 1) > 0 ∧ (y + 1 определено)
        /// </example>
        public override Predicate WeakestPrecondition(Predicate postcondition)
        {
            var substituted = postcondition.ReplaceVariable(Variable, Expression).Simplify();
            var definednessConditions = GetDefinednessConditions(Expression);
            var result = definednessConditions == null
                ? substituted
                : new LogicalPredicate(definednessConditions, "∧", substituted);
            return result.Simplify();
        }

        /// <summary>
        /// Проверяет, что выражение может быть вычислено без ошибок
        /// </summary>
        /// <param name="expr">Выражение для проверки</param>
        /// <returns>
        /// Условие гарантирующее корректность вычисления, или null если выражение всегда определено
        /// </returns>
        /// <remarks>
        /// Например, для деления проверяет что знаменатель не ноль
        /// </remarks>
        private static Predicate GetDefinednessConditions(Expression expr)
        {
            switch (expr)
            {
                case BinaryOperation binOp when binOp.Operator == "/":
                    // Для деления добавляем условие, что знаменатель != 0
                    var notZeroCondition = new ComparisonPredicate(
                        binOp.Right,
                        "!=",
                        new Constant(0)
                    );

                    var leftCond = GetDefinednessConditions(binOp.Left);
                    var rightCond = GetDefinednessConditions(binOp.Right);

                    return CombineConditions(CombineConditions(leftCond, rightCond), notZeroCondition);

                case BinaryOperation binOp:
                    var leftCondition = GetDefinednessConditions(binOp.Left);
                    var rightCondition = GetDefinednessConditions(binOp.Right);
                    return CombineConditions(leftCondition, rightCondition);

                case UnaryOperation unOp:
                    return GetDefinednessConditions(unOp.Operand);

                default:
                    return null; // Переменные и константы всегда определены
            }
        }

        /// <summary>
        /// Объединяет два условия логическим И
        /// </summary>
        /// <param name="left">Первое условие</param>
        /// <param name="right">Второе условие</param>
        /// <returns>Объединенное условие или одно из условий если другое null</returns>
        private static Predicate CombineConditions(Predicate left, Predicate right)
        {
            if (left == null) return right;
            if (right == null) return left;
            return new LogicalPredicate(left, "∧", right);
        }
    }

    /// <summary>
    /// Условный оператор (if-else)
    /// </summary>
    /// <remarks>
    /// Это команда "если условие верно, сделай одно, иначе сделай другое".
    /// Например: if (x > 0) { y := 1 } else { y := 0 }
    /// </remarks>
    public class Conditional : Statement
    {
        /// <summary>
        /// Условие для проверки
        /// </summary>
        /// <example>x > 0, a == b, flag && ready</example>
        public Predicate Condition { get; }

        /// <summary>
        /// Оператор, который выполняется если условие истинно
        /// </summary>
        public Statement ThenBranch { get; }

        /// <summary>
        /// Оператор, который выполняется если условие ложно
        /// </summary>
        public Statement ElseBranch { get; }

        /// <summary>
        /// Создает новый условный оператор
        /// </summary>
        /// <param name="condition">Условие для проверки</param>
        /// <param name="thenBranch">Оператор для случая "then"</param>
        /// <param name="elseBranch">Оператор для случая "else"</param>
        /// <exception cref="ArgumentNullException">Выбрасывается если какая-то часть отсутствует</exception>
        public Conditional(Predicate condition, Statement thenBranch, Statement elseBranch)
        {
            Condition = condition ?? throw new ArgumentNullException(nameof(condition));
            ThenBranch = thenBranch ?? throw new ArgumentNullException(nameof(thenBranch));
            ElseBranch = elseBranch ?? throw new ArgumentNullException(nameof(elseBranch));
        }

        /// <summary>
        /// Возвращает условный оператор в виде текста программы
        /// </summary>
        /// <returns>Условный оператор как текст</returns>
        public override string ToString()
        {
            return $"if ({Condition}) {{ {ThenBranch} }} else {{ {ElseBranch} }}";
        }

        /// <summary>
        /// Превращает условный оператор в понятный человеческий текст
        /// </summary>
        /// <returns>Условный оператор простыми словами</returns>
        public override string ToHumanReadable()
        {
            return $"если {Condition.ToHumanReadable()}, то {ThenBranch.ToHumanReadable()}, иначе {ElseBranch.ToHumanReadable()}";
        }

        /// <summary>
        /// Вычисляет weakest precondition для условного оператора
        /// </summary>
        /// <param name="postcondition">Постусловие, которое должно быть истинно после условного оператора</param>
        /// <returns>Предусловие, которое должно быть истинно до условного оператора</returns>
        /// <remarks>
        /// Формула: wp(if B then S1 else S2, R) = (B ∧ wp(S1,R)) ∨ (¬B ∧ wp(S2,R))
        /// Это означает: либо условие B истинно и weakest precondition для S1, 
        /// либо условие B ложно и weakest precondition для S2
        /// </remarks>
        /// <example>
        /// Постусловие: y > 0
        /// Условный оператор: if (x > 0) { y := 1 } else { y := -1 }
        /// Weakest precondition: (x > 0 ∧ 1 > 0) ∨ (x ≤ 0 ∧ -1 > 0)
        /// Упрощается до: (x > 0) ∨ false = x > 0
        /// </example>
        public override Predicate WeakestPrecondition(Predicate postcondition)
        {
            var thenWp = ThenBranch.WeakestPrecondition(postcondition).Simplify();
            var elseWp = ElseBranch.WeakestPrecondition(postcondition).Simplify();
            var thenCondition = new LogicalPredicate(Condition, "∧", thenWp);
            var elseCondition = new LogicalPredicate(new NotPredicate(Condition), "∧", elseWp);
            return new LogicalPredicate(thenCondition, "∨", elseCondition).Simplify();
        }
    }

    /// <summary>
    /// Последовательность операторов
    /// </summary>
    /// <remarks>
    /// Это несколько операторов, выполняющихся один за другим.
    /// Например: x := 1; y := 2; z := x + y
    /// </remarks>
    public class Sequence : Statement
    {
        /// <summary>
        /// Список операторов в последовательности
        /// </summary>
        public List<Statement> Statements { get; }

        /// <summary>
        /// Создает новую последовательность операторов
        /// </summary>
        /// <param name="statements">Список операторов</param>
        /// <exception cref="ArgumentNullException">Выбрасывается если список пустой или null</exception>
        /// <exception cref="ArgumentException">Выбрасывается если список пустой</exception>
        public Sequence(List<Statement> statements)
        {
            Statements = statements ?? throw new ArgumentNullException(nameof(statements));
            if (statements.Count == 0)
                throw new ArgumentException("Sequence cannot be empty", nameof(statements));
        }

        /// <summary>
        /// Возвращает последовательность в виде текста программы
        /// </summary>
        /// <returns>Последовательность как текст, например "x := 1; y := 2"</returns>
        public override string ToString()
        {
            return string.Join("; ", Statements);
        }

        /// <summary>
        /// Превращает последовательность в понятный человеческий текст
        /// </summary>
        /// <returns>Последовательность простыми словами</returns>
        public override string ToHumanReadable()
        {
            return string.Join(", затем ", Statements.ConvertAll(s => s.ToHumanReadable()));
        }

        /// <summary>
        /// Вычисляет weakest precondition для последовательности
        /// </summary>
        /// <param name="postcondition">Постусловие, которое должно быть истинно после всей последовательности</param>
        /// <returns>Предусловие, которое должно быть истинно до последовательности</returns>
        /// <remarks>
        /// Формула: wp(S1; S2; ...; Sn, R) = wp(S1, wp(S2, wp(..., wp(Sn, R))))
        /// Это означает: вычисляем weakest precondition с конца последовательности к началу
        /// </remarks>
        /// <example>
        /// Постусловие: z == 3
        /// Последовательность: x := 1; y := 2; z := x + y
        /// Вычисление:
        /// wp(z := x + y, z == 3) = (x + y == 3)
        /// wp(y := 2, x + y == 3) = (x + 2 == 3)
        /// wp(x := 1, x + 2 == 3) = (1 + 2 == 3) = true
        /// </example>
        public override Predicate WeakestPrecondition(Predicate postcondition)
        {
            var currentPredicate = postcondition.Simplify();
            for (int i = Statements.Count - 1; i >= 0; i--)
            {
                currentPredicate = Statements[i].WeakestPrecondition(currentPredicate).Simplify();
            }
            return currentPredicate.Simplify();
        }
    }
}