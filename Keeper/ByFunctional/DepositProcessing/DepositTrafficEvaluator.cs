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
                balance += _deposit.CalculationData.Traffic.Where(t => t.Timestamp.Date == day.Date).Sum(t => t.Amount * t.Destination());
                _deposit.CalculationData.DailyTable.Add(new DepositDailyLine{ Date = day, Balance = balance});
            }
        }


        /// <summary>
        /// http://msdn.microsoft.com/ru-ru/library/bb311040.aspx
        /// http://stackoverflow.com/questions/3404975/left-outer-join-in-linq
        /// </summary>
        private void DefineCurrencyRates()
        {
            // inner join - если в одно из таблиц нет строки с ключем , то и из второй таблицы данные не попадают в объединение
            var temp =
                from line in _deposit.CalculationData.DailyTable
                join rate in _db.CurrencyRates.Where(r => r.Currency == _deposit.DepositOffer.Currency)
                    on line.Date equals rate.BankDay 
                select new DepositDailyLine { Date = line.Date, Balance = line.Balance, CurrencyRate = (decimal)rate.Rate };
           _deposit.CalculationData.DailyTable = temp.ToList();

            // нужен outer join
/*
      для анонимных классов join ругается не может вывести тип
      а для именованных получается пустой результат
      предположительно сравнивает экземпляры класса по указателям, а не по содержимому
      не понятно как переопределять equals
      on new CurrencyPair(_deposit.DepositOffer.Currency, line.Date) equals new CurrencyPair(rate.Currency, rate.BankDay)
*/

            /*
             */
        }
    }
}