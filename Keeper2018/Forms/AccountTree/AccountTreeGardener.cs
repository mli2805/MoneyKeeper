using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;

namespace Keeper2018
{
    public enum AccountCantBeDeletedReasons
    {
        CanDelete, IsRoot, HasChildren, HasRelatedTransactions
    }

    public static class AccountTreeGardener
    {
        public static AccountModel GetSelectedAccountModel(this KeeperDataModel dataModel)
        {
            foreach (var root in dataModel.AccountsTree)
            {
                var selected = GetSelectedAccountModelInBranch(root);
                if (selected != null) return selected;
            }
            return null;
        }

        private static AccountModel GetSelectedAccountModelInBranch(AccountModel branch)
        {
            if (branch.IsSelected) return branch;
            foreach (var child in branch.Children)
            {
                var selected = GetSelectedAccountModelInBranch(child);
                if (selected != null) return selected;
            }
            return null;
        }

        public static void RemoveSelectedAccount(this KeeperDataModel dataModel)
        {
            var accountModel = dataModel.GetSelectedAccountModel();
            if (accountModel == null) return;
            var windowManager = new WindowManager();
            switch (CheckIfAccountCanBeDeleted(dataModel, accountModel))
            {
                case AccountCantBeDeletedReasons.CanDelete:
                    var myMessageBoxViewModel = new MyMessageBoxViewModel(MessageType.Confirmation,
                        new List<string>()
                        {
                            "Проверено, счет не используется в транзакциях.",
                            "Удаление счета", "",
                            $"<<{accountModel.Name}>>", "",
                            "Удалить?"
                        });
                    windowManager.ShowDialog(myMessageBoxViewModel);
                    if (myMessageBoxViewModel.IsAnswerPositive)
                        RemoveAccountLowLevel(dataModel, accountModel);
                    break;
                case AccountCantBeDeletedReasons.IsRoot:
                    windowManager.ShowDialog(new MyMessageBoxViewModel(MessageType.Error,
                        "Корневой счет нельзя удалять!"));
                    break;
                case AccountCantBeDeletedReasons.HasChildren:
                    windowManager.ShowDialog(new MyMessageBoxViewModel(MessageType.Error,
                        new List<string>() { "Разрешено удалять", "", "только конечные листья дерева счетов!" }, -1));
                    break;
                case AccountCantBeDeletedReasons.HasRelatedTransactions:
                    windowManager.ShowDialog(new MyMessageBoxViewModel(MessageType.Error,
                        "Этот счет используется в проводках!"));
                    break;
            }
        }

        private static AccountCantBeDeletedReasons CheckIfAccountCanBeDeleted(this KeeperDataModel dataModel, AccountModel account)
        {
            if (account.Owner == null) return AccountCantBeDeletedReasons.IsRoot;
            if (account.Children.Any()) return AccountCantBeDeletedReasons.HasChildren;
            if (dataModel.Bin.Transactions.Values.Any(t =>
                t.MyAccount.Equals(account.Id) || 
                t.MySecondAccount.Equals(account.Id) ||
                t.Tags != null && t.Tags.Contains(account.Id)))
                return AccountCantBeDeletedReasons.HasRelatedTransactions;
            return AccountCantBeDeletedReasons.CanDelete;
        }

        private static void RemoveAccountLowLevel(this KeeperDataModel dataModel, AccountModel accountModel)
        {
            var owner = accountModel.Owner;
            accountModel.Owner = null;
            owner.Items.Remove(accountModel);
            dataModel.Bin.AccountPlaneList.RemoveAll(a => a.Id == accountModel.Id);
        }
    }
}