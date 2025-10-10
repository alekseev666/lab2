using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lab2.Models
{
    public abstract class Statement
    {
        public abstract override string ToString();
        public abstract Predicate WeakestPrecondition(Predicate postcondition);
        public abstract string ToHumanReadable();
    }

    public class Assignment : Statement
    {
        public string Variable { get; }
        public Expression Expression { get; }

        public Assignment(string variable, Expression expression)
        {
            Variable = variable ?? throw new ArgumentNullException(nameof(variable));
            Expression = expression ?? throw new ArgumentNullException(nameof(expression));
        }

        public override string ToString()
        {
            return $"{Variable} := {Expression}";
        }

        public override string ToHumanReadable()
        {
            return $"присвоить переменной {Variable} значение {Expression}";
        }

        public override Predicate WeakestPrecondition(Predicate postcondition)
        {
            // wp(x := e, R) = R[x/e] ∧ "e определено"
            var substituted = postcondition.ReplaceVariable(Variable, Expression);

            // Добавляем требования определенности выражения
            var definednessConditions = GetDefinednessConditions(Expression);

            if (definednessConditions == null)
                return substituted;

            return new LogicalPredicate(definednessConditions, "∧", substituted);
        }

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

        private static Predicate CombineConditions(Predicate left, Predicate right)
        {
            if (left == null) return right;
            if (right == null) return left;
            return new LogicalPredicate(left, "∧", right);
        }
    }

    public class Conditional : Statement
    {
        public Predicate Condition { get; }
        public Statement ThenBranch { get; }
        public Statement ElseBranch { get; }

        public Conditional(Predicate condition, Statement thenBranch, Statement elseBranch)
        {
            Condition = condition ?? throw new ArgumentNullException(nameof(condition));
            ThenBranch = thenBranch ?? throw new ArgumentNullException(nameof(thenBranch));
            ElseBranch = elseBranch ?? throw new ArgumentNullException(nameof(elseBranch));
        }

        public override string ToString()
        {
            return $"if ({Condition}) {{ {ThenBranch} }} else {{ {ElseBranch} }}";
        }

        public override string ToHumanReadable()
        {
            return $"если {Condition.ToHumanReadable()}, то {ThenBranch.ToHumanReadable()}, иначе {ElseBranch.ToHumanReadable()}";
        }

        public override Predicate WeakestPrecondition(Predicate postcondition)
        {
            // wp(if B then S1 else S2, R) = (B ∧ wp(S1,R)) ∨ (¬B ∧ wp(S2,R))
            var thenWp = ThenBranch.WeakestPrecondition(postcondition);
            var elseWp = ElseBranch.WeakestPrecondition(postcondition);

            var thenCondition = new LogicalPredicate(Condition, "∧", thenWp);
            var elseCondition = new LogicalPredicate(new NotPredicate(Condition), "∧", elseWp);

            return new LogicalPredicate(thenCondition, "∨", elseCondition);
        }
    }

    public class Sequence : Statement
    {
        public List<Statement> Statements { get; }

        public Sequence(List<Statement> statements)
        {
            Statements = statements ?? throw new ArgumentNullException(nameof(statements));
            if (statements.Count == 0)
                throw new ArgumentException("Sequence cannot be empty", nameof(statements));
        }

        public override string ToString()
        {
            return string.Join("; ", Statements);
        }

        public override string ToHumanReadable()
        {
            return string.Join(", затем ", Statements.ConvertAll(s => s.ToHumanReadable()));
        }

        public override Predicate WeakestPrecondition(Predicate postcondition)
        {
            // wp(S1; S2; ...; Sn, R) = wp(S1, wp(S2, wp(..., wp(Sn, R))))
            // Обрабатываем с конца
            var currentPredicate = postcondition;

            for (int i = Statements.Count - 1; i >= 0; i--)
            {
                currentPredicate = Statements[i].WeakestPrecondition(currentPredicate);
            }

            return currentPredicate;
        }
    }
}
