using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using KeeperDomain;

namespace Keeper2018
{
    public static class AccountTreeMapper
    {
        public static void FillInAccountTreeAndDict(this KeeperDataModel dataModel, KeeperBin bin)
        {
            dataModel.AccountsTree = new ObservableCollection<AccountItemModel>();
            dataModel.AcMoDict = new Dictionary<int, AccountItemModel>();
            foreach (var account in bin.AccountPlaneList)
            {
                var accountItemModel = account.Map();
                dataModel.AcMoDict.Add(accountItemModel.Id, accountItemModel);

                accountItemModel.Deposit = bin.Deposits.FirstOrDefault(d => d.MyAccountId == accountItemModel.Id);
              
                accountItemModel.PayCard = bin.PayCards.FirstOrDefault(c=>c.MyAccountId == accountItemModel.Id);

                if (account.ParentId == 0)
                    dataModel.AccountsTree.Add(accountItemModel);
                else
                {
                    var parentItemModel = dataModel.AcMoDict[account.ParentId];
                    parentItemModel.Children.Add(accountItemModel);
                    accountItemModel.Parent = parentItemModel;
                }
            }
        }

        public static IEnumerable<Account> FlattenAccountTree(this KeeperDataModel dataModel)
        {
            return dataModel.AccountsTree.SelectMany(FlattenBranch);
        }

        private static IEnumerable<Account> FlattenBranch(AccountItemModel accountItemModel)
        {
            var result = new List<Account> { accountItemModel.Map() };
            foreach (var child in accountItemModel.Children)
            {
                result.AddRange(FlattenBranch((AccountItemModel)child));
            }
            return result;
        }
    }
}