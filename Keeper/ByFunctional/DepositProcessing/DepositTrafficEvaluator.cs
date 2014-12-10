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
        private readonly KeeperDb _db;
        private readonly RateExtractor _rateExtractor;
        private Deposit _deposit;

        [ImportingConstructor]
        public DepositTrafficEvaluator(KeeperDb db, RateExtractor rateExtractor)
        {
            _db = db;
            _rateExtractor = rateExtractor;
        }

        public Deposit EvaluateTraffic(Deposit deposit)
        {
            _deposit = deposit;
            SummarizeTraffic();
            DefineCurrentState();
            FillinDailyBalances();
            DefineUsdEquivalents();

            return _deposit;
        }

        private void SummarizeTraffic()
        {
            _deposit.CalculationData.TotalMyIns = _deposit.CalculationData.Traffic.Where(t => t.TransactionType == DepositTransactionTypes.Явнес).Sum(t => t.Amount);
            _deposit.CalculationData.TotalMyOuts = _deposit.CalculationData.Traffic.Where(t => t.TransactionType == DepositTransactionTypes.Расход).Sum(t => t.Amount);

            _deposit.CalculationData.TotalMyOutsInUsd = _deposit.CalculationData.Traffic.Where(t => t.TransactionType == DepositTransactionTypes.Расход).Sum(t => t.AmountInUsd);


            _deposit.CalculationData.TotalPercent = _deposit.CalculationData.Traffic.Where(t => t.TransactionType == DepositTransactionTypes.Проценты).Sum(t => t.Amount);

            _deposit.CalculationData.CurrentProfitInUsd = 
                _rateExtractor.GetUsdEquivalent(_deposit.CalculationData.CurrentBalance, _deposit.DepositOffer.Currency, DateTime.Today)
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


        public class Myclass
        {
            DateTime Date { get; set; }
            decimal Balance { get; set; }
            double Rate { get; set; }
            decimal BalanceInUsd { get; set; }

            public Myclass(DateTime date, decimal balance, double rate, decimal balanceInUsd)
            {
                Date = date;
                Balance = balance;
                Rate = rate;
                BalanceInUsd = balanceInUsd;
            }
        }

        public class CurrencyPair
        {
            private CurrencyCodes _currency;
            private DateTime _date;

            public CurrencyPair(CurrencyCodes currency, DateTime date)
            {
                _currency = currency;
                _date = date;
            }
        }

        /// <summary>
        /// http://msdn.microsoft.com/ru-ru/library/bb311040.aspx
        /// </summary>
        private void DefineUsdEquivalents()
        {
            var temp =
                from line in _deposit.CalculationData.DailyTable orderby line.Date
                join rate in _db.CurrencyRates 
                  on new CurrencyPair(_deposit.DepositOffer.Currency, line.Date) equals new CurrencyPair(rate.Currency, rate.BankDay)
                select new { line.Date,  line.Balance, rate.Rate,  InUsd = line.Balance / (decimal)rate.Rate};


        }
    }
}