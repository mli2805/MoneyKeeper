using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Keeper.DomainModel;

namespace Keeper.Utils
{
    class ClearDb
    {
        [Import]
        public static KeeperDb Db { get { return IoC.Get<KeeperDb>(); } }

        public static void ClearAllTables()
        {
            ClearCurrencyRates();
            ClearArticlesAssociations();
            ClearTransactions();
            ClearAccounts();
        }

        private static void ClearTransactions()
        {
            foreach (var transaction in Db.Transactions.ToArray())
            {
                Db.Transactions.Remove(transaction);
            }
        }

        private static void ClearArticlesAssociations()
        {
            foreach (var association in Db.ArticlesAssociations.ToArray())
            {
                Db.ArticlesAssociations.Remove(association);
            }
        }

        private static void ClearCurrencyRates()
        {
            foreach (var currencyRate in Db.CurrencyRates.ToArray())
            {
                Db.CurrencyRates.Remove(currencyRate);
            }
        }

        private static void ClearAccounts()
        {
            var roots = new List<Account>(from account in Db.Accounts.Local
                                          where account.Parent == null
                                          select account);
            foreach (var root in roots)
            {
                RemoveAccountFromDatabase(root);
            }
        }

        public static void RemoveAccountFromDatabase(Account account)
        {
            foreach (var child in account.Children.ToArray())
            {
                RemoveAccountFromDatabase(child);
            }
            Db.Accounts.Remove(account);
        }

    }
}
