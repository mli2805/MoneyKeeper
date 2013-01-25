using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Keeper.DomainModel;

namespace Keeper.ViewModels
{
  /// <summary>
  /// ����������� �����, �.�. ������ ������ ��������� ������������� � ������������ ������
  /// ������ �� ����� ����� ����������� ��������������� � �������� � ������ ���������
  /// </summary>
  public static class UsefulLists
  {
    public static KeeperDb Db { get { return IoC.Get<KeeperDb>(); } }

    public static List<CurrencyCodes> CurrencyList { get; private set; }
    public static List<OperationType> OperationTypeList { get; private set; }

    public static List<Account> AllAccounts { get; set; } // �.�. �����, �� �� ���������
    public static List<Account> AllArticles { get; set; } // �.�. ���������, �� �� �����

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
                      (account.GetRootName() == "���" || account.GetRootName() == "�������") && account.Children.Count == 0)).ToList();
      AllArticles = (Db.Accounts.Local.Where(account =>
                      (account.GetRootName() == "��� ������" || account.GetRootName() == "��� �������") && account.Children.Count == 0)).ToList();

      MyAccounts = (Db.Accounts.Local.Where(account => account.GetRootName() == "���" &&
             account.Children.Count == 0 || account.Name == "��� ����� ��������� ��������")).ToList();
      MyAccountsForShopping = (Db.Accounts.Local.Where(account => account.GetRootName() == "���" &&
        account.Children.Count == 0 && !account.IsDescendantOf("��������"))).ToList();
      AccountsWhoGivesMeMoney = (Db.Accounts.Local.Where(account => (account.IsDescendantOf("������������") ||
        account.IsDescendantOf("�����")) && account.Children.Count == 0)).ToList();
      AccountsWhoTakesMyMoney = (Db.Accounts.Local.Where(account => account.IsDescendantOf("����������������") &&
        account.Children.Count == 0)).ToList();
      BankAccounts = (Db.Accounts.Local.Where(account => account.IsDescendantOf("�����") &&
        account.Children.Count == 0)).ToList();
      IncomeArticles = (Db.Accounts.Local.Where(account => account.GetRootName() == "��� ������" &&
        account.Children.Count == 0)).ToList();
      ExpenseArticles = (Db.Accounts.Local.Where(account => account.GetRootName() == "��� �������" &&
        account.Children.Count == 0)).ToList();

      ExternalAccounts = (Db.Accounts.Local.Where(account => account.IsDescendantOf("�������") && account.Children.Count == 0)).ToList();
      AssociatedArticles = AllArticles;
    }
  }
}