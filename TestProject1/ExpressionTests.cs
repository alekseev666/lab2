using lab2.Models;

namespace TestProject1
{
    /// <summary>
    /// Тесты для классов математических выражений
    /// </summary>
    [TestClass]
    public class ExpressionTests
    {
        /// <summary>
        /// Тест 1: Проверяем простую переменную
        /// Убеждаемся, что переменная корректно создается и возвращает свое имя
        /// </summary>
        [TestMethod]
        public void Variable_ShouldReturnCorrectName()
        {
            // Arrange (Подготовка)
            string expectedName = "x";

            // Act (Выполнение)
            var variable = new Variable(expectedName);

            // Assert (Проверка)
            Assert.AreEqual(expectedName, variable.Name, "Имя переменной должно совпадать с заданным");
            Assert.AreEqual(expectedName, variable.ToString(), "ToString() должен возвращать имя переменной");
        }

        /// <summary>
        /// Тест 2: Проверяем замену переменной на выражение
        /// Пример: x заменить на (5 + 2) в выражении x + 10
        /// Результат должен быть: (5 + 2) + 10
        /// </summary>
        [TestMethod]
        public void BinaryOperation_ShouldReplaceVariableCorrectly()
        {
            // Arrange (Подготовка)
            var x = new Variable("x");
            var ten = new Constant(10);
            var five = new Constant(5);
            var two = new Constant(2);
            
            // Создаем выражение: x + 10
            var originalExpression = new BinaryOperation(x, "+", ten);
            
            // Создаем выражение для замены: 5 + 2
            var replacementExpression = new BinaryOperation(five, "+", two);

            // Act (Выполнение)
            // Заменяем x на (5 + 2)
            var result = originalExpression.ReplaceVariable("x", replacementExpression);

            // Assert (Проверка)
            Assert.IsInstanceOfType(result, typeof(BinaryOperation), "Результат должен быть бинарной операцией");
            
            var binaryResult = (BinaryOperation)result;
            Assert.AreEqual("+", binaryResult.Operator, "Оператор должен остаться +");
            
            // Левая часть должна быть заменена на (5 + 2)
            Assert.IsInstanceOfType(binaryResult.Left, typeof(BinaryOperation), "Левая часть должна быть бинарной операцией");
            
            // Правая часть должна остаться 10
            Assert.IsInstanceOfType(binaryResult.Right, typeof(Constant), "Правая часть должна остаться константой");
            var rightConstant = (Constant)binaryResult.Right;
            Assert.AreEqual(10, rightConstant.Value, "Правая часть должна быть равна 10");
            
            // Проверяем строковое представление
            string expectedString = "((5 + 2) + 10)";
            Assert.AreEqual(expectedString, result.ToString(), "Строковое представление должно быть корректным");
        }

        /// <summary>
        /// Тест 3: Проверяем получение всех переменных из сложного выражения
        /// Пример: (x + y) * (z - x) должно содержать переменные: x, y, z
        /// </summary>
        [TestMethod]
        public void ComplexExpression_ShouldReturnAllVariables()
        {
            // Arrange (Подготовка)
            var x = new Variable("x");
            var y = new Variable("y");
            var z = new Variable("z");
            
            // Создаем выражение: (x + y) * (z - x)
            var leftPart = new BinaryOperation(x, "+", y);          // x + y
            var rightPart = new BinaryOperation(z, "-", x);         // z - x  
            var complexExpression = new BinaryOperation(leftPart, "*", rightPart); // (x + y) * (z - x)

            // Act (Выполнение)
            var variables = complexExpression.GetAllVariables();

            // Assert (Проверка)
            Assert.AreEqual(3, variables.Count, "Должно быть найдено 3 уникальные переменные");
            Assert.IsTrue(variables.Contains("x"), "Должна содержаться переменная x");
            Assert.IsTrue(variables.Contains("y"), "Должна содержаться переменная y");
            Assert.IsTrue(variables.Contains("z"), "Должна содержаться переменная z");
        }

        /// <summary>
        /// Тест 4: Проверяем корректность работы с константами
        /// Константы не должны содержать переменных и не должны изменяться при замене
        /// </summary>
        [TestMethod]
        public void Constant_ShouldBehaveCorrectly()
        {
            // Arrange (Подготовка)
            double value = 42.5;
            var constant = new Constant(value);
            var replacement = new Variable("x");

            // Act & Assert (Выполнение и проверка)
            Assert.AreEqual(value, constant.Value, "Значение константы должно совпадать");
            Assert.AreEqual(value.ToString(), constant.ToString(), "ToString должен возвращать значение");
            
            // Константа не должна содержать переменных
            var variables = constant.GetAllVariables();
            Assert.AreEqual(0, variables.Count, "Константа не должна содержать переменных");
            
            // Константа не должна изменяться при замене переменных
            var result = constant.ReplaceVariable("x", replacement);
            Assert.AreSame(constant, result, "Константа должна остаться неизменной при замене переменных");
        }
    }
}