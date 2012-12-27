using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Keeper.DomainModel;

namespace Keeper.Utils
{
    class Balance
    {
        [Import]
        public static KeeperDb Db { get { return IoC.Get<KeeperDb>(); } }

        class BalancePair
        {
            public CurrencyCodes? Currency;
            public decimal Amount;
        }

        private static IEnumerable<BalancePair> AccountBalancePairs(Account balancedAccount)
        {
            var tempBalance = (from t in Db.Transactions.Local
                               where t.Credit.IsTheSameOrDescendantOf(balancedAccount.Name) ||
                                   t.Debet.IsTheSameOrDescendantOf(balancedAccount.Name)
                               group t by t.Currency into g
                               select new BalancePair()
                                          {
                                              Currency = g.Key,
                                              Amount = g.Sum(a => a.Amount * a.SignForAmount(balancedAccount))
                                          }).
                Concat
                (from t in Db.Transactions.Local
                 // учесть вторую сторону обмена - приход денег в другой валюте
                 where t.Amount2 != 0 && (t.Credit.IsTheSameOrDescendantOf(balancedAccount.Name) ||
                                                   t.Debet.IsTheSameOrDescendantOf(balancedAccount.Name))
                 group t by t.Currency2 into g
                 select new BalancePair()
                            {
                                Currency = g.Key,
                                Amount = g.Sum(a => a.Amount2 * a.SignForAmount(balancedAccount) * -1)
                            });

            return from b in tempBalance
                   group b by b.Currency into g
                   select new BalancePair()
                                   {
                                       Currency = g.Key,
                                       Amount = g.Sum(a => a.Amount)
                                   };

        }

        private static IEnumerable<BalancePair> ArticleBalancePairs(Account balancedAccount)
        {
            return from t in Db.Transactions.Local
                   where t.Article != null && t.Article.IsTheSameOrDescendantOf(balancedAccount.Name)
                   group t by t.Currency into g
                   select new BalancePair()
                              {
                                  Currency = g.Key,
                                  Amount = g.Sum(a => a.Amount)
                              };
        }

        private static List<string> OneBalance(Account balancedAccount)
        {
            var balance = new List<string>();

            bool kind = balancedAccount.IsTheSameOrDescendantOf("Все доходы") || balancedAccount.IsTheSameOrDescendantOf("Все расходы");
            var balancePairs = kind ? ArticleBalancePairs(balancedAccount) : AccountBalancePairs(balancedAccount);

            foreach (var item in balancePairs)
                if (item.Amount != 0) balance.Add(String.Format("{0:#,#} {1}", item.Amount, item.Currency));
            return balance;
        }

        /// <summary>
        /// Функция нужна только заполнения для 2-й рамки на ShellView
        /// Расчитываются остатки по счету и его потомкам 1-го поколения
        /// </summary>
        public static void CountBalances(Account selectedAccount, ObservableCollection<string> balanceList)
        {
            balanceList.Clear();

            var b = OneBalance(selectedAccount);
            foreach (var st in b)
                balanceList.Add(st);

            foreach (var child in selectedAccount.Children)
            {
                b = OneBalance(child);
                if (b.Count > 0) balanceList.Add("         " + child.Name);
                foreach (var st in b)
                    balanceList.Add("    " + st);
            }
        }

    }
}
