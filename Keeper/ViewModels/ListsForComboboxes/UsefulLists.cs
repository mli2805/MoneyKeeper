using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Keeper.DomainModel;

namespace Keeper.ViewModels
{
  /// <summary>
  /// статический класс, т.к. комбик внутри датагрида привязывается к статическому классу
  /// комбик на форме можно привязывать непосредственно к свойству в классе вьюмодели
  /// </summary>
  public static class UsefulLists
  {
    public static KeeperDb Db { get { return IoC.Get<KeeperDb>(); } }

    public static List<CurrencyCodes> CurrencyList { get; private set; }
    public static List<OperationType> OperationTypeList { get; private set; }

    public static List<Account> AllAccounts { get; set; } // т.е. счета, но не категории
    public static List<Account> AllArticles { get; set; } // т.е. категории, но не счета

    public static List<Account> MyAccounts { get; set; }
    public static List<Account> MyAccountsForShopping { get; set; }
    public static List<Account> AccountsWhoGivesMeMoney { get; set; }
    public static List<Account> AccountsWhoTakesMyMoney { get; set; }
    public static List<Account> BankAccounts { get; set; }
    public static List<Account> IncomeArticles { get; set; }
    public static List<Account> ExpenseArticles { get; set; }

    public static List<Account> ExternalAccounts { get; set; }
    public static List<Account> AssociatedArticles { get; set; }

    static UsefulLists()
    {
      CurrencyList = Enum.GetValues(typeof(CurrencyCodes)).OfType<CurrencyCodes>().ToList();
      OperationTypeList = Enum.GetValues(typeof(OperationType)).OfType<OperationType>().ToList();

      FillListsFromLocal();
    }

    public static void FillListsFromLocal()
    {
      AllAccounts = (Db.Accounts.Local.Where(account =>
                      (account.GetRootName() == "Мои" || account.GetRootName() == "Внешние") && account.Children.Count == 0)).ToList();
      AllArticles = (Db.Accounts.Local.Where(account =>
                      (account.GetRootName() == "Все доходы" || account.GetRootName() == "Все расходы") && account.Children.Count == 0)).ToList();

      MyAccounts = (Db.Accounts.Local.Where(account => account.GetRootName() == "Мои" &&
             account.Children.Count == 0 || account.Name == "Для ввода стартовых остатков")).ToList();
      MyAccountsForShopping = (Db.Accounts.Local.Where(account => account.GetRootName() == "Мои" &&
        account.Children.Count == 0 && !account.IsDescendantOf("Депозиты"))).ToList();
      AccountsWhoGivesMeMoney = (Db.Accounts.Local.Where(account => (account.IsDescendantOf("ДеньгоДатели") ||
        account.IsDescendantOf("Банки")) && account.Children.Count == 0)).ToList();
      AccountsWhoTakesMyMoney = (Db.Accounts.Local.Where(account => account.IsDescendantOf("ДеньгоПолучатели") &&
        account.Children.Count == 0)).ToList();
      BankAccounts = (Db.Accounts.Local.Where(account => account.IsDescendantOf("Банки") &&
        account.Children.Count == 0)).ToList();
      IncomeArticles = (Db.Accounts.Local.Where(account => account.GetRootName() == "Все доходы" &&
        account.Children.Count == 0)).ToList();
      ExpenseArticles = (Db.Accounts.Local.Where(account => account.GetRootName() == "Все расходы" &&
        account.Children.Count == 0)).ToList();

      ExternalAccounts = (Db.Accounts.Local.Where(account => account.IsDescendantOf("Внешние") && account.Children.Count == 0)).ToList();
      AssociatedArticles = AllArticles;
    }
  }
}