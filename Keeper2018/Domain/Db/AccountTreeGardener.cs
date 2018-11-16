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
        public static AccountModel GetSelectedAccountModel(this KeeperDb db)
        {
            foreach (var root in db.AccountsTree)
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

        public static void RemoveSelectedAccount(this KeeperDb db)
        {
            var accountModel = db.GetSelectedAccountModel();
            if (accountModel == null) return;
            var windowManager = new WindowManager();
            switch (CheckIfAccountCanBeDeleted(db, accountModel))
            {
                case AccountCantBeDeletedReasons.CanDelete:
                    var myMessageBoxViewModel = new MyMessageBoxViewModel(MessageType.Confirmation,
                        new List<string>()
                        {
                            "���������, ���� �� ������������ � �����������.",
                            "�������� �����", "",
                            $"<<{accountModel.Name}>>", "",
                            "�������?"
                        });
                    windowManager.ShowDialog(myMessageBoxViewModel);
                    if (myMessageBoxViewModel.IsAnswerPositive)
                        RemoveAccountLowLevel(db, accountModel);
                    break;
                case AccountCantBeDeletedReasons.IsRoot:
                    windowManager.ShowDialog(new MyMessageBoxViewModel(MessageType.Error,
                        "�������� ���� ������ �������!"));
                    break;
                case AccountCantBeDeletedReasons.HasChildren:
                    windowManager.ShowDialog(new MyMessageBoxViewModel(MessageType.Error,
                        new List<string>() { "��������� �������", "", "������ �������� ������ ������ ������!" }, -1));
                    break;
                case AccountCantBeDeletedReasons.HasRelatedTransactions:
                    windowManager.ShowDialog(new MyMessageBoxViewModel(MessageType.Error,
                        "���� ���� ������������ � ���������!"));
                    break;
            }
        }

        private static AccountCantBeDeletedReasons CheckIfAccountCanBeDeleted(this KeeperDb db, AccountModel account)
        {
            if (account.Owner == null) return AccountCantBeDeletedReasons.IsRoot;
            if (account.Children.Any()) return AccountCantBeDeletedReasons.HasChildren;
            if (db.TransactionModels.Any(t =>
                t.MyAccount.Equals(account) || 
                t.MySecondAccount != null && t.MySecondAccount.Equals(account) ||
                t.Tags != null && t.Tags.Contains(account)))
                return AccountCantBeDeletedReasons.HasRelatedTransactions;
            return AccountCantBeDeletedReasons.CanDelete;
        }

        private static void RemoveAccountLowLevel(this KeeperDb db, AccountModel accountModel)
        {

        }
    }


}