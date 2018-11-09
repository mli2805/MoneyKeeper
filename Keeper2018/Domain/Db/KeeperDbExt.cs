using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Keeper2018
{
    public static class KeeperDbExt
    {
        public static void FillInTheTree(this KeeperDb db)
        {
            db.AccountsTree = new ObservableCollection<AccountModel>();
            foreach (var account in db.AccountPlaneList)
            {
                var accountModel = new AccountModel(account.Header)
                {
                    Id = account.Id,
                    IsExpanded = account.IsExpanded,
                };

                if (account.OwnerId == 0)
                {
                    db.AccountsTree.Add(accountModel);
                }
                else
                {
                    var ownerModel = GetById(account.OwnerId, db.AccountsTree);
                    ownerModel.Items.Add(accountModel);
                    accountModel.Owner = ownerModel;
                }
            }
        }

        private static AccountModel GetById(int id, ICollection<AccountModel> roots)
        {
            foreach (var account in roots)
            {
                if (account.Id == id) return account;
                var acc = GetById(id, account.Children);
                if (acc != null) return acc;
            }
            return null;
        }

        public static AccountModel GetByName(string name, ICollection<AccountModel> roots)
        {
            foreach (var account in roots)
            {
                if (account.Name == name) return account;
                var acc = GetByName(name, account.Children);
                if (acc != null) return acc;
            }
            return null;
        }


        public static void Flatten(this KeeperDb db)
        {
            db.AccountPlaneList = new List<Account>();
            foreach (var root in db.AccountsTree)
            {
                db.AccountPlaneList.AddRange(FlattenOne(root));
            }
        }

        private static IEnumerable<Account> FlattenOne(AccountModel accountModel)
        {
            var result = new List<Account> { Map(accountModel) };
            foreach (var child in accountModel.Children)
            {
                result.AddRange(FlattenOne(child));
            }
            return result;
        }

        private static Account Map(AccountModel model)
        {
            return new Account()
            {
                Id = model.Id,
                OwnerId = model.Owner?.Id ?? 0,
                Header = (string)model.Header,
                IsExpanded = model.IsExpanded,
            };
        }
    }
}