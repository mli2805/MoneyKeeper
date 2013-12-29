using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using Keeper.DomainModel;
using Keeper.ViewModels;

namespace Keeper.Utils.Accounts
{
	[Export]
	public class AccountTreesGardener
	{
		readonly IWindowManager mWindowManager;
		readonly IMyFactory mMyFactory;
		private readonly KeeperDb _db;
		readonly AccountTreeStraightener mAccountTreeStraightener;
		readonly IUsefulLists mUsefulLists;

		[ImportingConstructor]
		public AccountTreesGardener(KeeperDb db, AccountTreeStraightener accountTreeStraightener,
			IUsefulLists usefulLists, IWindowManager windowManager, IMyFactory myFactory)
		{
			mWindowManager = windowManager;
			mMyFactory = myFactory;
			_db = db;
			mAccountTreeStraightener = accountTreeStraightener;
			mUsefulLists = usefulLists;
		}

		public void RemoveAccount(Account selectedAccount)
		{
			if (selectedAccount.Parent == null)
			{
				MessageBox.Show("Корневой счет нельзя удалять!", "Отказ!");
				return;
			}
			if (selectedAccount.Children.Count > 0)
			{
				MessageBox.Show("Удалять разрешено \n только конечные листья дерева счетов!", "Отказ!");
				return;
			}
			// такой запрос возвращает не коллекцию, а энумератор
			var tr = from transaction in _db.Transactions
					 where transaction.Debet == selectedAccount || transaction.Credit == selectedAccount || transaction.Article == selectedAccount
					 select transaction;

			// Any() пытается двинуться по этому энумератору и если может, то true
			if (tr.Any())
			{
				MessageBox.Show("Этот счет используется в проводках!", "Отказ!");
				return;
			}
			if (MessageBox.Show("Проверено, счет не используется в транзакциях.\n Удаление счета\n\n <<" + selectedAccount.Name + ">>\n          Удалить?", "Confirm",
								MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No) return;

			selectedAccount.Parent.Children.Remove(selectedAccount);

		}

		public void AddAccount(Account selectedAccount)
		{
			var accountInWork = mMyFactory.CreateAccount(selectedAccount);
			var vm = mMyFactory.CreateAddAndEditAccountViewModel(accountInWork, "Добавить");
			if (mWindowManager.ShowDialog(vm) != true) return;

			selectedAccount = accountInWork.Parent;
			accountInWork.Id = (from account in mAccountTreeStraightener.Flatten(_db.Accounts) select account.Id).Max() + 1;
			selectedAccount.Children.Add(accountInWork);

			mUsefulLists.FillLists();
		}

		public void ChangeAccount(Account selectedAccount)
		{
			var accountInWork = new Account();
			Account.CopyForEdit(accountInWork, selectedAccount);
			if (mWindowManager.ShowDialog(new AddAndEditAccountViewModel(accountInWork, "Редактировать")) != true) return;

			if (selectedAccount.Parent != accountInWork.Parent)
			{
				accountInWork.Parent.Children.Add(accountInWork);
				selectedAccount.Parent.Children.Remove(selectedAccount);
			}
			else selectedAccount.Name = accountInWork.Name;
		}

	}
	public interface IMyFactory {
		AddAndEditAccountViewModel CreateAddAndEditAccountViewModel(Account account, string title);
		Account CreateAccount();
		Account CreateAccount(Account parent);
	}

	[Export (typeof(IMyFactory))]
	public class MyFactory : IMyFactory
	{
		public AddAndEditAccountViewModel CreateAddAndEditAccountViewModel(Account account, string title)
		{
			return new AddAndEditAccountViewModel(account, title);
		}

		public Account CreateAccount()
		{
			return new Account();
		}

		public Account CreateAccount(Account parent)
		{
			return new Account(){Parent = parent};
		}
	}
}
