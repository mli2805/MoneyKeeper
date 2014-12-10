using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using Keeper.DomainModel;
using Keeper.DomainModel.Deposit;
using Keeper.Utils.Rates;

namespace Keeper.ByFunctional.DepositProcessing
{
    [Export]
    public class DepositTrafficEvaluator
    {
        private readonly RateExtractor _rateExtractor;
        private Deposit _deposit;

        [ImportingConstructor]
        public DepositTrafficEvaluator(RateExtractor rateExtractor)
        {
            _rateExtractor = rateExtractor;
        }

        public Deposit EvaluateTraffic(Deposit deposit)
        {
            _deposit = deposit;
            SummarizeTraffic();
            DefineCurrentState();
            FillinDailyBalances();

            return _deposit;
        }

        private void SummarizeTraffic()
        {
            _deposit.CalculationData.TotalMyIns = _deposit.CalculationData.Traffic.Where(t => t.TransactionType == DepositTransactionTypes.Явнес).Sum(t => t.Amount);
            _deposit.CalculationData.TotalMyOuts = _deposit.CalculationData.Traffic.Where(t => t.TransactionType == DepositTransactionTypes.Расход).Sum(t => t.Amount);
            _deposit.CalculationData.TotalPercent = _deposit.CalculationData.Traffic.Where(t => t.TransactionType == DepositTransactionTypes.Проценты).Sum(t => t.Amount);

            _deposit.CalculationData.CurrentProfitInUsd = _rateExtractor.GetUsdEquivalent(_deposit.CalculationData.CurrentBalance, _deposit.DepositOffer.Currency, DateTime.Today)
                                                          - _deposit.CalculationData.Traffic.Where(t => t.TransactionType == DepositTransactionTypes.Явнес).Sum(t => t.AmountInUsd)
                                                          + _deposit.CalculationData.Traffic.Where(t => t.TransactionType == DepositTransactionTypes.Расход).Sum(t => t.AmountInUsd);
        }

        private void DefineCurrentState()
        {
            if (_deposit.CalculationData.CurrentBalance == 0)
                _deposit.CalculationData.State = DepositStates.Закрыт;
            else
                _deposit.CalculationData.State = _deposit.FinishDate < DateTime.Today ? DepositStates.Просрочен : DepositStates.Открыт;
        }

        private void FillinDailyBalances()
        {
            var period = new Period(_deposit.StartDate, _deposit.FinishDate);
            _deposit.CalculationData.DailyTable = new List<DepositDailyLine>();
            decimal balance = 0;

            foreach (DateTime day in period)
            {
                var date = day;
                balance += _deposit.CalculationData.Traffic.Where(t => t.Timestamp.Date == date.Date).Sum(t => t.Amount * t.Destination());
                _deposit.CalculationData.DailyTable.Add(new DepositDailyLine { Date = day, Balance = balance });
            }
        }

    }
}