using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

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
                IsCurrentPeriod = isCurrentPeriod,
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
            _monthAnalysisModel.IncomeViewModel.List.Add("Доходы", FontWeights.Bold, Brushes.Blue);
            _monthAnalysisModel.IncomeViewModel.List.Add("", Brushes.Blue);

            var depoList = new List<string>();
            decimal total = 0;
            decimal depoTotal = 0;
            foreach (var tran in _db.TransactionModels.Where(t => t.Operation == OperationType.Доход
                                                                  && t.Timestamp >= startDate && t.Timestamp <= finishMoment))
            {
                var amStr = _db.AmountInUsdString(tran.Timestamp, tran.Currency, tran.Amount, out decimal amountInUsd);
                if (tran.MyAccount.IsDeposit)
                {
                    depoList.Add($"{amStr} {tran.MyAccount.Deposit.ShortName} {tran.Comment} {tran.Timestamp:dd MMM}");
                    depoTotal = depoTotal + amountInUsd;
                }
                else
                {
                    var comment = string.IsNullOrEmpty(tran.Comment) ? tran.Tags.First(t => t.Is(_incomesRoot)).Name : tran.Comment;
                    _monthAnalysisModel.IncomeViewModel.List.Add($"{amStr}  {tran.Timestamp:dd MMM} {comment}", Brushes.Blue);
                }

                total = total + amountInUsd;
            }

            if (depoList.Count > 0)
            {
                _monthAnalysisModel.IncomeViewModel.List.Add("");
                _monthAnalysisModel.IncomeViewModel.List.Add("   Депозиты:", Brushes.Blue);
                _monthAnalysisModel.IncomeViewModel.List.Add("");
                foreach (var line in depoList)
                {
                    _monthAnalysisModel.IncomeViewModel.List.Add($"   {line}", Brushes.Blue);
                }
                _monthAnalysisModel.IncomeViewModel.List.Add("");
                _monthAnalysisModel.IncomeViewModel.List.Add($"   Итого депозиты {depoTotal:#,0.00} usd", Brushes.Blue);
            }

            _monthAnalysisModel.IncomeViewModel.List.Add("");
            _monthAnalysisModel.IncomeViewModel.List.Add($"Итого {total:#,0.00} usd", FontWeights.Bold, Brushes.Blue);
            _monthAnalysisModel.Income = total;
        }

        private void FillExpenseList(DateTime startDate, DateTime finishMoment)
        {
            _monthAnalysisModel.ExpenseViewModel.List.Add("Расходы", FontWeights.Bold, Brushes.Red);
            _monthAnalysisModel.ExpenseViewModel.List.Add("");

            var articles = new BalanceWithTurnoverOfBranch();
            var largeExpenses = new ListOfLines();
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
                        largeExpenses.Add(line, Brushes.Red);
                        var comment = tran.Comment.Length > 70 ? tran.Comment.Substring(0, 70) : tran.Comment;
                        largeExpenses.Add($"        {comment}", Brushes.Red);
                    }
                    else
                        largeExpenses.Add($"{line}  {tran.Comment}", Brushes.Red);

                    largeTotal += amountInUsd;
                }
            }

            foreach (var pair in articles.ChildAccounts)
            {
                _monthAnalysisModel.ExpenseViewModel.List.Add($"{pair.Value.Currencies[CurrencyCode.USD].Plus:#,0.00} usd - {pair.Key.Name}", Brushes.Red);
            }

            if (largeExpenses.Lines.Count > 0)
            {
                _monthAnalysisModel.ExpenseViewModel.List.Add("");
                _monthAnalysisModel.ExpenseViewModel.List.Add("   В том числе крупные:", Brushes.Red);
                _monthAnalysisModel.ExpenseViewModel.List.Add("");
                _monthAnalysisModel.ExpenseViewModel.List.AddList(largeExpenses);
                _monthAnalysisModel.ExpenseViewModel.List.Add("");
                _monthAnalysisModel.ExpenseViewModel.List.Add($"   Итого крупные {largeTotal:#,0.00} usd", FontWeights.SemiBold, Brushes.Red);
            }

            _monthAnalysisModel.ExpenseViewModel.List.Add("");
            _monthAnalysisModel.ExpenseViewModel.List.Add($"Итого {total:#,0.00} usd", FontWeights.Bold, Brushes.Red);
            _monthAnalysisModel.Expense = total;
            _monthAnalysisModel.LargeExpense = largeTotal;
        }

        private void FillAfterList(DateTime finishMoment)
        {
            var trafficCalculator = new TrafficOfBranchCalculator(_db, _myAccountsRoot,
                                        new Period(new DateTime(2001, 12, 31), finishMoment));
            trafficCalculator.Evaluate();
            var text = _monthAnalysisModel.IsCurrentPeriod ? "сегодня" : "конец месяца";
            _monthAnalysisModel.AfterViewModel.List.Add($"Исходящий остаток на {text}", FontWeights.Bold);
            _monthAnalysisModel.AfterViewModel.List.AddList(trafficCalculator.ReportForMonthAnalysis());
            _monthAnalysisModel.After = trafficCalculator.TotalAmount;
        }
    }
}