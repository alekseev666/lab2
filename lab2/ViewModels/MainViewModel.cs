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
        private bool _isCalculating;
        private PresetExample _selectedPreset;
        private int _stepCounter;

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
                    LoadPreset(value);
            }
        }

        public ObservableCollection<PresetExample> Presets { get; }
        public ICommand CalculateWpCommand { get; }
        public ICommand ShowTripleCommand { get; }
        public ICommand ClearCommand { get; }
        public ICommand ShowHelpCommand { get; }

        public MainViewModel()
        {
            Presets = new ObservableCollection<PresetExample>(PresetExample.GetDefaultPresets());
            CalculateWpCommand = new RelayCommand(CalculateWp, CanCalculateWp);
            ShowTripleCommand = new RelayCommand(ShowTriple, () => CurrentResult != null && !CurrentResult.HasErrors);
            ClearCommand = new RelayCommand(Clear);
            ShowHelpCommand = new RelayCommand(ShowHelp);

            if (Presets.Any())
                SelectedPreset = Presets.First();
        }

        private void LoadPreset(PresetExample preset)
        {
            CodeInput = preset.Code;
            PostconditionInput = preset.Postcondition;
            PostconditionHumanInput = preset.Description;
        }

        private bool CanCalculateWp()
        {
            return !IsCalculating && !string.IsNullOrWhiteSpace(CodeInput) && !string.IsNullOrWhiteSpace(PostconditionInput);
        }

        private void CalculateWp()
        {
            IsCalculating = true;
            CurrentResult = null;
            _stepCounter = 0;

            try
            {
                var result = new WpResult { OriginalCode = CodeInput, OriginalPostcondition = PostconditionInput };
                var statement = Parser.ParseStatement(CodeInput);
                var postcondition = Parser.ParsePredicate(PostconditionInput).Simplify();


                result.Steps.Add(new WpCalculationStep(
                    ++_stepCounter,
                    "Исходное постусловие",
                    "",
                    postcondition.ToString(),
                    $"Нужно добиться: {postcondition.ToHumanReadable()}"
                ));

                if (postcondition is FalsePredicate)
                {
                    result.FinalPrecondition = FalsePredicate.Instance;
                    CurrentResult = result;
                    return;
                }

                var finalPrecondition = CalculateWpWithTrace(statement, postcondition, result.Steps).Simplify();
                result.FinalPrecondition = finalPrecondition;

                if (finalPrecondition is FalsePredicate)
                {
                    result.Steps.Add(new WpCalculationStep(
                        ++_stepCounter,
                        "Итог: постусловие недостижимо",
                        finalPrecondition.ToString(),
                        "",
                        "Цель недостижима (wp = ложь)"
                    ));
                }
                else
                {
                    result.Steps.Add(new WpCalculationStep(
                        ++_stepCounter,
                        "Итоговое предусловие",
                        finalPrecondition.ToString(),
                        "",
                        $"Должно выполняться ДО программы: {finalPrecondition.ToHumanReadable()}"
                    ));
                }

                CurrentResult = result;
            }
            catch (Exception ex)
            {
                CurrentResult = new WpResult { HasErrors = true, ErrorMessage = ex.Message, OriginalCode = CodeInput, OriginalPostcondition = PostconditionInput };
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
                case Assignment assign:
                    var result = assign.WeakestPrecondition(postcondition).Simplify();
                    steps.Add(new WpCalculationStep(
                        ++_stepCounter,
                        $"{assign.Variable} := {assign.Expression}",
                        postcondition.ToString(),
                        result.ToString(),
                        $"Заменяем '{assign.Variable}' -> '{assign.Expression}'"
                    ));
                    return result;

                case Sequence seq:
                    var current = postcondition;
                    for (int i = seq.Statements.Count - 1; i >= 0; i--)
                    {
                        current = CalculateWpWithTrace(seq.Statements[i], current, steps);
                    }
                    return current;

                case Conditional cond:
                    var thenWp = CalculateWpWithTrace(cond.ThenBranch, postcondition, steps).Simplify();
                    var elseWp = CalculateWpWithTrace(cond.ElseBranch, postcondition, steps).Simplify();

                    var thenPart = new LogicalPredicate(cond.Condition, "∧", thenWp);
                    var elsePart = new LogicalPredicate(new NotPredicate(cond.Condition), "∧", elseWp);
                    var finalCond = new LogicalPredicate(thenPart, "∨", elsePart).Simplify();

                    steps.Add(new WpCalculationStep(
                        ++_stepCounter,
                        $"if ({cond.Condition})",
                        postcondition.ToString(),
                        finalCond.ToString(),
                        $"Объединяем ветки: ({cond.Condition} ∧ {thenWp}) ∨ (¬{cond.Condition} ∧ {elseWp})"
                    ));
                    return finalCond;

                default:
                    throw new ArgumentException($"Неизвестный тип: {statement.GetType().Name}");
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
                    System.Windows.MessageBoxImage.Information);
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

        private void ShowHelp()
        {
            new lab2.Views.HelpWindow().ShowDialog();
        }
    }
}
