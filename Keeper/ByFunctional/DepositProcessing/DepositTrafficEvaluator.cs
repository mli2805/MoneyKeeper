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
            if (_deposit.DepositOffer.Currency != CurrencyCodes.USD) DefineCurrencyRates();

            return _deposit;
        }

        private void SummarizeTraffic()
        {
            _deposit.CalculationData.TotalMyIns = _deposit.CalculationData.Traffic.Where(t => t.TransactionType == DepositTransactionTypes.явнес).Sum(t => t.Amount);

            _deposit.CalculationData.TotalMyOuts = _deposit.CalculationData.Traffic.Where(t => t.TransactionType == DepositTransactionTypes.–асход).Sum(t => t.Amount);
            _deposit.CalculationData.TotalMyOutsInUsd = _deposit.CalculationData.Traffic.Where(t => t.TransactionType == DepositTransactionTypes.–асход).Sum(t => t.AmountInUsd);

            _deposit.CalculationData.TotalPercent = _deposit.CalculationData.Traffic.Where(t => t.TransactionType == DepositTransactionTypes.ѕроценты).Sum(t => t.Amount);
            _deposit.CalculationData.TotalPercentInUsd = _deposit.CalculationData.Traffic.Where(t => t.TransactionType == DepositTransactionTypes.ѕроценты).Sum(t => t.AmountInUsd);

            _deposit.CalculationData.CurrentProfitInUsd =
                _rateExtractor.GetUsdEquivalent(_deposit.CalculationData.CurrentBalance, _deposit.DepositOffer.Currency, DateTime.Today)
                - _deposit.CalculationData.Traffic.Where(t => t.TransactionType == DepositTransactionTypes.явнес).Sum(t => t.AmountInUsd)
                + _deposit.CalculationData.Traffic.Where(t => t.TransactionType == DepositTransactionTypes.–асход).Sum(t => t.AmountInUsd);
        }

        private void DefineCurrentState()
        {
            if (_deposit.CalculationData.CurrentBalance == 0)
                _deposit.CalculationData.State = DepositStates.«акрыт;
            else
                _deposit.CalculationData.State = _deposit.FinishDate < DateTime.Today ? DepositStates.ѕросрочен : DepositStates.ќткрыт;
        }

        private void FillinDailyBalances()
        {
            var period = new Period(_deposit.StartDate, _deposit.FinishDate);
            _deposit.CalculationData.DailyTable = new List<DepositDailyLine>();
            decimal balance = 0;

            foreach (DateTime day in period)
            {
                balance += _deposit.CalculationData.Traffic.Where(t => t.Timestamp.Date == day.Date).Sum(t => t.Amount * t.Destination());
                _deposit.CalculationData.DailyTable.Add(new DepositDailyLine { Date = day, Balance = balance });
            }
        }


        /// <summary>
        /// http://msdn.microsoft.com/ru-ru/library/bb311040.aspx
        /// http://smehrozalam.wordpress.com/2009/06/10/c-left-outer-joins-with-linq/
        /// </summary>
        private void DefineCurrencyRates()
        {
            LeftOuterJoin();
        }

        private void LeftOuterJoin()
        {
            /* вынесение курсов нужной валюты в промежуточный список 
             * и затем left outer join по дате
             * оказалось самым быстрым вариантом - в 3 раза быстрее,
             * чем получать курсы дл€ каждого дн€ в foreach
             * и в 2 раза быстее left outer join с двум€ where по дате и валюте
             * или where двойным условием (закоменчено ниже)
             * 
             * при этом foreach позвол€ет гибко установить курс предыдущего дн€,
             * если в базе нет курса дл€ определенного дн€, в то врем€ как
             * left outer join позвол€ет только подставить какое-либо значение по умолчанию
             *  это должно учитыватьс€ далее!

             * var temp = (from line in _deposit.CalculationData.DailyTable
             *       from rate in _db.CurrencyRates.Where(r => r.Currency == _deposit.DepositOffer.Currency)
             * .Where(rt => line.Date == rt.BankDay).DefaultIfEmpty()
             * 
             * var temp = (from line in _deposit.CalculationData.DailyTable
             *      from rate in _db.CurrencyRates.Where(r => r.Currency == _deposit.DepositOffer.Currency && r.BankDay == line.Date).DefaultIfEmpty()
            */

            var oneCurrencyRates =
                _db.CurrencyRates.Where(r => r.Currency == _deposit.DepositOffer.Currency).ToList();

            var temp = (from line in _deposit.CalculationData.DailyTable
                          from rate in oneCurrencyRates
                               .Where(rt => line.Date == rt.BankDay).DefaultIfEmpty()
                  select
                    new DepositDailyLine
                    {
                        Date = line.Date,
                        Balance = line.Balance,
                        CurrencyRate = rate != null ? (decimal) rate.Rate : 0
                    }
                );
            _deposit.CalculationData.DailyTable = temp.ToList();
        }

        private void InnerJoin()
        {
            // inner join - если в одно из таблиц нет строки с ключем , 
            // то и из второй таблицы данные не попадают в объединение
            var temp =
                from line in _deposit.CalculationData.DailyTable
                join rate in _db.CurrencyRates.Where(r => r.Currency == _deposit.DepositOffer.Currency)
                    on line.Date equals rate.BankDay
                select new DepositDailyLine {Date = line.Date, Balance = line.Balance, CurrencyRate = (decimal) rate.Rate};
            _deposit.CalculationData.DailyTable = temp.ToList();
        }
    }
}