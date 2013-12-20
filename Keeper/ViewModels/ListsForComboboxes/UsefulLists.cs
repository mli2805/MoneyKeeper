using System;
using System.Collections.Generic;
using System.Linq;
using Keeper.DomainModel;

namespace Keeper.ViewModels
{
  /// <summary>
  /// ����������� �����, �.�. ������ ������ ��������� ������������� � ������������ ������
  /// ������ �� ����� ����� ����������� ��������������� � �������� � ������ ���������
  /// </summary>
  public static class UsefulLists
  {
    public static List<CurrencyCodes> CurrencyList { get; private set; }
    public static List<OperationType> OperationTypeList { get; private set; }

    public static List<Account> AllAccounts { get; set; } // �.�. �����, �� �� ���������
    public static List<Account> AllArticles { get; set; } // �.�. ���������, �� �� �����

    public static List<Account> MyAccounts { get; set; }
    public static List<Account> MyModernAccounts { get; set; }
    public static List<Account> MyAccountsForShopping { get; set; }
    public static List<Account> AccountsWhoGivesMeMoney { get; set; }
    public static List<Account> AccountsWhoTakesMyMoney { get; set; }
    public static List<Account> BankAccounts { get; set; }
    public static List<Account> IncomeArticles { get; set; }
    public static List<Account> ExpenseArticles { get; set; }

    public static List<Account> ExternalAccounts { get; set; }
    public static List<Account> AssociatedArticles { get; set; }

    public static void FillLists(KeeperDb db)
    {
      CurrencyList = Enum.GetValues(typeof(CurrencyCodes)).OfType<CurrencyCodes>().ToList();
      OperationTypeList = Enum.GetValues(typeof(OperationType)).OfType<OperationType>().ToList();

      AllAccounts = (db.AccountsPlaneList.Where(account =>
                      (account.GetRootName() == "���" || account.GetRootName() == "�������") && account.Children.Count == 0)).ToList();
      AllArticles = (db.AccountsPlaneList.Where(account =>
                      (account.GetRootName() == "��� ������" || account.GetRootName() == "��� �������") && account.Children.Count == 0)).ToList();

      MyAccounts = (db.AccountsPlaneList.Where(account => account.GetRootName() == "���" &&
             account.Children.Count == 0 || account.Name == "��� ����� ��������� ��������")).ToList();
      MyModernAccounts = db.AccountsPlaneList.Where(account => account.GetRootName() == "���" &&
                   account.Children.Count == 0 && account.Parent.Name != "�������� ��������").ToList();
      MyAccountsForShopping = (db.AccountsPlaneList.Where(account => account.GetRootName() == "���" &&
        account.Children.Count == 0 && !account.IsDescendantOf("��������"))).ToList();
      AccountsWhoGivesMeMoney = (db.AccountsPlaneList.Where(account => (account.IsDescendantOf("������������") ||
        account.IsDescendantOf("�����")) && account.Children.Count == 0)).ToList();
      AccountsWhoTakesMyMoney = (db.AccountsPlaneList.Where(account => account.IsDescendantOf("����������������") &&
        account.Children.Count == 0)).ToList();
      BankAccounts = (db.AccountsPlaneList.Where(account => account.IsDescendantOf("�����") &&
        account.Children.Count == 0)).ToList();
      IncomeArticles = (db.AccountsPlaneList.Where(account => account.GetRootName() == "��� ������" &&
        account.Children.Count == 0)).ToList();
      ExpenseArticles = (db.AccountsPlaneList.Where(account => account.GetRootName() == "��� �������" &&
        account.Children.Count == 0)).ToList();

      ExternalAccounts = (db.AccountsPlaneList.Where(account => account.IsDescendantOf("�������") && account.Children.Count == 0)).ToList();
      AssociatedArticles = AllArticles;
    }
  }
}