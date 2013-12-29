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

		private readonly KeeperDb _db;
		readonly AccountTreeStraightener mAccountTreeStraightener;

		[ImportingConstructor]
		public AccountTreesGardener(KeeperDb db, AccountTreeStraightener accountTreeStraightener, IWindowManager windowManager)
		{
			mWindowManager = windowManager;
			_db = db;
			mAccountTreeStraightener = accountTreeStraightener;
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
			IEnumerable<Transaction> tr = from transaction in _db.Transactions
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
			var accountInWork = new Account { Parent = selectedAccount };
			if (mWindowManager.ShowDialog(new AddAndEditAccountViewModel(accountInWork, "Добавить")) != true) return;

			selectedAccount = accountInWork.Parent;
			accountInWork.Id = (from account in new AccountTreeStraightener().Flatten(_db.Accounts) select account.Id).Max() + 1;
			selectedAccount.Children.Add(accountInWork);

			UsefulLists.FillLists(_db);
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
}
