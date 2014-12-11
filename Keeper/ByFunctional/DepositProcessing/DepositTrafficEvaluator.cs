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
            _deposit.CalculationData.TotalMyIns = _deposit.CalculationData.Traffic.Where(t => t.TransactionType == DepositTransactionTypes.�����).Sum(t => t.Amount);

            _deposit.CalculationData.TotalMyOuts = _deposit.CalculationData.Traffic.Where(t => t.TransactionType == DepositTransactionTypes.������).Sum(t => t.Amount);
            _deposit.CalculationData.TotalMyOutsInUsd = _deposit.CalculationData.Traffic.Where(t => t.TransactionType == DepositTransactionTypes.������).Sum(t => t.AmountInUsd);

            _deposit.CalculationData.TotalPercent = _deposit.CalculationData.Traffic.Where(t => t.TransactionType == DepositTransactionTypes.��������).Sum(t => t.Amount);
            _deposit.CalculationData.TotalPercentInUsd = _deposit.CalculationData.Traffic.Where(t => t.TransactionType == DepositTransactionTypes.��������).Sum(t => t.AmountInUsd);

            _deposit.CalculationData.CurrentProfitInUsd =
                _rateExtractor.GetUsdEquivalent(_deposit.CalculationData.CurrentBalance, _deposit.DepositOffer.Currency, DateTime.Today)
                - _deposit.CalculationData.Traffic.Where(t => t.TransactionType == DepositTransactionTypes.�����).Sum(t => t.AmountInUsd)
                + _deposit.CalculationData.Traffic.Where(t => t.TransactionType == DepositTransactionTypes.������).Sum(t => t.AmountInUsd);
        }

        private void DefineCurrentState()
        {
            if (_deposit.CalculationData.CurrentBalance == 0)
                _deposit.CalculationData.State = DepositStates.������;
            else
                _deposit.CalculationData.State = _deposit.FinishDate < DateTime.Today ? DepositStates.��������� : DepositStates.������;
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
            /* ��������� ������ ������ ������ � ������������� ������ 
             * � ����� left outer join �� ����
             * ��������� ����� ������� ��������� - � 3 ���� �������,
             * ��� �������� ����� ��� ������� ��� � foreach
             * � � 2 ���� ������ left outer join � ����� where �� ���� � ������
             * ��� where ������� �������� (����������� ����)
             * 
             * ��� ���� foreach ��������� ����� ���������� ���� ����������� ���,
             * ���� � ���� ��� ����� ��� ������������� ���, � �� ����� ���
             * left outer join ��������� ������ ���������� �����-���� �������� �� ���������
             *  ��� ������ ����������� �����!

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
            // inner join - ���� � ���� �� ������ ��� ������ � ������ , 
            // �� � �� ������ ������� ������ �� �������� � �����������
            var temp =
                from line in _deposit.CalculationData.DailyTable
                join rate in _db.CurrencyRates.Where(r => r.Currency == _deposit.DepositOffer.Currency)
                    on line.Date equals rate.BankDay
                select new DepositDailyLine {Date = line.Date, Balance = line.Balance, CurrencyRate = (decimal) rate.Rate};
            _deposit.CalculationData.DailyTable = temp.ToList();
        }
    }
}