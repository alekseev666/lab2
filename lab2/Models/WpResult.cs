using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lab2.Models
{
    public class WpCalculationStep
    {
        public string Description { get; set; }
        public string StatementProcessed { get; set; }
        public string IntermediatePredicate { get; set; }
        public string HumanReadable { get; set; }

        public WpCalculationStep(string description, string statement, string predicate, string humanReadable)
        {
            Description = description;
            StatementProcessed = statement;
            IntermediatePredicate = predicate;
            HumanReadable = humanReadable;
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
            HasErrors = false;
        }

        public string GetHoareTriple()
        {
            if (HasErrors || FinalPrecondition == null)
                return "Невозможно построить триаду Хоара из-за ошибок";

            return $"{{ {FinalPrecondition.ToHumanReadable()} }} {OriginalCode} {{ {GetPostconditionHumanReadable()} }}";
        }

        private string GetPostconditionHumanReadable()
        {
            // Простая попытка сделать постусловие читаемым
            return OriginalPostcondition?.Replace("&&", " И ")
                                        ?.Replace("||", " ИЛИ ")
                                        ?.Replace("==", " равно ")
                                        ?.Replace(">=", " больше или равно ")
                                        ?.Replace("<=", " меньше или равно ")
                                        ?.Replace(">", " больше ")
                                        ?.Replace("<", " меньше ")
                                        ?? "неизвестное постусловие";
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
                    "Демонстрация протягивания условия через цепочку присваиваний",
                    "x := x + 10; y := x + 1",
                    "y == x - 9 && x > 15"
                ),
                new PresetExample(
                    "Квадратное уравнение (упрощенное)",
                    "Вычисление корня квадратного уравнения при положительном дискриминанте",
                    "if (d >= 0) { root := (-b + d) / (2 * a) } else { root := -999 }",
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
