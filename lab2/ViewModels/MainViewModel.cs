using lab2.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace lab2.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private string _codeInput = "";
        private string _postconditionInput = "";
        private string _postconditionHumanInput = "";
        private WpResult _currentResult;
        private bool _isCalculating = false;
        private PresetExample _selectedPreset;

        public string CodeInput
        {
            get => _codeInput;
            set => SetProperty(ref _codeInput, value);
        }

        public string PostconditionInput
        {
            get => _postconditionInput;
            set => SetProperty(ref _postconditionInput, value);
        }

        public string PostconditionHumanInput
        {
            get => _postconditionHumanInput;
            set => SetProperty(ref _postconditionHumanInput, value);
        }

        public WpResult CurrentResult
        {
            get => _currentResult;
            set => SetProperty(ref _currentResult, value);
        }

        public bool IsCalculating
        {
            get => _isCalculating;
            set => SetProperty(ref _isCalculating, value);
        }

        public PresetExample SelectedPreset
        {
            get => _selectedPreset;
            set
            {
                if (SetProperty(ref _selectedPreset, value) && value != null)
                {
                    LoadPreset(value);
                }
            }
        }

        public ObservableCollection<PresetExample> Presets { get; }

        public ICommand CalculateWpCommand { get; }
        public ICommand ShowTripleCommand { get; }
        public ICommand ClearCommand { get; }

        public MainViewModel()
        {
            Presets = new ObservableCollection<PresetExample>(PresetExample.GetDefaultPresets());

            CalculateWpCommand = new RelayCommand(CalculateWp, CanCalculateWp);
            ShowTripleCommand = new RelayCommand(ShowTriple, () => CurrentResult != null && !CurrentResult.HasErrors);
            ClearCommand = new RelayCommand(Clear);

            // Загружаем первый пример по умолчанию
            if (Presets.Any())
            {
                SelectedPreset = Presets.First();
            }
        }

        private void LoadPreset(PresetExample preset)
        {
            CodeInput = preset.Code;
            PostconditionInput = preset.Postcondition;
            PostconditionHumanInput = preset.Description;
        }

        private bool CanCalculateWp()
        {
            return !IsCalculating &&
                   !string.IsNullOrWhiteSpace(CodeInput) &&
                   !string.IsNullOrWhiteSpace(PostconditionInput);
        }

        private void CalculateWp()
        {
            IsCalculating = true;
            CurrentResult = null;

            try
            {
                var result = new WpResult
                {
                    OriginalCode = CodeInput,
                    OriginalPostcondition = PostconditionInput
                };

                // Парсим код и постусловие
                var statement = Parser.ParseStatement(CodeInput);
                var postcondition = Parser.ParsePredicate(PostconditionInput);

                result.Steps.Add(new WpCalculationStep(
                    "Исходные данные",
                    $"Программа: {CodeInput}",
                    $"Постусловие: {PostconditionInput}",
                    $"Постусловие: {postcondition.ToHumanReadable()}"
                ));

                // Вычисляем wp с пошаговым трейсом
                var finalPrecondition = CalculateWpWithTrace(statement, postcondition, result.Steps);

                result.FinalPrecondition = finalPrecondition;

                result.Steps.Add(new WpCalculationStep(
                    "Итоговое предусловие",
                    "Завершение вычисления",
                    finalPrecondition.ToString(),
                    $"Финальное предусловие: {finalPrecondition.ToHumanReadable()}"
                ));

                CurrentResult = result;
            }
            catch (Exception ex)
            {
                CurrentResult = new WpResult
                {
                    HasErrors = true,
                    ErrorMessage = ex.Message,
                    OriginalCode = CodeInput,
                    OriginalPostcondition = PostconditionInput
                };
            }
            finally
            {
                IsCalculating = false;
            }
        }

        private Predicate CalculateWpWithTrace(Statement statement, Predicate postcondition, List<WpCalculationStep> steps)
        {
            switch (statement)
            {
                case Assignment assignment:
                    steps.Add(new WpCalculationStep(
                        "Обработка присваивания",
                        assignment.ToString(),
                        $"Заменяем {assignment.Variable} на {assignment.Expression} в {postcondition}",
                        $"Применяем правило для присваивания: {assignment.ToHumanReadable()}"
                    ));

                    var result = assignment.WeakestPrecondition(postcondition);

                    steps.Add(new WpCalculationStep(
                        "Результат подстановки",
                        assignment.ToString(),
                        result.ToString(),
                        result.ToHumanReadable()
                    ));

                    return result;

                case Sequence sequence:
                    steps.Add(new WpCalculationStep(
                        "Начало обработки последовательности",
                        sequence.ToString(),
                        postcondition.ToString(),
                        "Обрабатываем операторы с конца к началу"
                    ));

                    var currentPredicate = postcondition;

                    // Обрабатываем операторы с конца
                    for (int i = sequence.Statements.Count - 1; i >= 0; i--)
                    {
                        var stmt = sequence.Statements[i];

                        steps.Add(new WpCalculationStep(
                            $"Обработка оператора {sequence.Statements.Count - i} с конца",
                            stmt.ToString(),
                            currentPredicate.ToString(),
                            $"Текущее условие: {currentPredicate.ToHumanReadable()}"
                        ));

                        currentPredicate = CalculateWpWithTrace(stmt, currentPredicate, steps);
                    }

                    return currentPredicate;

                case Conditional conditional:
                    steps.Add(new WpCalculationStep(
                        "Начало обработки условного оператора",
                        conditional.ToString(),
                        postcondition.ToString(),
                        "Применяем правило для if-else: (B ∧ wp(S1,R)) ∨ (¬B ∧ wp(S2,R))"
                    ));

                    // Обрабатываем then-ветку
                    steps.Add(new WpCalculationStep(
                        "Обработка THEN-ветки",
                        conditional.ThenBranch.ToString(),
                        postcondition.ToString(),
                        "Вычисляем wp для ветки 'тогда'"
                    ));

                    var thenWp = CalculateWpWithTrace(conditional.ThenBranch, postcondition, steps);

                    // Обрабатываем else-ветку  
                    steps.Add(new WpCalculationStep(
                        "Обработка ELSE-ветки",
                        conditional.ElseBranch.ToString(),
                        postcondition.ToString(),
                        "Вычисляем wp для ветки 'иначе'"
                    ));

                    var elseWp = CalculateWpWithTrace(conditional.ElseBranch, postcondition, steps);

                    // Формируем итоговое условие
                    var condition = conditional.Condition;
                    var notCondition = new NotPredicate(condition);
                    var thenCondition = new LogicalPredicate(condition, "∧", thenWp);
                    var elseCondition = new LogicalPredicate(notCondition, "∧", elseWp);
                    var finalCondition = new LogicalPredicate(thenCondition, "∨", elseCondition);

                    steps.Add(new WpCalculationStep(
                        "Формирование итогового условия",
                        conditional.ToString(),
                        finalCondition.ToString(),
                        $"Объединяем ветки: ({condition.ToHumanReadable()} И {thenWp.ToHumanReadable()}) ИЛИ (НЕ({condition.ToHumanReadable()}) И {elseWp.ToHumanReadable()})"
                    ));

                    return finalCondition;

                default:
                    throw new ArgumentException($"Неизвестный тип оператора: {statement.GetType().Name}");
            }
        }

        private void ShowTriple()
        {
            if (CurrentResult != null && !CurrentResult.HasErrors)
            {
                System.Windows.MessageBox.Show(
                    CurrentResult.GetHoareTriple(),
                    "Триада Хоара",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Information
                );
            }
        }

        private void Clear()
        {
            CodeInput = "";
            PostconditionInput = "";
            PostconditionHumanInput = "";
            CurrentResult = null;
            SelectedPreset = null;
        }
    }
}
