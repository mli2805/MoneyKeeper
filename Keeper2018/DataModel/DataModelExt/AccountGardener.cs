using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using KeeperDomain;

namespace Keeper2018
{
    public static class AccountGardener
    {
        public static void FillInAccountTreeAndDict(this KeeperDataModel dataModel, KeeperBin bin)
        {
            dataModel.AccountsTree = new ObservableCollection<AccountModel>();
            dataModel.AcMoDict = new Dictionary<int, AccountModel>();
            foreach (var account in bin.AccountPlaneList)
            {
                var accountModel = account.Map();
                dataModel.AcMoDict.Add(accountModel.Id, accountModel);

                accountModel.Deposit = bin.Deposits.FirstOrDefault(d => d.MyAccountId == accountModel.Id);
                if (accountModel.Deposit != null)
                {
                    accountModel.Deposit.Card = bin.PayCards.FirstOrDefault(c => c.DepositId == accountModel.Deposit.Id);
                    if (accountModel.Deposit.Card != null)
                        accountModel.Deposit.Card.DepositId = accountModel.Deposit.Id;
                }

                if (account.OwnerId == 0)
                    dataModel.AccountsTree.Add(accountModel);
                else
                {
                    var ownerModel = dataModel.AcMoDict[account.OwnerId];
                    ownerModel.Items.Add(accountModel);
                    accountModel.Owner = ownerModel;
                }
            }
        }

        public static IEnumerable<Account> FlattenAccountTree(this KeeperDataModel dataModel)
        {
            return dataModel.AccountsTree.SelectMany(FlattenBranch);
        }

        private static IEnumerable<Account> FlattenBranch(AccountModel accountModel)
        {
            var result = new List<Account> { accountModel.Map() };
            foreach (var child in accountModel.Children)
            {
                result.AddRange(FlattenBranch(child));
            }
            return result;
        }
    }
}