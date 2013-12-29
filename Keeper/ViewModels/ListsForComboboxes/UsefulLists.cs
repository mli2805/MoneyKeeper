using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using Keeper.DomainModel;
using Keeper.Utils.Accounts;

namespace Keeper.ViewModels
{
	public interface IUsefulLists 
	{
		void FillLists();
	}
	/// <summary>
	/// ����������� �����, �.�. ������ ������ ��������� ������������� � ������������ ������
	/// ������ �� ����� ����� ����������� ��������������� � �������� � ������ ���������
	/// </summary>
	
	[Export (typeof(IUsefulLists	))]
	public class UsefulLists : IUsefulLists
	{
		readonly KeeperDb mDb;
		readonly AccountTreeStraightener mAccountTreeStraightener;

		[ImportingConstructor]
		public UsefulLists(KeeperDb db, AccountTreeStraightener accountTreeStraightener)
		{
			mDb = db;
			mAccountTreeStraightener = accountTreeStraightener;
		}

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

		public void FillLists()
		{
			CurrencyList = Enum.GetValues(typeof(CurrencyCodes)).OfType<CurrencyCodes>().ToList();
			OperationTypeList = Enum.GetValues(typeof(OperationType)).OfType<OperationType>().ToList();

			AllAccounts = (mAccountTreeStraightener.Flatten(mDb.Accounts).Where(account =>
							(account.GetRootName() == "���" || account.GetRootName() == "�������") && account.Children.Count == 0)).ToList();
			AllArticles = (mAccountTreeStraightener.Flatten(mDb.Accounts).Where(account =>
							(account.GetRootName() == "��� ������" || account.GetRootName() == "��� �������") && account.Children.Count == 0)).ToList();

			MyAccounts = (mAccountTreeStraightener.Flatten(mDb.Accounts).Where(account => account.GetRootName() == "���" &&
				   account.Children.Count == 0 || account.Name == "��� ����� ��������� ��������")).ToList();
			MyModernAccounts = mAccountTreeStraightener.Flatten(mDb.Accounts).Where(account => account.GetRootName() == "���" &&
						 account.Children.Count == 0 && account.Parent.Name != "�������� ��������").ToList();
			MyAccountsForShopping = (mAccountTreeStraightener.Flatten(mDb.Accounts).Where(account => account.GetRootName() == "���" &&
			  account.Children.Count == 0 && !account.IsDescendantOf("��������"))).ToList();
			AccountsWhoGivesMeMoney = (mAccountTreeStraightener.Flatten(mDb.Accounts).Where(account => (account.IsDescendantOf("������������") ||
			  account.IsDescendantOf("�����")) && account.Children.Count == 0)).ToList();
			AccountsWhoTakesMyMoney = (mAccountTreeStraightener.Flatten(mDb.Accounts).Where(account => account.IsDescendantOf("����������������") &&
			  account.Children.Count == 0)).ToList();
			BankAccounts = (mAccountTreeStraightener.Flatten(mDb.Accounts).Where(account => account.IsDescendantOf("�����") &&
			  account.Children.Count == 0)).ToList();
			IncomeArticles = (mAccountTreeStraightener.Flatten(mDb.Accounts).Where(account => account.GetRootName() == "��� ������" &&
			  account.Children.Count == 0)).ToList();
			ExpenseArticles = (mAccountTreeStraightener.Flatten(mDb.Accounts).Where(account => account.GetRootName() == "��� �������" &&
			  account.Children.Count == 0)).ToList();

			ExternalAccounts = (mAccountTreeStraightener.Flatten(mDb.Accounts).Where(account => account.IsDescendantOf("�������") && account.Children.Count == 0)).ToList();
			AssociatedArticles = AllArticles;
		}
	}
}