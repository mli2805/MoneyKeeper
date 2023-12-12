using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using KeeperDomain;

namespace Keeper2018
{
    public static class IncomeAnalyzerExt
    {
        public static Tuple<ListOfLines, decimal, decimal> CollectIncomeList(this KeeperDataModel dataModel,
            DateTime startDate, DateTime finishMoment, bool isYearAnalysisMode)
        {
            var list = new ListOfLines(60);
            list.Add("Доходы", FontWeights.Bold, Brushes.Blue);

            var incomeTrans = dataModel.Transactions.Values
                .Where(t => t.Operation == OperationType.Доход
                            && t.Timestamp >= startDate && t.Timestamp <= finishMoment);

            var employers = new Dictionary<AccountItemModel, decimal>();
            var salaryList = new List<string>();

            var depoByCurrency = new Dictionary<CurrencyCode, decimal>();
            var depoList = new List<string>();

            var moneyBackList = new List<string>();
            var rantierList = new List<string>();
            var restList = new List<string>();
            decimal total = 0;
            decimal salaryTotal = 0;
            decimal moneyBackTotal = 0;
            decimal rantierTotal = 0;
            decimal depoTotal = 0;
            decimal restTotal = 0;

            foreach (var tran in incomeTrans)
            {
                var amStr = dataModel.AmountInUsdString(tran.Timestamp, tran.Currency, tran.Amount, out decimal amountInUsd);
                var tag = tran.Tags.First(t => t.Is(185)); // один тэг тип доходов, другой контрагент

                if (tag.Is(186)) // зарплата
                {
                    if (isYearAnalysisMode)
                    {
                        var employer = tran.Tags.First(t => t != tag);
                        if (employers.ContainsKey(employer))
                            employers[employer] += amountInUsd;
                        else employers.Add(employer, amountInUsd);
                    }
                    else
                        salaryList.Add($"{amStr} {BuildCommentForIncomeTransaction(tran, true)} {tran.Timestamp:dd MMM}");
                    salaryTotal += amountInUsd;
                }
                else if (tag.Id == 208 || tag.Id == 209) // %% по вкладу (по карточкам тоже) или дивиденды (траст)
                {
                    if (isYearAnalysisMode)
                    {
                        var currency = tran.Currency;
                        if (depoByCurrency.ContainsKey(currency))
                            depoByCurrency[currency] += amountInUsd;
                        else depoByCurrency.Add(currency, amountInUsd);
                    }
                    else
                    {
                        var depo = tran.MyAccount.ShortName ??
                                   tran.MyAccount.ShortName ?? tran.MyAccount.Name;
                        depoList.Add($"{amStr} {depo} {tran.Comment} {tran.Timestamp:dd MMM}");
                    }
                    depoTotal += amountInUsd;
                }
                else if (tag.Id == 701) // manyback
                {
                    moneyBackList.Add($"{amStr} {tran.MyAccount.ShortName} {tran.Comment} {tran.Timestamp:dd MMM}");
                    moneyBackTotal += amountInUsd;
                }
                else if (tag.Is(188)) // остальная рента
                {
                    rantierList.Add($"{amStr} {tran.MyAccount.ShortName} {tran.Comment} {tran.Timestamp:dd MMM}");
                    rantierTotal += amountInUsd;
                }
                else  // остальные типы доходов
                {
                    restList.Add($"{amStr} {BuildCommentForIncomeTransaction(tran, false)} {tran.Timestamp:dd MMM}");
                    restTotal += amountInUsd;
                }

                total += amountInUsd;
            }

            if (isYearAnalysisMode)
            {
                InsertYearSalary(list, employers);
                InsertYearDepositIncome(list, depoByCurrency);
            }
            else
            {
                InsertLinesIntoIncomeList(list, true, salaryList, salaryTotal, "зарплата");
                InsertLinesIntoIncomeList(list, true, depoList, depoTotal, "депозиты");
            }


            InsertLinesIntoIncomeList(list, !isYearAnalysisMode, moneyBackList, moneyBackTotal, "манибэк");
            InsertLinesIntoIncomeList(list, !isYearAnalysisMode, rantierList, rantierTotal, "прочие с капитала");
            InsertLinesIntoIncomeList(list, !isYearAnalysisMode, restList, restTotal, "прочее");

            list.Add("");
            list.Add($"Итого {total:#,0.00} usd", FontWeights.Bold, Brushes.Blue);

            return new Tuple<ListOfLines, decimal, decimal>(list, total, depoTotal);
        }

        private static void InsertYearSalary(ListOfLines list, Dictionary<AccountItemModel, decimal> employers)
        {
            list.Add("");
            foreach (var pair in employers)
            {
                list.Add($"   {pair.Value:#,0.00} usd  {pair.Key.Name}", Brushes.Blue);
            }
            list.Add($"          Итого зарплата {employers.Values.Sum():#,0.00} usd", FontWeights.Bold, Brushes.Blue);
        }

        private static void InsertYearDepositIncome(ListOfLines list, Dictionary<CurrencyCode, decimal> depoByCurrency)
        {
            if (depoByCurrency.Count == 0) return;
            list.Add("");
            foreach (var pair in depoByCurrency)
            {
                list.Add($"   депозиты в {pair.Key} - {pair.Value:#,0.00} usd", Brushes.Blue);
            }
            list.Add($"          Итого депозиты {depoByCurrency.Values.Sum():#,0.00} usd", FontWeights.Bold, Brushes.Blue);
        }

        private static void InsertLinesIntoIncomeList(ListOfLines list, bool isDetailed, List<string> lines, decimal total, string word)
        {
            if (lines.Count == 0) return;
            list.Add("");
            if (isDetailed)
            {
                foreach (var line in lines)
                {
                    list.Add($"   {line}", Brushes.Blue);
                }
            }

            list.Add($"          Итого {word} {total:#,0.00} usd", FontWeights.Bold, Brushes.Blue);
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
