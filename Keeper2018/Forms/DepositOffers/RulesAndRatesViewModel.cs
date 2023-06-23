using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using KeeperDomain;

namespace Keeper2018
{
    public class RulesAndRatesViewModel : Screen
    {
        private readonly KeeperDataModel _dataModel;
        private readonly IWindowManager _windowManager;
        public string Title;
        public DepoCondsModel Conditions { get; set; }
        public ObservableCollection<DepositRateLine> Rows { get; set; }
        public DateTime NewDate { get; set; } = DateTime.Today;
        private int _maxDepoRateLineId;

        public Visibility FormulaVisibility { get; set; }

        public List<string> Operations { get; set; } = new List<string>() { "*", "+", "/", "-" };

        private string _selectedOperation;
        public string SelectedOperation
        {
            get => _selectedOperation;
            set
            {
                if (value == _selectedOperation) return;
                _selectedOperation = value;
                Conditions.RateFormula = $"СР {SelectedOperation} {FormulaK}";
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(SaveFormulaAs));
            }
        }

        private double _formulaK;
        public double FormulaK
        {
            get => _formulaK;
            set
            {
                if (value.Equals(_formulaK)) return;
                _formulaK = value;
                Conditions.RateFormula = $"СР {SelectedOperation} {FormulaK}";
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(SaveFormulaAs));
            }
        }

        public string SaveFormulaAs => Conditions.RateFormula;

        public RulesAndRatesViewModel(KeeperDataModel dataModel, IWindowManager windowManager)
        {
            _dataModel = dataModel;
            _windowManager = windowManager;
        }

        public void Initialize(string title, DepoCondsModel conditions, RateType rateType, int maxDepoRateLineId)
        {
            Title = title;
            Conditions = conditions;
            _maxDepoRateLineId = maxDepoRateLineId;

            FormulaVisibility = rateType == RateType.Linked ? Visibility.Visible : Visibility.Collapsed;
            if (rateType == RateType.Linked)
            {
                if (Conditions.RateFormula == null) Conditions.RateFormula = "СР * -1";
                RateFormula.TryParse(conditions.RateFormula, out string op, out double k);
                SelectedOperation = Operations.First(o => o == op);
                FormulaK = k;
            }

            Rows = new ObservableCollection<DepositRateLine>();
            foreach (var rateLine in conditions.RateLines)
            {
                Rows.Add(rateLine);
            }
            if (Rows.Count == 0)
                Rows.Add(CreateRateLine(maxDepoRateLineId + 1,
                    rateType == RateType.Linked ? (decimal)RateFormula.Calculate(Conditions.RateFormula, 1) : 0));
        }

        private DepositRateLine CreateRateLine(int id, decimal rate)
        {
            return new DepositRateLine()
            {
                Id = id,
                DepositOfferConditionsId = Conditions.Id,
                DateFrom = DateTime.Today,
                AmountFrom = 0,
                AmountTo = 999999999999,
                Rate = rate,
            };
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Title;
        }

        public void AddLine()
        {
            var lastLine = Rows.Last();
            var id = Math.Max(_maxDepoRateLineId, Rows.Max(r => r.Id));
            var newLine = new DepositRateLine()
            {
                Id = id + 1,
                DepositOfferConditionsId = lastLine.DepositOfferConditionsId,
                DateFrom = lastLine.DateFrom,
                AmountFrom = lastLine.AmountTo + (decimal)0.01,
                AmountTo = lastLine.AmountTo * 100 - (decimal)0.01,
                Rate = lastLine.Rate,
            };
            Rows.Add(newLine);
        }

        public void RepeatDay()
        {
            var lastLine = Rows.Last();
            var copy = Rows.Where(r => r.DateFrom == lastLine.DateFrom)
                .Select(line => new DepositRateLine()
                {
                    DepositOfferConditionsId = line.DepositOfferConditionsId,
                    DateFrom = NewDate,
                    AmountFrom = line.AmountFrom,
                    AmountTo = line.AmountTo,
                    Rate = line.Rate,
                })
                .ToList();

            var id = Math.Max(_maxDepoRateLineId, Rows.Max(r => r.Id));
            foreach (var line in copy)
            {
                line.Id = ++id;
                Rows.Add(line);
            }
        }

        // a) Button visible only if rate is LINKED
        // b) пока реализовано только для ставки связанной с Ставкой рефинансирования
        // c) чтобы не заводить новые строки с новыми Id - не удаляем существующие строки, а пересчитываем ставку
        public void RecalculateRates()
        {
           UpdateTable();
        }

        // эта функция нужна только если введены неправильные ставки
        public void RecalculateExistingLines()
        {
            var table = Rows.ToList();
            Rows.Clear();

            foreach (var depositRateLine in table)
            {
                var l = _dataModel.RefinancingRates.Last(r => r.Date.Date <= depositRateLine.DateFrom.Date);
                depositRateLine.Rate = (decimal)RateFormula.Calculate(Conditions.RateFormula, l.Value);
                Rows.Add(depositRateLine);
            }
        }

        // таблицы ставок должны обновляться централизовано, после ввода новой ставки рефинансирования
        // единственный случай когда нужна кнопка здесь - когда заводим новый депозит/новые условия
        private void UpdateTable()
        {
            _maxDepoRateLineId = _dataModel.UpdateRateLinesInConditions(Conditions, _maxDepoRateLineId);
            Rows.Clear();
            Conditions.RateLines.ForEach(Rows.Add);
        }

        public override void CanClose(Action<bool> callback)
        {
            if (Rows.Count == 0)
            {
                var vm = new MyMessageBoxViewModel(MessageType.Error, "Таблица не должна быть пустая");
                _windowManager.ShowDialog(vm);
                return;
            }

            Conditions.RateLines = Rows.ToList();
            base.CanClose(callback);
        }

        public void Close()
        {
            TryClose();
        }
    }
}
