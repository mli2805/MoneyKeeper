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
            if (isCurrentPeriod)
                FillIncomeForecastList(startDate, finishMoment);
            FillExpenseList(startDate, finishMoment);
            FillAfterList(finishMoment);
            var startMoment = startDate.AddDays(-1);
            _monthAnalysisModel.RatesChanges = _db.GetRatesDifference(startMoment, finishMoment);
            _monthAnalysisModel.FillResultList(isCurrentPeriod);
            if (isCurrentPeriod)
                _monthAnalysisModel.FillForecast(finishMoment, (decimal)_db.GetRate(DateTime.Today, CurrencyCode.BYN).Value);

            return _monthAnalysisModel;
        }

        private void FillBeforeViewModel(DateTime startDate)
        {
            var trafficCalculator = new TrafficOfBranchCalculator(_db, _myAccountsRoot,
                                        new Period(new DateTime(2001, 12, 31), startDate.AddSeconds(-1)));
            trafficCalculator.EvaluateAccount();
            _monthAnalysisModel.BeforeViewModel.List.Add("Входящий остаток на начало месяца", FontWeights.Bold);
            _monthAnalysisModel.BeforeViewModel.List.AddList(trafficCalculator.ReportForMonthAnalysis());
            _monthAnalysisModel.Before = trafficCalculator.TotalAmount;
        }

        private void FillIncomeList(DateTime startDate, DateTime finishMoment)
        {
            _monthAnalysisModel.IncomeViewModel.List.Add("Доходы", FontWeights.Bold, Brushes.Blue);

            var salaryList = new List<string>();
            var depoList = new List<string>();
            var restList = new List<string>();
            decimal total = 0;
            decimal salaryTotal = 0;
            decimal depoTotal = 0;
            decimal restTotal = 0;
            foreach (var tran in _db.Bin.Transactions.Values.Where(t => t.Operation == OperationType.Доход
                                                                  && t.Timestamp >= startDate && t.Timestamp <= finishMoment))
            {
                var amStr = _db.AmountInUsdString(tran.Timestamp, tran.Currency, tran.Amount, out decimal amountInUsd);

                foreach (var tagId in tran.Tags)
                {
                    var tagModel = _db.AcMoDict[tagId];
                    if (tagModel.Is(185))
                    {
                        if (tagModel.Is(186))
                        {
                            salaryList.Add($"{amStr} {BuildCommentForIncomeTransaction(tran)} {tran.Timestamp:dd MMM}");
                            salaryTotal = salaryTotal + amountInUsd;
                        }
                        else if (tagModel.Is(188))
                        {
                            var accModel = _db.AcMoDict[tran.MyAccount];
                            depoList.Add($"{amStr} {accModel.Deposit?.ShortName} {tran.Comment} {tran.Timestamp:dd MMM}");
                            depoTotal = depoTotal + amountInUsd;
                        }
                        else
                        {
                            restList.Add($"{amStr} {BuildCommentForIncomeTransaction(tran)} {tran.Timestamp:dd MMM}");
                            restTotal = restTotal + amountInUsd;
                        }
                    }
                }
                total = total + amountInUsd;
            }

            if (salaryList.Count > 0)
                InsertDepoLinesIntoIncomeList(salaryList, salaryTotal, "зарплата");
            if (depoList.Count > 0)
                InsertDepoLinesIntoIncomeList(depoList, depoTotal, "депозиты");
            if (restList.Count > 0)
                InsertDepoLinesIntoIncomeList(restList, restTotal, "прочее");

            _monthAnalysisModel.IncomeViewModel.List.Add("");
            _monthAnalysisModel.IncomeViewModel.List.Add($"Итого {total:#,0.00} usd", FontWeights.Bold, Brushes.Blue);
            _monthAnalysisModel.DepoIncome = depoTotal;
            _monthAnalysisModel.Income = total;
        }

        private void FillIncomeForecastList(DateTime startDate, DateTime finishMoment)
        {
            var realIncomes = _db.Bin.Transactions.Values.Where(t => t.Operation == OperationType.Доход
                                              && t.Timestamp >= startDate && t.Timestamp <= finishMoment).ToList();
            var salaryAccountId = 204;
            if (realIncomes.FirstOrDefault(t => t.Tags.Contains(salaryAccountId)) == null)
            {
                var salaryInUsdValue = 1500;
                _monthAnalysisModel.IncomeForecastList.Add($"зарплата {salaryInUsdValue} usd");
                _monthAnalysisModel.IncomeForecast += salaryInUsdValue;
            }

            var depos = _db.AcMoDict[166];
            foreach (var depo in depos.Children.Where(c => c.IsDeposit))
                if (realIncomes.FirstOrDefault(t => t.MyAccount == depo.Id && t.Tags.Contains(208)) == null)
                    ForeseeDepoIncome(depo);
        }

        private void ForeseeDepoIncome(AccountModel depo)
        {
            var depositOffer = _db.Bin.DepositOffers.First(o => o.Id == depo.Deposit.DepositOfferId);

            var revenue = depo.GetRevenueInThisMonth(_db);
            var currency = depositOffer.MainCurrency == CurrencyCode.BYR ? CurrencyCode.BYN : depositOffer.MainCurrency;
            _monthAnalysisModel.IncomeForecastList.
                Add($"%%:  {depo.Deposit.ShortName}  {revenue:#,0.00} {currency.ToString().ToLower()}");
            _monthAnalysisModel.IncomeForecast += depositOffer.MainCurrency == CurrencyCode.USD
                ? revenue
                : _db.AmountInUsd(DateTime.Today, depositOffer.MainCurrency, revenue);
        }

        private void InsertDepoLinesIntoIncomeList(List<string> lines, decimal total, string word)
        {
            _monthAnalysisModel.IncomeViewModel.List.Add("");
            _monthAnalysisModel.IncomeViewModel.List.Add($"   {word}:", Brushes.Blue);
            _monthAnalysisModel.IncomeViewModel.List.Add("");
            foreach (var line in lines)
            {
                _monthAnalysisModel.IncomeViewModel.List.Add($"   {line}", Brushes.Blue);
            }

            _monthAnalysisModel.IncomeViewModel.List.Add("");
            _monthAnalysisModel.IncomeViewModel.List.Add($"   Итого {word} {total:#,0.00} usd", Brushes.Blue);
        }

        private string BuildCommentForIncomeTransaction(Transaction tran)
        {
            var comment = "";
            if (!string.IsNullOrEmpty(tran.Comment))
                comment = tran.Comment;
            else
            {
                foreach (var tagId in tran.Tags)
                {
                    var tagModel = _db.AcMoDict[tagId];
                    if (tagModel.Is(_incomesRoot))
                    {
                        comment = tagModel.Name;
                        break;
                    }
                }
            }

            return comment;
        }

        private void FillExpenseList(DateTime startDate, DateTime finishMoment)
        {
            _monthAnalysisModel.ExpenseViewModel.List.Add("Расходы", FontWeights.Bold, Brushes.Red);
            _monthAnalysisModel.ExpenseViewModel.List.Add("");

            var articles = new BalanceWithTurnoverOfBranch();
            var largeExpenses = new ListOfLines();
            decimal total = 0;
            decimal largeTotal = 0;
            foreach (var tran in _db.Bin.Transactions.Values.Where(t => t.Operation == OperationType.Расход
                                               && t.Timestamp >= startDate && t.Timestamp <= finishMoment))
            {
                foreach (var tagId in tran.Tags)
                {
                    var accModel = _db.AcMoDict[tagId];
                    var expenseArticle = accModel.IsC(_expensesRoot);
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
            trafficCalculator.EvaluateAccount();
            var text = _monthAnalysisModel.IsCurrentPeriod ? "сегодня" : "конец месяца";
            _monthAnalysisModel.AfterViewModel.List.Add($"Исходящий остаток на {text}", FontWeights.Bold);
            _monthAnalysisModel.AfterViewModel.List.AddList(trafficCalculator.ReportForMonthAnalysis());
            _monthAnalysisModel.After = trafficCalculator.TotalAmount;
        }
    }
}