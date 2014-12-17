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

        [ImportingConstructor]
        public DepositTrafficEvaluator(KeeperDb db, RateExtractor rateExtractor)
        {
            _db = db;
            _rateExtractor = rateExtractor;
        }

        public void EvaluateTraffic(Deposit deposit)
        {
            SummarizeTraffic(deposit);
            DefineCurrentState(deposit);
            FillinDailyBalances(deposit);
            if (deposit.DepositOffer.Currency != CurrencyCodes.USD) DefineCurrencyRates(deposit);

        }

        private void SummarizeTraffic(Deposit deposit)
        {
            deposit.CalculationData.TotalMyIns = deposit.CalculationData.Traffic.Where(t => t.TransactionType == DepositTransactionTypes.явнес).Sum(t => t.Amount);

            deposit.CalculationData.TotalMyOuts = 
                deposit.CalculationData.Traffic.Where(t => t.TransactionType == DepositTransactionTypes.–асход).Sum(t => t.Amount);
            deposit.CalculationData.TotalMyOutsInUsd = 
                deposit.CalculationData.Traffic.Where(t => t.TransactionType == DepositTransactionTypes.–асход).Sum(t => t.AmountInUsd);

            deposit.CalculationData.TotalPercent = deposit.CalculationData.Traffic.Where(t => t.TransactionType == DepositTransactionTypes.ѕроценты).Sum(t => t.Amount);
            deposit.CalculationData.TotalPercentInUsd = deposit.CalculationData.Traffic.Where(t => t.TransactionType == DepositTransactionTypes.ѕроценты).Sum(t => t.AmountInUsd);

            deposit.CalculationData.CurrentProfitInUsd =
                _rateExtractor.GetUsdEquivalent(deposit.CalculationData.CurrentBalance, deposit.DepositOffer.Currency, DateTime.Today)
                - deposit.CalculationData.Traffic.Where(t => t.TransactionType == DepositTransactionTypes.явнес).Sum(t => t.AmountInUsd)
                + deposit.CalculationData.Traffic.Where(t => t.TransactionType == DepositTransactionTypes.–асход).Sum(t => t.AmountInUsd);
        }

        private void DefineCurrentState(Deposit deposit)
        {
            if (deposit.CalculationData.CurrentBalance == 0)
                deposit.CalculationData.State = DepositStates.«акрыт;
            else
                deposit.CalculationData.State = deposit.FinishDate < DateTime.Today ? DepositStates.ѕросрочен : DepositStates.ќткрыт;
        }

        private void FillinDailyBalances(Deposit deposit)
        {
            var period = new Period(deposit.StartDate, deposit.FinishDate);
            deposit.CalculationData.DailyTable = new List<DepositDailyLine>();
            decimal balance = 0;

            foreach (DateTime day in period)
            {
                balance += deposit.CalculationData.Traffic.Where(t => t.Timestamp.Date == day.Date).Sum(t => t.Amount * t.Destination());
                deposit.CalculationData.DailyTable.Add(new DepositDailyLine { Date = day, Balance = balance });
            }
        }


        /// <summary>
        /// http://msdn.microsoft.com/ru-ru/library/bb311040.aspx
        /// http://smehrozalam.wordpress.com/2009/06/10/c-left-outer-joins-with-linq/
        /// </summary>
        private void DefineCurrencyRates(Deposit deposit)
        {
            LeftOuterJoin(deposit);
        }

        private void LeftOuterJoin(Deposit deposit)
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

             * var temp = (from line in deposit.CalculationData.DailyTable
             *       from rate in _db.CurrencyRates.Where(r => r.Currency == deposit.DepositOffer.Currency)
             * .Where(rt => line.Date == rt.BankDay).DefaultIfEmpty()
             * 
             * var temp = (from line in deposit.CalculationData.DailyTable
             *      from rate in _db.CurrencyRates.Where(r => r.Currency == deposit.DepositOffer.Currency && r.BankDay == line.Date).DefaultIfEmpty()
            */

            var oneCurrencyRates =
                _db.CurrencyRates.Where(r => r.Currency == deposit.DepositOffer.Currency).ToList();

            var temp = (from line in deposit.CalculationData.DailyTable
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
            deposit.CalculationData.DailyTable = temp.ToList();
        }

        private void InnerJoin(Deposit deposit)
        {
            // inner join - если в одно из таблиц нет строки с ключем , 
            // то и из второй таблицы данные не попадают в объединение
            var temp =
                from line in deposit.CalculationData.DailyTable
                join rate in _db.CurrencyRates.Where(r => r.Currency == deposit.DepositOffer.Currency)
                    on line.Date equals rate.BankDay
                select new DepositDailyLine {Date = line.Date, Balance = line.Balance, CurrencyRate = (decimal) rate.Rate};
            deposit.CalculationData.DailyTable = temp.ToList();
        }
    }
}