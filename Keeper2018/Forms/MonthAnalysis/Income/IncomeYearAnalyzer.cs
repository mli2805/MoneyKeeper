using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using KeeperDomain;

namespace Keeper2018
{
    public static class IncomeYearAnalyzer
    {
        public static IIncomeForPeriod SortYearIncome(this KeeperDataModel dataModel,
            IEnumerable<TransactionModel> incomeTrans)
        {
            var result = new YearIncome();

            foreach (var tran in incomeTrans)
            {
                var amountInUsd = dataModel.AmountInUsd(tran.Timestamp, tran.Currency, tran.Amount);
                var tag = tran.Tags.First(t => t.Is(185)); // один тэг тип доходов, другой контрагент

                if (tag.Is(186) || tag.Is(212)) // зарплата (+иррациональные)
                {
                    result.Employers.CreateNewOrSumValue(tran.Tags.First(t => t != tag), amountInUsd);
                }
                else if (tag.Id == 208 || tag.Id == 209) // %% по вкладу (по карточкам тоже) или дивиденды (траст)
                {
                    result.DepoByCurrency.CreateNewOrSumValue(tran.Currency, amountInUsd);
                }
                else if (tag.Id == 701) // manyback
                {
                    result.Cards.CreateNewOrSumValue(tran.MyAccount, amountInUsd);
                }
                else  // остальные типы доходов
                {

                    result.Rests.CreateNewOrSumValue(tag, amountInUsd);
                    // result.Rests.CreateNewOrSumValue(tran.Tags.First(t => t != tag), amountInUsd);
                }
            }

            return result;
        }

        public static ListOfLines InsertYearSalary(this ListOfLines list, Dictionary<AccountItemModel, decimal> employers)
        {
            if (employers.Count == 0) return list;
            list.Add("");
            foreach (var pair in employers.OrderByDescending(p => p.Value))
            {
                var sum = $"{pair.Value:#,0.00} usd".PadLeft(14);
                list.Add($" {sum}  {pair.Key.Name}", Brushes.Blue);
            }
            list.Add($"   Итого зарплата {employers.Values.Sum():#,0.00} usd", FontWeights.Bold, Brushes.Blue);
            return list;
        }

        public static ListOfLines InsertYearDepositIncome(this ListOfLines list, Dictionary<CurrencyCode, decimal> depoByCurrency)
        {
            if (depoByCurrency.Count == 0) return list;
            list.Add("");
            foreach (var pair in depoByCurrency.OrderByDescending(p => p.Value))
            {
                var sum = $"{pair.Value:#,0.00} usd".PadLeft(14);
                list.Add($" {sum} - депозиты в {pair.Key}", Brushes.Blue);
            }
            list.Add($"   Итого депозиты {depoByCurrency.Values.Sum():#,0.00} usd", FontWeights.Bold, Brushes.Blue);
            return list;
        }

        public static ListOfLines InsertYearMoneyback(this ListOfLines list, Dictionary<AccountItemModel, decimal> cards)
        {
            if (cards.Count == 0) return list;
            list.Add("");
            foreach (var pair in cards.OrderByDescending(p => p.Value))
            {
                var sum = $"{pair.Value:#,0.00} usd".PadLeft(14);
                var cardName = string.IsNullOrEmpty(pair.Key.ShortName) ? pair.Key.Name : pair.Key.ShortName;
                list.Add($" {sum}  {cardName}", Brushes.Blue);

            }
            list.Add($"   Итого манибэк {cards.Values.Sum():#,0.00} usd", FontWeights.Bold, Brushes.Blue);
            return list;
        }

        public static void InsertYearRest(this ListOfLines list, Dictionary<AccountItemModel, decimal> rests)
        {
            if (rests.Count == 0) return;
            list.Add("");
            foreach (var pair in rests.OrderByDescending(p => p.Value))
            {
                var sum = $"{pair.Value:#,0.00} usd".PadLeft(14);
                list.Add($" {sum}  {pair.Key.Name}", Brushes.Blue);
            }
            list.Add($"   Итого прочее {rests.Values.Sum():#,0.00} usd", FontWeights.Bold, Brushes.Blue);
        }

        private static void CreateNewOrSumValue<TKey>(this IDictionary<TKey, decimal> map, TKey key, decimal value)
        {
            if (map.ContainsKey(key))
                map[key] += value;
            else
                map.Add(key, value);
        }
    }
}