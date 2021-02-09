using System.Collections.Generic;
using System.Collections.ObjectModel;
using KeeperDomain;

namespace Keeper2018
{
    public static class AccountGardener
    {
        public static void FillInAccountTreeAndDict(this KeeperDataModel dataModel)
        {
            dataModel.AccountsTree = new ObservableCollection<AccountModel>();
            dataModel.AcMoDict = new Dictionary<int, AccountModel>();
            foreach (var account in dataModel.AccountPlaneList)
            {
                var accountModel = account.Map();
                dataModel.AcMoDict.Add(accountModel.Id, accountModel);
                if (account.OwnerId == 0)
                    dataModel.AccountsTree.Add(accountModel);
                else
                {
                    var ownerModel = dataModel.AcMoDict[account.OwnerId];
                    ownerModel.Items.Add(accountModel);
                    ownerModel.IsFolder = true;
                    accountModel.Owner = ownerModel;
                }
            }
        }

        public static void FlattenAccountTree(this KeeperDataModel dataModel)
        {
            dataModel.AccountPlaneList = new List<Account>();
            foreach (var root in dataModel.AccountsTree)
            {
                dataModel.AccountPlaneList.AddRange(FlattenOne(root));
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