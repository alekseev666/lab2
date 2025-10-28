using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lab2.Models
{
    public class WpCalculationStep
    {
        public int StepNumber { get; set; }
        public string Operation { get; set; }
        public string Before { get; set; }
        public string After { get; set; }
        public string Explanation { get; set; }

        // Backward-compatible properties for XAML bindings
        public string Description => StepNumber > 0 ? $"Шаг {StepNumber}: {Operation}" : Operation;
        public string StatementProcessed => Operation;
        public string IntermediatePredicate => After;
        public string HumanReadable => Explanation;

        public WpCalculationStep(int step, string op, string before, string after, string explain)
        {
            StepNumber = step;
            Operation = op;
            Before = before;
            After = after;
            Explanation = explain;
        }
    }

    public class WpResult
    {
        public Predicate FinalPrecondition { get; set; }
        public List<WpCalculationStep> Steps { get; set; }
        public string OriginalCode { get; set; }
        public string OriginalPostcondition { get; set; }
        public bool HasErrors { get; set; }
        public string ErrorMessage { get; set; }

        public WpResult()
        {
            Steps = new List<WpCalculationStep>();
        }

        public string GetHoareTriple()
        {
            if (HasErrors || FinalPrecondition == null)
                return "Ошибка построения триады";
            return $"{{ {FinalPrecondition} }} {OriginalCode} {{ {OriginalPostcondition} }}";
        }

    }

    public class PresetExample
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Code { get; set; }
        public string Postcondition { get; set; }

        public PresetExample(string name, string description, string code, string postcondition)
        {
            Name = name;
            Description = description;
            Code = code;
            Postcondition = postcondition;
        }

        public static List<PresetExample> GetDefaultPresets()
        {
            return new List<PresetExample>
            {
                new PresetExample(
                    "Максимум из двух",
                    "Находит максимум из двух переменных x1 и x2",
                    "if (x1 >= x2) { max := x1 } else { max := x2 }",
                    "max > 100"
                ),
                new PresetExample(
                    "Последовательность присваиваний",
                    "Цель y = x − 9 и x > 15 тянется назад через x := x+10; y := x - 9 до x > 5",
                    "x := x + 10; y := x - 9",
                    "y == x - 9 && x > 15"
                ),
                new PresetExample(
                    "Квадратное уравнение (упрощенное)",
                    "Вычисление корня квадратного уравнения при положительном дискриминанте",
                    "if (d >= 0) { root := d + b } else { root := -999 }",
                    "root != -999"
                ),
                new PresetExample(
                    "Деление с проверкой",
                    "Демонстрация условий определенности для деления",
                    "if (y != 0) { result := x / y } else { result := 0 }",
                    "result > 5"
                ),
                new PresetExample(
                    "Простое присваивание",
                    "Базовый случай для понимания подстановки",
                    "x := 2 * x + 5",
                    "x > 15"
                )
            };
        }
    }
}