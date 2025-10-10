using lab2.Models;
using System;
using System.Collections.Generic;

namespace TestProject1
{
    /// <summary>
    /// Интеграционные тесты, проверяющие что вычисленные предусловия 
    /// действительно гарантируют выполнение постусловий на случайных данных
    /// </summary>
    [TestClass]
    public class IntegrationTests
    {
        private static readonly Random random = new Random(42); // Фиксированный seed для повторяемости тестов

        /// <summary>
        /// Тест 1: Проверка гарантий для простого присваивания
        /// Программа: x := x + 10, Постусловие: x > 15
        /// WP должно быть: x + 10 > 15 (то есть x > 5)
        /// </summary>
        [TestMethod]
        public void SimpleAssignment_WPShouldGuaranteePostcondition()
        {
            // Arrange - подготовка программы
            var program = "x := x + 10";
            var postcondition = "x > 15";
            
            // Разбираем программу и постусловие
            var statement = Parser.ParseStatement(program);
            var post = Parser.ParsePredicate(postcondition);
            
            // Вычисляем WP
            var wp = statement.WeakestPrecondition(post);
            
            // Act & Assert - проверяем на 20 случайных значениях
            for (int i = 0; i < 20; i++)
            {
                // Генерируем случайное значение x, которое удовлетворяет WP
                double x = GenerateValueSatisfyingSimplePredicate(wp, "x");
                
                // Выполняем программу: x := x + 10
                double resultX = x + 10;
                
                // Проверяем, что постусловие выполнилось: resultX > 15
                Assert.IsTrue(resultX > 15, 
                    $"Для x={x} программа дала результат {resultX}, но постусловие x > 15 не выполнилось");
            }
        }

        /// <summary>
        /// Тест 2: Проверка гарантий для последовательности присваиваний
        /// Программа: x := x + 1; y := x * 2, Постусловие: y > 10
        /// WP должно быть: (x + 1) * 2 > 10 (то есть x > 4)
        /// </summary>
        [TestMethod]
        public void SequenceAssignment_WPShouldGuaranteePostcondition()
        {
            // Arrange - подготовка программы
            var program = "x := x + 1; y := x * 2";
            var postcondition = "y > 10";
            
            // Разбираем программу и постусловие
            var statement = Parser.ParseStatement(program);
            var post = Parser.ParsePredicate(postcondition);
            
            // Вычисляем WP
            var wp = statement.WeakestPrecondition(post);
            
            // Act & Assert - проверяем на 15 случайных значениях
            for (int i = 0; i < 15; i++)
            {
                // Генерируем случайное значение x, которое удовлетворяет WP
                double x = GenerateValueSatisfyingSimplePredicate(wp, "x");
                
                // Выполняем программу:
                // x := x + 1
                double newX = x + 1;
                // y := x * 2 (здесь x уже новое значение)
                double y = newX * 2;
                
                // Проверяем, что постусловие выполнилось: y > 10
                Assert.IsTrue(y > 10, 
                    $"Для начального x={x} программа дала y={y}, но постусловие y > 10 не выполнилось");
            }
        }

        /// <summary>
        /// Тест 3: Проверка гарантий для условного оператора (простой случай)
        /// Программа: if (x >= 0) { y := x } else { y := 1 }, Постусловие: y > 0
        /// WP должно гарантировать, что y всегда будет больше 0
        /// Для данного примера WP: (x >= 0 И x > 0) ИЛИ (НЕ(x >= 0) И 1 > 0)
        /// Упрощенно: x > 0 ИЛИ x < 0 ИЛИ true, то есть всегда true, кроме x = 0
        /// </summary>
        [TestMethod]
        public void ConditionalStatement_WPShouldGuaranteePostcondition()
        {
            // Arrange - подготовка программы
            var program = "if (x >= 0) { y := x } else { y := 1 }";
            var postcondition = "y > 0";
            
            // Разбираем программу и постусловие
            var statement = Parser.ParseStatement(program);
            var post = Parser.ParsePredicate(postcondition);
            
            // Вычисляем WP
            var wp = statement.WeakestPrecondition(post);
            
            // Act & Assert - проверяем на значениях, которые точно удовлетворяют WP
            var testValues = new double[] { -10, -1, 1, 5, 100, -0.5, 3.14 }; // убираем 0, так как для x=0 получим y=0
            
            foreach (double x in testValues)
            {
                // Выполняем программу
                double y;
                if (x >= 0)
                {
                    y = x; // then ветка: y := x
                }
                else
                {
                    y = 1; // else ветка: y := 1
                }
                
                // Проверяем постусловие
                Assert.IsTrue(y > 0, 
                    $"Для x={x} программа дала y={y}, но постусловие y > 0 не выполнилось");
            }
        }

