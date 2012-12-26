using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Keeper.DomainModel;

namespace Keeper.ViewModels
{
    public static class AssociatedArticlesLists
    {
        public static KeeperDb Db { get { return IoC.Get<KeeperDb>(); } }

        public static List<Account> WhoGivesMeMoneyAccounts { get; set; }
        public static List<Account> WhoTakesMyMoneyAccounts { get; set; }
        public static List<Account> IncomeArticles { get; set; }
        public static List<Account> ExpenseArticles { get; set; }

        public static void ComboBoxesValues()
        {
            WhoGivesMeMoneyAccounts = (Db.Accounts.Local.Where(account => (account.IsDescendantOf("ДеньгоДатели") ||
              account.IsDescendantOf("Банки")) && account.Children.Count == 0)).ToList();
            WhoTakesMyMoneyAccounts = (Db.Accounts.Local.Where(account => account.IsDescendantOf("ДеньгоПолучатели") &&
              account.Children.Count == 0)).ToList();
            IncomeArticles = (Db.Accounts.Local.Where(account => account.GetRootName() == "Все доходы" &&
              account.Children.Count == 0)).ToList();
            ExpenseArticles = (Db.Accounts.Local.Where(account => account.GetRootName() == "Все расходы" &&
              account.Children.Count == 0)).ToList();
        }

        static AssociatedArticlesLists()
        {
            Db.Accounts.Load();
            ComboBoxesValues();
        }
    }
}
