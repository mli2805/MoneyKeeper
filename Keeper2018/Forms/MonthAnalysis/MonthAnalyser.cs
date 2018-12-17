using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Keeper2018
{
    public class MonthAnalyser
    {
        private readonly KeeperDb _db;
        private MonthAnalysisModel _monthAnalysisModel;
        private AccountModel _myAccountsRoot;
        private AccountModel _incomesRoot;
        private AccountModel _expensesRoot;

        public MonthAnalyser(KeeperDb db)
        {
            _db = db;
        }

        public void Initialize()
        {
            _myAccountsRoot = _db.AccountsTree.First(a => a.Name == "Мои");
            _incomesRoot = _db.AccountsTree.First(a => a.Name == "Все доходы");
            _expensesRoot = _db.AccountsTree.First(a => a.Name == "Все расходы");
        }

        public MonthAnalysisModel Produce(DateTime startDate)
        {
            var isCurrentPeriod = DateTime.Today.Year == startDate.Date.Year &&
                                  DateTime.Today.Month == startDate.Date.Month;

            _monthAnalysisModel = new MonthAnalysisModel()
            {
                StartDate = startDate,
                MonthAnalysisViewCaption = startDate.ToString("MMMM yyyy") + (isCurrentPeriod ? " - текущий период!" : ""),

                ForecastListVisibility = isCurrentPeriod ? Visibility.Visible : Visibility.Collapsed,
            };

            FillBeforeViewModel(startDate);
            var finishMoment = isCurrentPeriod ? DateTime.Today.GetEndOfDate() : startDate.AddMonths(1).AddSeconds(-1);
            FillIncomeList(startDate, finishMoment);
            FillExpenseList(startDate, finishMoment);
            FillAfterList(finishMoment);
            _monthAnalysisModel.FillResultList();
            if (isCurrentPeriod)
                _monthAnalysisModel.FillForecast(finishMoment, (decimal)_db.GetRate(DateTime.Today, CurrencyCode.BYN).Value);
            _monthAnalysisModel.RatesChanges = _db.GetRatesMonthDifference(startDate, finishMoment);

            return _monthAnalysisModel;
        }

        private void FillBeforeViewModel(DateTime startDate)
        {
            var trafficCalculator = new TrafficOfBranchCalculator(_db, _myAccountsRoot,
                                        new Period(new DateTime(2001, 12, 31), startDate.AddSeconds(-1)));
            trafficCalculator.Evaluate();
            _monthAnalysisModel.BeforeViewModel.List.Add("Входящий остаток на начало месяца", FontWeights.Bold);
            _monthAnalysisModel.BeforeViewModel.List.AddList(trafficCalculator.ReportForMonthAnalysis());
            _monthAnalysisModel.Before = trafficCalculator.TotalAmount;
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
                    var comment = string.IsNullOrEmpty(tran.Comment) ? tran.Tags.First(t => t.Is(_incomesRoot)).Name : tran.Comment;
                    _monthAnalysisModel.IncomeList.Add($"{amStr}  {tran.Timestamp:dd MMM} {comment}");
                }

                total = total + amountInUsd;
            }

            if (depoList.Count > 0)
            {
                _monthAnalysisModel.IncomeList.Add("");
                _monthAnalysisModel.IncomeList.Add("   Депозиты:");
                _monthAnalysisModel.IncomeList.Add("");
                foreach (var line in depoList)
                {
                    _monthAnalysisModel.IncomeList.Add($"   {line}");
                }
                _monthAnalysisModel.IncomeList.Add("");
                _monthAnalysisModel.IncomeList.Add($"   Итого депозиты {depoTotal:#,0.00} usd");
            }

            _monthAnalysisModel.IncomeList.Add("");
            _monthAnalysisModel.IncomeList.Add($"Итого {total:#,0.00} usd");
            _monthAnalysisModel.Income = total;
        }

        private void FillExpenseList(DateTime startDate, DateTime finishMoment)
        {
            _monthAnalysisModel.ExpenseList.Add("Расходы");
            _monthAnalysisModel.ExpenseList.Add("");

            var articles = new BalanceWithTurnoverOfBranch();
            var largeExpenses = new List<string>();
            decimal total = 0;
            decimal largeTotal = 0;
            foreach (var tran in _db.TransactionModels.Where(t => t.Operation == OperationType.Расход
                                               && t.Timestamp >= startDate && t.Timestamp <= finishMoment))
            {
                foreach (var accountModel in tran.Tags)
                {
                    var expenseArticle = accountModel.IsC(_expensesRoot);
                    if (expenseArticle == null) continue;
                    var amountInUsd = _db.AmountInUsd(tran.Timestamp, tran.Currency, tran.Amount);
                    articles.Add(expenseArticle, CurrencyCode.USD, amountInUsd);
                    total += amountInUsd;

                    if (Math.Abs(amountInUsd) <= 70) continue;

                    var line = $"   {tran.Amount:#,0.00} {tran.Currency.ToString().ToLower()} ( {amountInUsd:#,0.00} usd )  {tran.Timestamp:dd MMM}";
                    if (tran.Comment.Length > 30)
                    {
                        largeExpenses.Add(line);
                        var comment = tran.Comment.Length > 70 ? tran.Comment.Substring(0, 70) : tran.Comment;
                        largeExpenses.Add($"        {comment}");
                    }
                    else
                        largeExpenses.Add($"{line}  {tran.Comment}");

                    largeTotal += amountInUsd;
                }
            }

            foreach (var pair in articles.ChildAccounts)
            {
                _monthAnalysisModel.ExpenseList.Add($"{pair.Value.Currencies[CurrencyCode.USD].Plus:#,0.00} usd - {pair.Key.Name}");
            }

            if (largeExpenses.Count > 0)
            {
                _monthAnalysisModel.ExpenseList.Add("");
                _monthAnalysisModel.ExpenseList.Add("   В том числе крупные:");
                _monthAnalysisModel.ExpenseList.Add("");
                _monthAnalysisModel.ExpenseList.AddRange(largeExpenses);
                _monthAnalysisModel.ExpenseList.Add("");
                _monthAnalysisModel.ExpenseList.Add($"   Итого крупные {largeTotal:#,0.00} usd");
            }

            _monthAnalysisModel.ExpenseList.Add("");
            _monthAnalysisModel.ExpenseList.Add($"Итого {total:#,0.00} usd");
            _monthAnalysisModel.Expense = total;
            _monthAnalysisModel.LargeExpense = largeTotal;
        }

        private void FillAfterList(DateTime finishMoment)
        {
            var trafficCalculator = new TrafficOfBranchCalculator(_db, _myAccountsRoot,
                                        new Period(new DateTime(2001, 12, 31), finishMoment));
            trafficCalculator.Evaluate();
            _monthAnalysisModel.AfterViewModel.List.Add("Исходящий остаток на конец месяца", FontWeights.Bold);
            _monthAnalysisModel.AfterViewModel.List.AddList(trafficCalculator.ReportForMonthAnalysis());
            _monthAnalysisModel.After = trafficCalculator.TotalAmount;
        }


    }
}