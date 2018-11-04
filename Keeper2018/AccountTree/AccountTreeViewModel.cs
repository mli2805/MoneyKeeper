using System.Collections.Generic;
using System.Collections.ObjectModel;
using Caliburn.Micro;

namespace Keeper2018
{
    public class AccountTreeViewModel : PropertyChangedBase
    {
        public IWindowManager WindowManager { get; }
        public AskDragAccountActionViewModel AskDragAccountActionViewModel { get; }

        private KeeperDb _keeperDb;
        public ObservableCollection<AccountModel> AccountsTree { get; set; } = new ObservableCollection<AccountModel>();

        public AccountTreeViewModel(KeeperDb keeperDb, IWindowManager windowManager,
            AskDragAccountActionViewModel askDragAccountActionViewModel)
        {
            WindowManager = windowManager;
            AskDragAccountActionViewModel = askDragAccountActionViewModel;

            _keeperDb = keeperDb;
            FillInTheTree();
        }

        private void FillInTheTree()
        {
            foreach (var account in _keeperDb.AccountPlaneList)
            {
                var accountModel = new AccountModel(account.Header)
                {
                    Id = account.Id,
                    IsExpanded = account.IsExpanded,
                };

                if (account.OwnerId == 0)
                {
                    AccountsTree.Add(accountModel);
                }
                else
                {
                    var ownerModel = GetById(account.OwnerId, AccountsTree);
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

        public void RefreshPlaneList()
        {
            _keeperDb.AccountPlaneList = Flatten(AccountsTree);
        }

        private static List<Account> Flatten(ICollection<AccountModel> roots)
        {
            var result = new List<Account>();
            foreach (var root in roots)
            {
                result.AddRange(FlattenOne(root));
            }
            return result;
        }

        private static IEnumerable<Account> FlattenOne(AccountModel accountModel)
        {
            var result = new List<Account> {Map(accountModel)};
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
