using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using KeeperDomain;

namespace Keeper2018
{
    public class MonthAnalyser
    {
        private readonly KeeperDataModel _dataModel;
        private MonthAnalysisModel _monthAnalysisModel;
        private AccountModel _myAccountsRoot;
        private AccountModel _incomesRoot;
        private AccountModel _expensesRoot;

        public MonthAnalyser(KeeperDataModel dataModel)
        {
            _dataModel = dataModel;
        }

        public void Initialize()
        {
            _myAccountsRoot = _dataModel.AccountsTree.First(a => a.Name == "Мои");
            _incomesRoot = _dataModel.AccountsTree.First(a => a.Name == "Все доходы");
            _expensesRoot = _dataModel.AccountsTree.First(a => a.Name == "Все расходы");
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
            _monthAnalysisModel.RatesChanges = _dataModel.GetRatesDifference(startMoment, finishMoment);
            _monthAnalysisModel.FillResultList(isCurrentPeriod);
            if (isCurrentPeriod)
                _monthAnalysisModel.FillForecast(finishMoment, (decimal)_dataModel.GetRate(DateTime.Today, CurrencyCode.BYN).Value);

            return _monthAnalysisModel;
        }

        private void FillBeforeViewModel(DateTime startDate)
        {
            var trafficCalculator = new TrafficOfBranchCalculator(_dataModel, _myAccountsRoot,
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
            var rantierList = new List<string>();
            var depoList = new List<string>();
            var restList = new List<string>();
            decimal total = 0;
            decimal salaryTotal = 0;
            decimal rantierTotal = 0;
            decimal depoTotal = 0;
            decimal restTotal = 0;
            foreach (var tran in _dataModel.Transactions.Values
                         .Where(t => t.Operation == OperationType.Доход 
                                     && t.Timestamp >= startDate && t.Timestamp <= finishMoment))
            {
                var amStr = _dataModel.AmountInUsdString(tran.Timestamp, tran.Currency, tran.Amount, out decimal amountInUsd);

                foreach (var tag in tran.Tags)
                {
                    if (tag.Is(185))
                    {
                        if (tag.Is(186))
                        {
                            salaryList.Add($"{amStr} {BuildCommentForIncomeTransaction(tran)} {tran.Timestamp:dd MMM}");
                            salaryTotal = salaryTotal + amountInUsd;
                        }
                        else if (tag.Is(188))
                        {
                            if (tag.Id == 208)
                            {
                                depoList.Add($"{amStr} {tran.MyAccount.Deposit?.ShortName} {tran.Comment} {tran.Timestamp:dd MMM}");
                                depoTotal = depoTotal + amountInUsd;
                            }
                            else
                            {
                                rantierList.Add($"{amStr} {tran.MyAccount.Deposit?.ShortName} {tran.Comment} {tran.Timestamp:dd MMM}");
                                rantierTotal = rantierTotal + amountInUsd;
                            }
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
                InsertLinesIntoIncomeList(salaryList, salaryTotal, "зарплата");
            if (depoList.Count > 0)
                InsertLinesIntoIncomeList(depoList, depoTotal, "депозиты");
            if (rantierList.Count > 0)
                InsertLinesIntoIncomeList(rantierList, rantierTotal, "прочие с капитала");
            if (restList.Count > 0)
                InsertLinesIntoIncomeList(restList, restTotal, "прочее");

            _monthAnalysisModel.IncomeViewModel.List.Add("");
            _monthAnalysisModel.IncomeViewModel.List.Add($"Итого {total:#,0.00} usd", FontWeights.Bold, Brushes.Blue);
            _monthAnalysisModel.DepoIncome = depoTotal;
            _monthAnalysisModel.Income = total;
        }

        private void FillIncomeForecastList(DateTime startDate, DateTime finishMoment)
        {
            var realIncomes = _dataModel.Transactions.Values.Where(t => t.Operation == OperationType.Доход
                                              && t.Timestamp >= startDate && t.Timestamp <= finishMoment).ToList();
            var salaryAccountId = 204;
            var iitAccountId = 443;
            var optixsoftAccountId = 172;
            if (!realIncomes.Any(t => t.Tags.Select(tt => tt.Id).Contains(salaryAccountId)
                && t.Tags.Select(tt => tt.Id).Contains(iitAccountId)))
            {
                var salaryInUsdValue = 800;
                _monthAnalysisModel.IncomeForecastList.Add($"зарплата ИИТ {salaryInUsdValue} usd");
                _monthAnalysisModel.IncomeForecast += salaryInUsdValue;
            }
            if (!realIncomes.Any(t => t.Tags.Select(tt => tt.Id).Contains(salaryAccountId)
                           && t.Tags.Select(tt => tt.Id).Contains(optixsoftAccountId)))
            {
                var salaryInUsdValue = 1200;
                _monthAnalysisModel.IncomeForecastList.Add($"зарплата OptixSoft {salaryInUsdValue} usd");
                _monthAnalysisModel.IncomeForecast += salaryInUsdValue;
            }

            var depoMainFolder = _dataModel.AcMoDict[166];
            foreach (var depo in depoMainFolder.Children.Where(c => c.IsDeposit))
                    ForeseeDepoIncome(depo);
        }

        private void ForeseeDepoIncome(AccountModel depo)
        {
            var depoMainCurrency = _dataModel.DepositOffers
                .First(o => o.Id == depo.Deposit.DepositOfferId).MainCurrency;
            var currency = depoMainCurrency == CurrencyCode.BYR ? CurrencyCode.BYN : depoMainCurrency;

            var revenues = depo.GetRevenuesInThisMonth(_dataModel);
            foreach (var tuple in revenues)
            {
                _monthAnalysisModel.IncomeForecastList.
                    Add($"{depo.Deposit.ShortName}  {tuple.Item2:#,0.00} {currency.ToString().ToLower()} {tuple.Item1:dd MMM}");
                _monthAnalysisModel.IncomeForecast += currency == CurrencyCode.USD
                    ? tuple.Item2
                    : _dataModel.AmountInUsd(DateTime.Today, depoMainCurrency, tuple.Item2);
            }
           
        }

        private void InsertLinesIntoIncomeList(List<string> lines, decimal total, string word)
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

        private string BuildCommentForIncomeTransaction(TransactionModel tran)
        {
            var comment = "";
            if (!string.IsNullOrEmpty(tran.Comment))
                comment = tran.Comment;
            else
            {
                foreach (var tag in tran.Tags)
                {
                    if (tag.Is(_incomesRoot))
                    {
                        comment = tag.Name;
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
            foreach (var tran in _dataModel.Transactions.Values.Where(t => t.Operation == OperationType.Расход
                                               && t.Timestamp >= startDate && t.Timestamp <= finishMoment))
            {
                foreach (var tag in tran.Tags)
                {
                    var expenseArticle = tag.IsC(_expensesRoot);
                    if (expenseArticle == null) continue;
                    var amountInUsd = _dataModel.AmountInUsd(tran.Timestamp, tran.Currency, tran.Amount);
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
            var trafficCalculator = new TrafficOfBranchCalculator(_dataModel, _myAccountsRoot,
                                        new Period(new DateTime(2001, 12, 31), finishMoment));
            trafficCalculator.EvaluateAccount();
            var text = _monthAnalysisModel.IsCurrentPeriod ? "сегодня" : "конец месяца";
            _monthAnalysisModel.AfterViewModel.List.Add($"Исходящий остаток на {text}", FontWeights.Bold);
            _monthAnalysisModel.AfterViewModel.List.AddList(trafficCalculator.ReportForMonthAnalysis());
            _monthAnalysisModel.After = trafficCalculator.TotalAmount;
        }
    }
}