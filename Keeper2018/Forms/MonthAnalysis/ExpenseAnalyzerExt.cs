using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using KeeperDomain;

namespace Keeper2018
{
    public static class ExpenseAnalyzerExt
    {
        public static Tuple<ListOfLines, decimal, decimal> CollectExpenseList(this KeeperDataModel dataModel,
            DateTime startDate, DateTime finishMoment, bool isYearAnalysisMode, decimal totalIncome)
        {
            var list = new ListOfLines(55);
            list.Add("Расходы", FontWeights.Bold, Brushes.Red);

            SortTransactions(dataModel, startDate, finishMoment, isYearAnalysisMode,
                out var byCategories, out var largeExpenses);

            list.AddRange(EvaluatePercents(byCategories, totalIncome, isYearAnalysisMode));
            list.AddRange(AddLargeExpenseLines(largeExpenses));

            list.Add("");
            decimal total = byCategories.Values.Sum();
            list.Add($"Итого {total:#,0.00} usd", FontWeights.Bold, Brushes.Red);

            var largeTotal = largeExpenses.Sum(a => a.Item2);
            return new Tuple<ListOfLines, decimal, decimal>(list, total, largeTotal);
        }

        private static void SortTransactions(KeeperDataModel dataModel,
            DateTime startDate, DateTime finishMoment, bool isYearAnalysisMode,
            out Dictionary<AccountItemModel, decimal> byCategories,
            out List<Tuple<AccountItemModel, decimal, TransactionModel>> largeExpenses)
        {
            var expenseTrans = dataModel.Transactions.Values
                .Where(t => t.Operation == OperationType.Расход &&
                            t.Timestamp >= startDate && t.Timestamp <= finishMoment);

            byCategories = new Dictionary<AccountItemModel, decimal>();
            largeExpenses = new List<Tuple<AccountItemModel, decimal, TransactionModel>>();

            foreach (var tran in expenseTrans)
            {
                foreach (var tag in tran.Tags)
                {
                    var expenseArticle = (AccountItemModel)tag.IsC(dataModel.ExpenseRoot());
                    if (expenseArticle == null) continue;

                    var amountInUsd = dataModel.AmountInUsd(tran.Timestamp, tran.Currency, tran.Amount);
                    if (byCategories.ContainsKey(expenseArticle))
                        byCategories[expenseArticle] += amountInUsd;
                    else byCategories.Add(expenseArticle, amountInUsd);

                    if (Math.Abs(amountInUsd) >= (isYearAnalysisMode ? 800 : 70))
                        largeExpenses.Add(new Tuple<AccountItemModel, decimal, TransactionModel>(expenseArticle, amountInUsd, tran));
                }
            }
        }

        private static IEnumerable<ListLine> EvaluatePercents(
            Dictionary<AccountItemModel, decimal> byCategories, decimal totalIncome, bool isYearAnalysisMode)
        {
            if (byCategories.Count == 0) yield break;
            yield return new ListLine("");
            foreach (var pair in byCategories.OrderByDescending(p => p.Value))
            {
                var fromIncomeStr = pair.Value.ToPercent(totalIncome);

                var amount = $"{pair.Value:#,0.00} usd".PadLeft(isYearAnalysisMode ? 14 : 12);
                yield return new ListLine($"{amount} {fromIncomeStr} - {pair.Key.Name}",
                    Brushes.Red, 12, 19);
            }

            var line = new ListLine("     %% вычисляется от всех доходов за период", FontWeights.Normal, Brushes.Black, 11);
            line.FontFamily = new FontFamily("Arial");
            yield return line;
        }

        private static string ToPercent(this decimal value, decimal total)
        {
            var percent = Math.Round(value / total * 100);
            var str = percent.ToString(CultureInfo.InvariantCulture).PadLeft(2);
            return percent > 0 ? $"({str}%)" : "     ";
        }

        private static IEnumerable<ListLine> AddLargeExpenseLines(List<Tuple<AccountItemModel, decimal, TransactionModel>> lE)
        {
            if (lE.Count <= 0) yield break;

            yield return new ListLine("");
            yield return new ListLine("   В том числе крупные:", Brushes.Red);
            yield return new ListLine("");
            foreach (var line in lE.SelectMany(tuple => tuple.Item3.ToListLines(tuple.Item2)))
            {
                yield return line;
            }
            yield return new ListLine("");
            var largeTotal = lE.Sum(a => a.Item2);
            yield return new ListLine($"   Итого крупные {largeTotal:#,0.00} usd", FontWeights.SemiBold, Brushes.Red, 12, 19);
        }

        private static IEnumerable<ListLine> ToListLines(this TransactionModel tran, decimal amountInUsd)
        {
            var sum = $"{tran.Amount:#,0.00} {tran.Currency.ToString().ToLower()}";
            var line = tran.Currency == CurrencyCode.USD
                ? $"   {sum} {tran.Timestamp:dd MMM}"
                : $"   {sum} ({amountInUsd:#,0.00} usd)  {tran.Timestamp:dd MMM}";

            if (tran.Comment.Length > 0)
                line += "; " + tran.Comment;
            foreach (var str in line.Wrap(50))
            {
                yield return new ListLine(str, Brushes.Red, 12, 19);
            }
        }

        private static IEnumerable<string> Wrap(this string source, int limit)
        {
            var parts = source.Split(' ');
            var line = 1;
            var res = "";
            foreach (var part in parts)
            {
                if (res.Length + part.Length + 1 > limit)
                {
                    yield return line == 1 ? res.TrimEnd() : new string(' ',5) + res.TrimEnd();
                    line++;
                    res = "";
                }

                res += part + " ";

            }
            if (res != "")
                yield return  line == 1 ? res.TrimEnd() : new string(' ',5) + res.TrimEnd();
        }
    }
}
