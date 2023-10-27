using System.Collections.Generic;
using System.Linq;

namespace Keeper2018
{
    public static class DepoByRevocabilitySeparator
    {
        public static AccountGroups SeparateByRevocability(this KeeperDataModel dataModel, AccountItemModel folder)
        {
            var revocable = new AccountGroup("Отзывные");
            var notRevocable = new AccountGroup("Безотзывные");
            foreach (var leaf in GetFoldersTerminalLeaves(folder))
            {
                if (leaf.IsDeposit)
                {
                    var depositOffer = dataModel.DepositOffers.FirstOrDefault(o => o.Id == leaf.BankAccount.DepositOfferId);
                    if (depositOffer != null && depositOffer.IsNotRevocable)
                    {
                        notRevocable.Accounts.Add(leaf);
                        continue;
                    }
                }
                revocable.Accounts.Add(leaf);
            }

            return new AccountGroups(new List<AccountGroup>() { revocable, notRevocable });
        }


        private static List<AccountItemModel> GetFoldersTerminalLeaves(AccountItemModel folder)
        {
            var result = new List<AccountItemModel>();
            foreach (var child in folder.Children)
            {
                if (child.Children.Any())
                    result.AddRange(GetFoldersTerminalLeaves((AccountItemModel)child));
                else
                    result.Add((AccountItemModel)child);
            }
            return result;
        }
    }
}