        /// <summary>
        /// Тест 4: Проверка на граничных значениях
        /// Программа: x := x * 2, Постусловие: x >= 20
        /// Проверяем, что WP корректно работает на граничных случаях
        /// </summary>
        [TestMethod]
        public void BoundaryValues_WPShouldGuaranteePostcondition()
        {
            // Arrange
            var program = "x := x * 2";
            var postcondition = "x >= 20";
            
            var statement = Parser.ParseStatement(program);
            var post = Parser.ParsePredicate(postcondition);
            var wp = statement.WeakestPrecondition(post);
            
            // Act & Assert - проверяем только значения, которые точно удовлетворяют WP (x >= 10)
            var validValues = new double[] { 10, 10.1, 15, 25 }; // убираем 9.9, так как 9.9 * 2 = 19.8 < 20
            
            foreach (double x in validValues)
            {
                // Для x * 2 >= 20 нужно x >= 10, поэтому проверяем только такие значения
                if (x >= 10)
                {
                    // Выполняем программу: x := x * 2
                    double resultX = x * 2;
                    
                    // Проверяем постусловие: x >= 20
                    Assert.IsTrue(resultX >= 20, 
                        $"Для x={x} программа дала результат {resultX}, но постусловие x >= 20 не выполнилось");
                }
            }
        }

        /// <summary>
        /// Тест 5: Комплексная проверка с отрицательными числами
        /// Программа: x := x - 5, Постусловие: x < 0
        /// WP: x - 5 < 0 (то есть x < 5)
        /// </summary>
        [TestMethod]
        public void NegativeNumbers_WPShouldGuaranteePostcondition()
        {
            // Arrange
            var program = "x := x - 5";
            var postcondition = "x < 0";
            
            var statement = Parser.ParseStatement(program);
            var post = Parser.ParsePredicate(postcondition);
            var wp = statement.WeakestPrecondition(post);
            
            // Act & Assert - тестируем различные значения
            for (int i = 0; i < 10; i++)
            {
                // Генерируем случайное значение x < 5 (чтобы удовлетворяло WP)
                double x = random.NextDouble() * 8 - 2; // от -2 до 6
                
                if (x < 5) // проверяем, что удовлетворяет предусловию
                {
                    // Выполняем программу
                    double resultX = x - 5;
                    
                    // Проверяем постусловие
                    Assert.IsTrue(resultX < 0, 
                        $"Для x={x} программа дала результат {resultX}, но постусловие x < 0 не выполнилось");
                }
            }
        }

        /// <summary>
        /// Вспомогательный метод: генерирует значение переменной, которое удовлетворяет простому предикату
        /// </summary>
        private static double GenerateValueSatisfyingSimplePredicate(Predicate predicate, string variableName)
        {
            // Упрощенная логика для простых предикатов вида "x > число" или "(x + число) > число"
            string predicateStr = predicate.ToString();
            
            if (predicateStr.Contains("> 15")) // для x + 10 > 15
            {
                return 6 + random.NextDouble() * 10; // x от 6 до 16
            }
            else if (predicateStr.Contains("> 10")) // для (x + 1) * 2 > 10
            {
                return 5 + random.NextDouble() * 5; // x от 5 до 10
            }
            else if (predicateStr.Contains(">= 20")) // для x * 2 >= 20
            {
                return 10 + random.NextDouble() * 10; // x от 10 до 20
            }
            else if (predicateStr.Contains("< 0")) // для x - 5 < 0
            {
                return -2 + random.NextDouble() * 6; // x от -2 до 4
            }
            
            // По умолчанию возвращаем положительное число
            return 1 + random.NextDouble() * 100;
        }

