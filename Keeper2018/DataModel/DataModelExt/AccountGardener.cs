using System.Collections.Generic;
using System.Collections.ObjectModel;
using KeeperDomain;

namespace Keeper2018
{
    public static class AccountGardener
    {
        public static void FillInAccountTree(this KeeperDataModel dataModel)
        {
            dataModel.AccountsTree = new ObservableCollection<AccountModel>();
            dataModel.AcMoDict = new Dictionary<int, AccountModel>();
            foreach (var account in dataModel.Bin.AccountPlaneList)
            {
                var accountModel = account.Map(dataModel.AcMoDict);
                if (account.OwnerId == 0)
                    dataModel.AccountsTree.Add(accountModel);
            }
        }

        public static void FlattenAccountTree(this KeeperDataModel dataModel)
        {
            dataModel.Bin.AccountPlaneList = new List<Account>();
            foreach (var root in dataModel.AccountsTree)
            {
                dataModel.Bin.AccountPlaneList.AddRange(FlattenOne(root));
            }
        }

        private static IEnumerable<Account> FlattenOne(AccountModel accountModel)
        {
            var result = new List<Account> { accountModel.Map() };
            foreach (var child in accountModel.Children)
            {
                result.AddRange(FlattenOne(child));
            }
            return result;
        }
    }
}