using System;
using System.Collections.Generic;
using System.Linq;

namespace Keeper2018
{
    public class MonthAnalyser
    {
        private readonly KeeperDb _db;
        private MonthAnalysisModel _monthAnalysisModel;
        private AccountModel _myAccountsRoot;
        private AccountModel _incomesRoot;


        public MonthAnalyser(KeeperDb db)
        {
            _db = db;
        }

        public void Initialize()
        {
            _myAccountsRoot = _db.AccountsTree.First(a => a.Name == "Мои");
            _incomesRoot = _db.AccountsTree.First(a => a.Name == "Все доходы");
        }

        public MonthAnalysisModel Produce(DateTime startDate)
        {
            var isCurrentPeriod = DateTime.Today.Year == startDate.Date.Year &&
                                  DateTime.Today.Month == startDate.Date.Month;

            _monthAnalysisModel = new MonthAnalysisModel()
            {
                StartDate = startDate,
                MonthAnalysisViewCaption = startDate.ToString("MMMM yyyy") + (isCurrentPeriod ? " - текущий период!" : ""),
            };

            FillBeforeList(startDate);
            var finishMoment = isCurrentPeriod ? DateTime.Today.GetEndOfDate() : startDate.AddMonths(1).AddSeconds(-1);
            FillIncomeList(startDate, finishMoment);
            FillAfterList(finishMoment);

            return _monthAnalysisModel;
        }

        private void FillBeforeList(DateTime startDate)
        {
            var trafficCalculator = new TrafficOfBranchCalculator(_db, _myAccountsRoot,
                                        new Period(new DateTime(2001, 12, 31), startDate.AddSeconds(-1)));
            trafficCalculator.Evaluate();
            _monthAnalysisModel.BeforeList.Add("Входящий остаток на начало месяца");
            foreach (var line in trafficCalculator.ReportForMonthAnalysis())
                _monthAnalysisModel.BeforeList.Add(line);
        }

        private void FillIncomeList(DateTime startDate, DateTime finishMoment)
        {
            _monthAnalysisModel.IncomeList.Add("Доходы");
            _monthAnalysisModel.IncomeList.Add("");

            var depoList = new List<string>();
            decimal total = 0;
            decimal depoTotal = 0;
            foreach (var tran in _db.TransactionModels.Where(t => t.Operation == OperationType.Доход
                                                                  && t.Timestamp >= startDate && t.Timestamp <= finishMoment))
            {
                var amStr = _db.AmountInUsdString(tran.Timestamp, tran.Currency, tran.Amount, out decimal amountInUsd);
                if (tran.MyAccount.IsDeposit)
                {
                    depoList.Add($"{amStr}  {tran.Comment} {tran.Timestamp:dd MMM}");
                    depoTotal = depoTotal + amountInUsd;
                }
                else
                {
                    var comment = string.IsNullOrEmpty(tran.Comment) ? tran.Tags.First(t=>t.Is(_incomesRoot)).Name : tran.Comment;
                    _monthAnalysisModel.IncomeList.Add($"{amStr}  {comment} {tran.Timestamp:dd MMM}");
                }

                total = total + amountInUsd;
            }

            _monthAnalysisModel.IncomeList.Add("");
            _monthAnalysisModel.IncomeList.Add("   Депозиты");
            foreach (var line in depoList)
            {
                _monthAnalysisModel.IncomeList.Add($"   {line}");
            }
            _monthAnalysisModel.IncomeList.Add($"   Итого депозиты {depoTotal:0.00} usd");


            _monthAnalysisModel.IncomeList.Add("");
            _monthAnalysisModel.IncomeList.Add($"Итого {total:0.00} usd");
        }

        private void FillAfterList(DateTime finishMoment)
        {
            var trafficCalculator = new TrafficOfBranchCalculator(_db, _myAccountsRoot,
                                        new Period(new DateTime(2001, 12, 31), finishMoment));
            trafficCalculator.Evaluate();
            _monthAnalysisModel.AfterList.Add("Исходящий остаток на конец месяца");
            foreach (var line in trafficCalculator.ReportForMonthAnalysis())
                _monthAnalysisModel.AfterList.Add(line);
        }
    }
}