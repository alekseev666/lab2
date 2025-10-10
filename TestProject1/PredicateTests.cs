using lab2.Models;

namespace TestProject1
{
    /// <summary>
    /// Тесты для классов логических условий (предикатов)
    /// </summary>
    [TestClass]
    public class PredicateTests
    {
        /// <summary>
        /// Тест 1: Проверяем простое условие сравнения
        /// Пример: x > 5 должно правильно создаваться и отображаться
        /// </summary>
        [TestMethod]
        public void ComparisonPredicate_ShouldWorkCorrectly()
        {
            // Arrange (Подготовка)
            var x = new Variable("x");
            var five = new Constant(5);
            
            // Act (Выполнение)
            var predicate = new ComparisonPredicate(x, ">", five);

            // Assert (Проверка)
            Assert.AreEqual("x > 5", predicate.ToString(), "Строковое представление должно быть корректным");
            Assert.AreEqual("x больше 5", predicate.ToHumanReadable(), "Человеко-читаемое представление должно быть на русском");
            
            // Проверяем, что предикат содержит правильную переменную
            var variables = predicate.GetAllVariables();
            Assert.AreEqual(1, variables.Count, "Должна быть одна переменная");
            Assert.IsTrue(variables.Contains("x"), "Должна содержаться переменная x");
        }

        /// <summary>
        /// Тест 2: Проверяем замену переменных в условии сравнения  
        /// Пример: x > 5, заменить x на (y + 2), получить: (y + 2) > 5
        /// </summary>
        [TestMethod]
        public void ComparisonPredicate_ShouldReplaceVariableCorrectly()
        {
            // Arrange (Подготовка)
            var x = new Variable("x");
            var five = new Constant(5);
            var originalPredicate = new ComparisonPredicate(x, ">", five);
            
            // Создаем выражение для замены: y + 2
            var y = new Variable("y");
            var two = new Constant(2);
            var replacement = new BinaryOperation(y, "+", two);

            // Act (Выполнение)
            var result = originalPredicate.ReplaceVariable("x", replacement);

            // Assert (Проверка)
            Assert.IsInstanceOfType(result, typeof(ComparisonPredicate), "Результат должен быть условием сравнения");
            
            var comparisonResult = (ComparisonPredicate)result;
            Assert.AreEqual(">", comparisonResult.Operator, "Оператор сравнения должен остаться тем же");
            
            // Левая часть должна быть заменена на (y + 2)
            Assert.IsInstanceOfType(comparisonResult.Left, typeof(BinaryOperation), "Левая часть должна быть бинарной операцией");
            
            // Правая часть должна остаться 5
            Assert.IsInstanceOfType(comparisonResult.Right, typeof(Constant), "Правая часть должна остаться константой");
            
            // Проверяем строковое представление
            Assert.AreEqual("(y + 2) > 5", result.ToString(), "Строковое представление должно отражать замену");
            
            // Проверяем переменные - теперь должна быть y вместо x
            var variables = result.GetAllVariables();
            Assert.AreEqual(1, variables.Count, "Должна быть одна переменная");
            Assert.IsTrue(variables.Contains("y"), "Должна содержаться переменная y");
            Assert.IsFalse(variables.Contains("x"), "Переменная x должна быть заменена");
        }

