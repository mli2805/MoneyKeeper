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
        public static IIncomeForPeriod SortMonthIncome(this KeeperDataModel dataModel, IEnumerable<TransactionModel> incomeTrans)
        {
            var result = new MonthIncome();

            foreach (var tran in incomeTrans)
            {
                var amStr = dataModel.AmountInUsdString(tran.Timestamp, tran.Currency, tran.Amount, out decimal amountInUsd, false);

                if (tran.Category.Is(186) || tran.Category.Is(212)) // зарплата (+иррациональные)
                {
                    result.Add(IncomeCategories.Зарплата,
                        $"{amStr} {BuildCommentForIncomeTransaction(tran, true)} {tran.Timestamp:dd MMM}", amountInUsd);
                }
                else if (tran.Category.Id == 208 || tran.Category.Id == 209) // %% по вкладу (по карточкам тоже) или дивиденды (траст)
                {
                    var depo = tran.MyAccount.ShortName ?? tran.MyAccount.Name;
                    result.Add(IncomeCategories.Депозиты, $"{amStr} {depo} {tran.Comment} {tran.Timestamp:dd MMM}", amountInUsd);
                }
                else if (tran.Category.Id == 701) // manyback
                {
                    result.Add(IncomeCategories.Манибэк, 
                        $"{amStr} {tran.MyAccount.ShortName} {tran.Comment} {tran.Timestamp:dd MMM}", amountInUsd);
                }
                else  // остальные типы доходов
                {
                    result.Add(IncomeCategories.Прочее, 
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
                var comment = tran.Counterparty.Name;

                if (!isSalary)
                    comment += ";  " + tran.Category.Name;

                if (!string.IsNullOrEmpty(tran.Comment))
                    comment += ";  " + tran.Comment;

                return comment;
            }
            catch (Exception e)
            {
                LogHelper.AppendLine(e, "IncomeMonthAnalyzer::BuildCommentForIncomeTransaction");
                throw;
            }
        }
    }
}
