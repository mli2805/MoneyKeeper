using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AutoMapper;
using KeeperDomain;

namespace Keeper2018
{
    public static class AccountTreeMapper
    {
        private static readonly IMapper Mapper = new MapperConfiguration(
            cfg => cfg.AddProfile<MappingEntitiesToModelsProfile>()).CreateMapper();

        public static void FillInAccountTreeAndDict(this KeeperDataModel dataModel, KeeperBin bin)
        {
            dataModel.AccountsTree = new ObservableCollection<AccountItemModel>();
            dataModel.AcMoDict = new Dictionary<int, AccountItemModel>();
            foreach (var account in bin.AccountPlaneList)
            {
                var accountItemModel = account.Map();
                dataModel.AcMoDict.Add(accountItemModel.Id, accountItemModel);

                var bankAccount = bin.BankAccounts.FirstOrDefault(b => b.Id == accountItemModel.Id);
                if (bankAccount != null)
                {
                    accountItemModel.BankAccount = Mapper.Map<BankAccountModel>(bankAccount);

                    accountItemModel.BankAccount.Deposit = bin.Deposits.FirstOrDefault(d => d.Id == accountItemModel.Id);
              
                    accountItemModel.BankAccount.PayCard = bin.PayCards.FirstOrDefault(c=>c.Id == accountItemModel.Id);
                }

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