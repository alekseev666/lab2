using lab2.Models;

namespace TestProject1
{
    /// <summary>
    /// Тесты для основной функциональности - вычисления слабейших предусловий (WP)
    /// </summary>
    [TestClass]
    public class WeakestPreconditionTests
    {
        /// <summary>
        /// Тест 1: Простое присваивание
        /// Пример: x := x + 10, постусловие: x > 15
        /// Ожидаемый результат WP: (x + 10) > 15, то есть x > 5
        /// </summary>
        [TestMethod]
        public void Assignment_ShouldCalculateWeakestPreconditionCorrectly()
        {

            // Создаем присваивание: x := x + 10
            var x = new Variable("x");
            var ten = new Constant(10);
            var expression = new BinaryOperation(x, "+", ten); // x + 10
            var assignment = new Assignment("x", expression);
            
            // Создаем постусловие: x > 15
            var fifteen = new Constant(15);
            var postcondition = new ComparisonPredicate(x, ">", fifteen);

  
            var result = assignment.WeakestPrecondition(postcondition);
            Assert.IsInstanceOfType(result, typeof(ComparisonPredicate), "Результат должен быть условием сравнения");
            
            var comparisonResult = (ComparisonPredicate)result;
            Assert.AreEqual(">", comparisonResult.Operator, "Оператор сравнения должен остаться >");
            
            // Левая часть должна быть (x + 10)
            Assert.IsInstanceOfType(comparisonResult.Left, typeof(BinaryOperation), "Левая часть должна быть выражением x + 10");
            var leftExpression = (BinaryOperation)comparisonResult.Left;
            Assert.AreEqual("+", leftExpression.Operator, "Оператор в левой части должен быть +");
            
            // Правая часть должна остаться 15
            Assert.IsInstanceOfType(comparisonResult.Right, typeof(Constant), "Правая часть должна остаться константой");
            var rightConstant = (Constant)comparisonResult.Right;
            Assert.AreEqual(15, rightConstant.Value, "Правая часть должна быть равна 15");
            
            // Проверяем строковое представление
            string expectedString = "(x + 10) > 15";
            Assert.AreEqual(expectedString, result.ToString(), "WP должно быть: (x + 10) > 15");
        }

        /// <summary>
        /// Тест 2: Условный оператор (if-else)
        /// Пример: if (x >= 0) { y := x } else { y := -x }, постусловие: y > 10
        /// Ожидаемый результат WP: (x >= 0 ∧ x > 10) ∨ (¬(x >= 0) ∧ -x > 10)
        /// Упрощенно: x > 10 ∨ x < -10
        /// </summary>
        [TestMethod]
        public void Conditional_ShouldCalculateWeakestPreconditionCorrectly()
        {
            var x = new Variable("x");
            var y = new Variable("y");
            var zero = new Constant(0);
            var ten = new Constant(10);
            
            // Создаем условие: x >= 0
            var condition = new ComparisonPredicate(x, ">=", zero);
            
            // Создаем ветку THEN: y := x
            var thenAssignment = new Assignment("y", x);
            
            // Создаем ветку ELSE: y := -x
            var negativeX = new UnaryOperation("-", x);
            var elseAssignment = new Assignment("y", negativeX);
            
            // Создаем условный оператор
            var conditional = new Conditional(condition, thenAssignment, elseAssignment);
            
            // Создаем постусловие: y > 10
            var postcondition = new ComparisonPredicate(y, ">", ten);

 
            var result = conditional.WeakestPrecondition(postcondition);

 
            Assert.IsInstanceOfType(result, typeof(LogicalPredicate), "Результат должен быть логическим предикатом");
            
            var logicalResult = (LogicalPredicate)result;
            Assert.AreEqual("∨", logicalResult.Operator, "Основная операция должна быть ∨ (ИЛИ)");
            
            // Левая часть должна быть (x >= 0 ∧ x > 10)
            Assert.IsInstanceOfType(logicalResult.Left, typeof(LogicalPredicate), "Левая часть должна быть логическим предикатом");
            var leftLogical = (LogicalPredicate)logicalResult.Left;
            Assert.AreEqual("∧", leftLogical.Operator, "В левой части должна быть операция ∧ (И)");
            
            // Правая часть должна быть (¬(x >= 0) ∧ -(x) > 10)
            Assert.IsInstanceOfType(logicalResult.Right, typeof(LogicalPredicate), "Правая часть должна быть логическим предикатом");
            var rightLogical = (LogicalPredicate)logicalResult.Right;
            Assert.AreEqual("∧", rightLogical.Operator, "В правой части должна быть операция ∧ (И)");
            
            // Проверяем, что результат содержит правильные переменные
            var variables = result.GetAllVariables();
            Assert.IsTrue(variables.Contains("x"), "Результат должен содержать переменную x");
            Assert.IsFalse(variables.Contains("y"), "Переменная y должна быть заменена");
            
            // Проверяем общую структуру (содержит основные элементы)
            string resultString = result.ToString();
            Assert.IsTrue(resultString.Contains("x >= 0"), "Результат должен содержать исходное условие x >= 0");
            Assert.IsTrue(resultString.Contains("∨"), "Результат должен содержать операцию ИЛИ");
            Assert.IsTrue(resultString.Contains("∧"), "Результат должен содержать операции И");
            Assert.IsTrue(resultString.Contains("¬"), "Результат должен содержать отрицание");
        }