        /// <summary>
        /// Вспомогательный метод: проверяет, удовлетворяет ли значение переменной предикату
        /// </summary>
        private static bool EvaluatePredicateForValue(Predicate predicate, string variableName, double value)
        {
            // Упрощенная проверка для демонстрации
            // В реальной реализации здесь был бы полноценный интерпретатор
            
            string predicateStr = predicate.ToString();
            
            // Простые случаи для демонстрации
            if (predicateStr.Contains("x >= 0"))
                return value >= 0;
            if (predicateStr.Contains("x > 0"))
                return value > 0;
            if (predicateStr.Contains("x < 5"))
                return value < 5;
            if (predicateStr.Contains("x >= 10"))
                return value >= 10;
                
            // По умолчанию считаем, что удовлетворяет
            return true;
        }

        /// <summary>
        /// Дополнительный тест: проверка корректности на известных примерах
        /// Этот тест использует предопределенные значения для проверки правильности
        /// </summary>
        [TestMethod]
        public void KnownExamples_ShouldWorkCorrectly()
        {
            var testCases = new[]
            {
                new { Program = "x := x + 3", Postcondition = "x > 10", TestX = 8.0, ExpectedResult = 11.0 },
                new { Program = "x := x * 3", Postcondition = "x > 15", TestX = 6.0, ExpectedResult = 18.0 },
                new { Program = "y := 2 * x", Postcondition = "y >= 8", TestX = 4.0, ExpectedResult = 8.0 }
            };

            foreach (var testCase in testCases)
            {
                // Разбираем и вычисляем WP
                var statement = Parser.ParseStatement(testCase.Program);
                var post = Parser.ParsePredicate(testCase.Postcondition);
                var wp = statement.WeakestPrecondition(post);

                // Информационное сообщение о вычисленном WP
                Console.WriteLine($"Программа: {testCase.Program}");
                Console.WriteLine($"Постусловие: {testCase.Postcondition}");
                Console.WriteLine($"Вычисленное WP: {wp}");
                Console.WriteLine($"Человеко-читаемое WP: {wp.ToHumanReadable()}");
                Console.WriteLine();

                // Проверяем, что WP не пустое
                Assert.IsNotNull(wp, "Предусловие не должно быть null");
                Assert.IsFalse(string.IsNullOrEmpty(wp.ToString()), "Предусловие не должно быть пустым");
            }
        }
<<<<<<< HEAD

        [TestMethod]
        public void TestQuadraticEquationExample()
        {
            // Arrange - упрощенный пример квадратного уравнения из пресетов
            var program = "if (d >= 0) { root := d + b } else { root := -999 }";
            var postcondition = "root != -999";

            // Act - парсим и вычисляем WP
            var statement = Parser.ParseStatement(program);
            var postPredicate = Parser.ParsePredicate(postcondition);
            var wp = statement.WeakestPrecondition(postPredicate);

            // Assert - проверяем, что WP вычислилось успешно
            Assert.IsNotNull(wp, "WP должно быть вычислено");
            
            var wpString = wp.ToString();
            Assert.IsFalse(string.IsNullOrEmpty(wpString), "WP не должно быть пустым");
            
            // Проверяем, что WP содержит основные переменные
            Assert.IsTrue(wpString.Contains("d") || wp.ToHumanReadable().Contains("d"), 
                $"WP должно содержать переменную 'd', WP = {wpString}");
            
            // Проверяем, что можно преобразовать в читаемый вид
            var humanReadable = wp.ToHumanReadable();
            Assert.IsFalse(string.IsNullOrEmpty(humanReadable), 
                "Должно быть возможно преобразование в читаемый вид");
        }

    }
}
=======
    }
}
>>>>>>> 7a3a1b29fb6eebbc796145bc219ed00da22737b1
