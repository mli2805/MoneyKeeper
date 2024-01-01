using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using KeeperDomain;

namespace Keeper2018
{
    public static class IncomeMonthAnalyzer
    {
        // временно, чарт депозиты-вал разницы получаетс данные не сам, а через анализ месяца
        public static MonthIncome SortMonthIncome(this KeeperDataModel dataModel, DateTime startDate, DateTime finishMoment)
        {
            var incomeTrans = dataModel.Transactions.Values
                .Where(t => t.Operation == OperationType.Доход
                            && t.Timestamp >= startDate && t.Timestamp <= finishMoment);

            return (MonthIncome)dataModel.SortMonthIncome(incomeTrans);
        }

        public static IIncomeForPeriod SortMonthIncome(this KeeperDataModel dataModel, IEnumerable<TransactionModel> incomeTrans)
        {
            var result = new MonthIncome();

            foreach (var tran in incomeTrans)
            {
                var amStr = dataModel.AmountInUsdString(tran.Timestamp, tran.Currency, tran.Amount, out decimal amountInUsd, false);
                var tag = tran.Tags.First(t => t.Is(185)); // один тэг тип доходов, другой контрагент

                if (tag.Is(186) || tag.Is(212)) // зарплата (+иррациональные)
                {
                    result.Add(IncomeCategories.зарплата,
                        $"{amStr} {BuildCommentForIncomeTransaction(tran, true)} {tran.Timestamp:dd MMM}", amountInUsd);
                }
                else if (tag.Id == 208 || tag.Id == 209) // %% по вкладу (по карточкам тоже) или дивиденды (траст)
                {
                    var depo = tran.MyAccount.ShortName ?? tran.MyAccount.Name;
                    result.Add(IncomeCategories.депозиты, $"{amStr} {depo} {tran.Comment} {tran.Timestamp:dd MMM}", amountInUsd);
                }
                else if (tag.Id == 701) // manyback
                {
                    result.Add(IncomeCategories.манибэк, 
                        $"{amStr} {tran.MyAccount.ShortName} {tran.Comment} {tran.Timestamp:dd MMM}", amountInUsd);
                }
                else  // остальные типы доходов
                {
                    result.Add(IncomeCategories.прочее, 
                        $"{amStr} {BuildCommentForIncomeTransaction(tran, false)} {tran.Timestamp:dd MMM}", amountInUsd);
                }
            }

            return result;
        }

        public static void InsertLinesIntoIncomeList(this ListOfLines list, List<Tuple<string, decimal>> lines, string word)
        {
            if (lines.Count == 0) return;
            list.Add("");
            foreach (var line in lines)
            {
                list.Add($" {line.Item1}", Brushes.Blue);
            }

            list.Add($"   Итого {word} {lines.Sum(c => c.Item2):#,0.00} usd", FontWeights.Bold, Brushes.Blue);
        }

        private static string BuildCommentForIncomeTransaction(TransactionModel tran, bool isSalary)
        {
            try
            {
                var comment = tran.Tags.First(t => t.Is(157)).Name;

                if (!isSalary)
                    comment += ";  " + tran.Tags.First(t => t.Is(NickNames.IncomeTags)).Name;

                if (!string.IsNullOrEmpty(tran.Comment))
                    comment += ";  " + tran.Comment;

                return comment;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
