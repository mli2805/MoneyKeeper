using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using KeeperDomain;

namespace Keeper2018
{
    public static class ExpanseAnalyzerExt
    {
        public static Tuple<ListOfLines, decimal, decimal> CollectExpenseList(this KeeperDataModel dataModel,
            DateTime startDate, DateTime finishMoment, bool isYearAnalysisMode)
        {
            var list = new ListOfLines(60);
            list.Add("Расходы", FontWeights.Bold, Brushes.Red);
            list.Add("");
            var expenseTrans = dataModel.Transactions.Values
                .Where(t => t.Operation == OperationType.Расход &&
                            t.Timestamp >= startDate && t.Timestamp <= finishMoment);

            var articles = new BalanceWithTurnoverOfBranch();
            var largeExpenses = new ListOfLines();
            decimal total = 0;
            decimal largeTotal = 0;

            foreach (var tran in expenseTrans)
            {
                foreach (var tag in tran.Tags)
                {
                    var expenseArticle = (AccountItemModel)tag.IsC(dataModel.ExpenseRoot());
                    if (expenseArticle == null) continue;
                    var amountInUsd = dataModel.AmountInUsd(tran.Timestamp, tran.Currency, tran.Amount);
                    articles.Add(expenseArticle, CurrencyCode.USD, amountInUsd);
                    total += amountInUsd;

                    if (Math.Abs(amountInUsd) < (isYearAnalysisMode ? 800 : 70)) continue;

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

            foreach (var pair in articles.ChildAccounts
                         .OrderByDescending(p=>p.Value.Currencies[CurrencyCode.USD].Plus))
            {
                var percent = Math.Round(pair.Value.Currencies[CurrencyCode.USD].Plus / total * 100);

                var percentStr = percent > 0 ?  $"({percent}%)" : "";
                list.Add($"{pair.Value.Currencies[CurrencyCode.USD].Plus:#,0.00} usd {percentStr} - {pair.Key.Name}", Brushes.Red);
            }

            if (largeExpenses.Lines.Count > 0)
            {
                list.Add("");
                list.Add("   В том числе крупные:", Brushes.Red);
                list.Add("");
                list.AddList(largeExpenses);
                list.Add("");
                list.Add($"   Итого крупные {largeTotal:#,0.00} usd", FontWeights.SemiBold, Brushes.Red);
            }

            list.Add("");
            list.Add($"Итого {total:#,0.00} usd", FontWeights.Bold, Brushes.Red);
            
            return new Tuple<ListOfLines, decimal, decimal>(list, total, largeTotal);
        }
    }
}