        /// <summary>
        /// Тест 3: Проверяем сложное логическое условие
        /// Пример: (x > 0) И (y < 10) - проверяем корректность создания и работы
        /// </summary>
        [TestMethod]
        public void LogicalPredicate_ShouldCombineConditionsCorrectly()
        {
            // Arrange (Подготовка)
            var x = new Variable("x");
            var y = new Variable("y");
            var zero = new Constant(0);
            var ten = new Constant(10);
            
            // Создаем два простых условия
            var leftCondition = new ComparisonPredicate(x, ">", zero);   // x > 0
            var rightCondition = new ComparisonPredicate(y, "<", ten);   // y < 10

            // Act (Выполнение)
            // Объединяем условия через И (∧)
            var logicalPredicate = new LogicalPredicate(leftCondition, "∧", rightCondition);

            // Assert (Проверка)
            string expectedString = "(x > 0 ∧ y < 10)";
            Assert.AreEqual(expectedString, logicalPredicate.ToString(), "Строковое представление должно быть корректным");
            
            string expectedHuman = "(x больше 0) И (y меньше 10)";
            Assert.AreEqual(expectedHuman, logicalPredicate.ToHumanReadable(), "Человеко-читаемое представление должно быть на русском");
            
            // Проверяем переменные - должны быть обе: x и y
            var variables = logicalPredicate.GetAllVariables();
            Assert.AreEqual(2, variables.Count, "Должно быть две переменные");
            Assert.IsTrue(variables.Contains("x"), "Должна содержаться переменная x");
            Assert.IsTrue(variables.Contains("y"), "Должна содержаться переменная y");
        }

        /// <summary>
        /// Тест 4: Проверяем отрицание условия
        /// Пример: НЕ(x > 5) должно правильно создаваться и работать
        /// </summary>
        [TestMethod]
        public void NotPredicate_ShouldNegateCorrectly()
        {
            // Arrange (Подготовка)
            var x = new Variable("x");
            var five = new Constant(5);
            var originalCondition = new ComparisonPredicate(x, ">", five); // x > 5

            // Act (Выполнение)
            var notPredicate = new NotPredicate(originalCondition);

            // Assert (Проверка)
            string expectedString = "¬(x > 5)";
            Assert.AreEqual(expectedString, notPredicate.ToString(), "Строковое представление отрицания должно быть корректным");
            
            string expectedHuman = "НЕ (x больше 5)";
            Assert.AreEqual(expectedHuman, notPredicate.ToHumanReadable(), "Человеко-читаемое представление отрицания должно быть на русском");
            
            // Переменные должны остаться теми же
            var variables = notPredicate.GetAllVariables();
            Assert.AreEqual(1, variables.Count, "Должна быть одна переменная");
            Assert.IsTrue(variables.Contains("x"), "Должна содержаться переменная x");
        }

        /// <summary>
        /// Тест 5: Проверяем специальные предикаты True и False
        /// Они должны работать как синглтоны и не содержать переменных
        /// </summary>
        [TestMethod]
        public void TrueAndFalsePredicates_ShouldBehaveCorrectly()
        {
            // Act (Выполнение)
            var truePredicate = TruePredicate.Instance;
            var falsePredicate = FalsePredicate.Instance;

            // Assert (Проверка)
            // Проверяем строковые представления
            Assert.AreEqual("true", truePredicate.ToString(), "True предикат должен возвращать 'true'");
            Assert.AreEqual("false", falsePredicate.ToString(), "False предикат должен возвращать 'false'");
            
            // Проверяем человеко-читаемые представления
            Assert.AreEqual("истина", truePredicate.ToHumanReadable(), "True предикат должен возвращать 'истина'");
            Assert.AreEqual("ложь", falsePredicate.ToHumanReadable(), "False предикат должен возвращать 'ложь'");
            
            // Проверяем, что они не содержат переменных
            Assert.AreEqual(0, truePredicate.GetAllVariables().Count, "True предикат не должен содержать переменных");
            Assert.AreEqual(0, falsePredicate.GetAllVariables().Count, "False предикат не должен содержать переменных");
            
            // Проверяем, что замена переменных не влияет на них
            var replacement = new Variable("x");
            Assert.AreSame(truePredicate, truePredicate.ReplaceVariable("x", replacement), "True предикат должен остаться неизменным");
            Assert.AreSame(falsePredicate, falsePredicate.ReplaceVariable("x", replacement), "False предикат должен остаться неизменным");
            
            // Проверяем синглтон-свойство
            Assert.AreSame(truePredicate, TruePredicate.Instance, "True предикат должен быть синглтоном");
            Assert.AreSame(falsePredicate, FalsePredicate.Instance, "False предикат должен быть синглтоном");
        }
    }
}