        /// <summary>
        /// Тест 3: Последовательность операторов
        /// Пример: x := x + 1; y := x * 2, постусловие: y > 20  
        /// Ожидаемый результат WP: x + 1 заменить в (x * 2 > 20), получить (x + 1) * 2 > 20
        /// Упрощенно: (x + 1) * 2 > 20
        /// </summary>
        [TestMethod]
        public void Sequence_ShouldCalculateWeakestPreconditionCorrectly()
        {
  
            var x = new Variable("x");
            var y = new Variable("y");
            var one = new Constant(1);
            var two = new Constant(2);
            var twenty = new Constant(20);
            
            // Создаем первое присваивание: x := x + 1
            var xPlusOne = new BinaryOperation(x, "+", one);
            var firstAssignment = new Assignment("x", xPlusOne);
            
            // Создаем второе присваивание: y := x * 2
            var xTimesTwo = new BinaryOperation(x, "*", two);
            var secondAssignment = new Assignment("y", xTimesTwo);
            
            // Создаем последовательность
            var statements = new List<Statement> { firstAssignment, secondAssignment };
            var sequence = new Sequence(statements);
            
            // Создаем постусловие: y > 20
            var postcondition = new ComparisonPredicate(y, ">", twenty);

   
            var result = sequence.WeakestPrecondition(postcondition);

    
            Assert.IsInstanceOfType(result, typeof(ComparisonPredicate), "Результат должен быть условием сравнения");
            
            var comparisonResult = (ComparisonPredicate)result;
            Assert.AreEqual(">", comparisonResult.Operator, "Оператор сравнения должен остаться >");
            
            // Левая часть должна быть ((x + 1) * 2)
            Assert.IsInstanceOfType(comparisonResult.Left, typeof(BinaryOperation), "Левая часть должна быть бинарной операцией");
            var leftOperation = (BinaryOperation)comparisonResult.Left;
            Assert.AreEqual("*", leftOperation.Operator, "Внешняя операция должна быть умножение");
            
            // Внутри умножения должно быть (x + 1)
            Assert.IsInstanceOfType(leftOperation.Left, typeof(BinaryOperation), "Внутри умножения должно быть сложение");
            var innerOperation = (BinaryOperation)leftOperation.Left;
            Assert.AreEqual("+", innerOperation.Operator, "Внутренняя операция должна быть сложение");
            
            // Правая часть должна остаться 20
            Assert.IsInstanceOfType(comparisonResult.Right, typeof(Constant), "Правая часть должна остаться константой");
            var rightConstant = (Constant)comparisonResult.Right;
            Assert.AreEqual(20, rightConstant.Value, "Правая часть должна быть равна 20");
            
            // Проверяем переменные - должна остаться только x
            var variables = result.GetAllVariables();
            Assert.AreEqual(1, variables.Count, "Должна остаться только одна переменная");
            Assert.IsTrue(variables.Contains("x"), "Должна содержаться переменная x");
            Assert.IsFalse(variables.Contains("y"), "Переменная y должна быть заменена");
            
            // Проверяем строковое представление
            string expectedString = "((x + 1) * 2) > 20";
            Assert.AreEqual(expectedString, result.ToString(), "WP должно быть: ((x + 1) * 2) > 20");
        }

        /// <summary>
        /// Тест 4: Проверяем парсер - разбор простого присваивания
        /// Пример: "x := 5" должно правильно разбираться в объект Assignment
        /// </summary>
        [TestMethod]
        public void Parser_ShouldParseSimpleAssignmentCorrectly()
        {
    
            string code = "x := 5";

    
            var result = Parser.ParseStatement(code);

            Assert.IsInstanceOfType(result, typeof(Assignment), "Результат должен быть присваиванием");
            
            var assignment = (Assignment)result;
            Assert.AreEqual("x", assignment.Variable, "Переменная должна быть x");
            Assert.IsInstanceOfType(assignment.Expression, typeof(Constant), "Выражение должно быть константой");
            
            var constant = (Constant)assignment.Expression;
            Assert.AreEqual(5, constant.Value, "Значение константы должно быть 5");
            
            // Проверяем строковое представление
            Assert.AreEqual("x := 5", result.ToString(), "Строковое представление должно совпадать с исходным");
        }

        /// <summary>
        /// Тест 5: Проверяем парсер - разбор простого условия сравнения
        /// Пример: "x > 10" должно правильно разбираться в объект ComparisonPredicate
        /// </summary>
        [TestMethod]
        public void Parser_ShouldParseSimpleComparisonCorrectly()
        {
          
            string predicate = "x > 10";

    
            var result = Parser.ParsePredicate(predicate);

            Assert.IsInstanceOfType(result, typeof(ComparisonPredicate), "Результат должен быть условием сравнения");
            
            var comparison = (ComparisonPredicate)result;
            Assert.AreEqual(">", comparison.Operator, "Оператор должен быть >");
            
            // Левая часть должна быть переменной x
            Assert.IsInstanceOfType(comparison.Left, typeof(Variable), "Левая часть должна быть переменной");
            var variable = (Variable)comparison.Left;
            Assert.AreEqual("x", variable.Name, "Переменная должна быть x");
            
            // Правая часть должна быть константой 10
            Assert.IsInstanceOfType(comparison.Right, typeof(Constant), "Правая часть должна быть константой");
            var constant = (Constant)comparison.Right;
            Assert.AreEqual(10, constant.Value, "Значение константы должно быть 10");
            
            // Проверяем строковое представление
            Assert.AreEqual("x > 10", result.ToString(), "Строковое представление должно совпадать с исходным");
            Assert.AreEqual("x больше 10", result.ToHumanReadable(), "Человеко-читаемое представление должно быть на русском");
        }
    }
}