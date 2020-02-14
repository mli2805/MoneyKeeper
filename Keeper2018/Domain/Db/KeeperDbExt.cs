using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Keeper2018
{
    public static class KeeperDbExt
    {
        public static void FillInAccountTree(this KeeperDb db)
        {
            db.AccountsTree = new ObservableCollection<AccountModel>();
            db.AcMoDict = new Dictionary<int, AccountModel>();
            foreach (var account in db.Bin.AccountPlaneList)
            {
                var accountModel = account.Map(db.AcMoDict);
                if (account.OwnerId == 0)
                    db.AccountsTree.Add(accountModel);
            }
        }

        public static int GetAccountLevel(this KeeperDb db, Account account)
        {
            var accountModel = db.AcMoDict[account.Id];
            var level = 0;
            while (true)
            {
                if (accountModel.Owner == null) return level;
                accountModel = db.AcMoDict[accountModel.Owner.Id];
                level++;
            }
        }

        public static void FlattenAccountTree(this KeeperDb db)
        {
            db.Bin.AccountPlaneList = new List<Account>();
            foreach (var root in db.AccountsTree)
            {
                db.Bin.AccountPlaneList.AddRange(FlattenOne(root));
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

        public static Account Map(this AccountModel model)
        {
            return new Account()
            {
                Id = model.Id,
                OwnerId = model.Owner?.Id ?? 0,
                Header = (string)model.Header,
                IsFolder = model.IsFolder,
                IsExpanded = model.IsExpanded,
                Deposit = model.Deposit,
            };
        }

        public static DepositOffer Map(this DepositOfferModel depositOfferModel)
        {
            return new DepositOffer()
            {
                Id = depositOfferModel.Id,
                Bank = depositOfferModel.Bank.Id,
                Title = depositOfferModel.Title,
                IsNotRevocable = depositOfferModel.IsNotRevocable,
                MainCurrency = depositOfferModel.MainCurrency,
                Essentials = depositOfferModel.Essentials,
                Comment = depositOfferModel.Comment,
            };
        }

        public static AccountGroups SeparateByRevocability(this KeeperDb db, AccountModel folder)
        {
            var revocable = new AccountGroup("Отзывные");
            var notRevocable = new AccountGroup("Безотзывные");
            foreach (var leaf in GetFoldersTerminalLeaves(folder))
            {
                if (leaf.IsDeposit)
                {
                    var depositOffer = db.Bin.DepositOffers.FirstOrDefault(o => o.Id == leaf.Deposit.DepositOfferId);
                    if (depositOffer != null && depositOffer.IsNotRevocable)
                    {
                        notRevocable.Accounts.Add(leaf);
                        continue;
                    }
                }
                revocable.Accounts.Add(leaf);
            }

            return new AccountGroups(new List<AccountGroup>(){revocable, notRevocable});
        }

        public static AccountModelGroups SortFoldersTerminalChildren(this KeeperDb db, AccountModel folder)
        {
            var result = new AccountModelGroups();
            foreach (var leaf in GetFoldersTerminalLeaves(folder))
            {
                if (leaf.IsDeposit)
                {
                    var depositOffer = db.Bin.DepositOffers.FirstOrDefault(o => o.Id == leaf.Deposit.DepositOfferId);
                    if (depositOffer != null && depositOffer.IsNotRevocable)
                    {
                        result.NotRevocable.Add(leaf);
                        continue;
                    }
                }
                result.Revocable.Add(leaf);
            }
            return result;
        }

        private static List<AccountModel> GetFoldersTerminalLeaves(AccountModel folder)
        {
            var result = new List<AccountModel>();
            foreach (var child in folder.Children)
            {
                if (child.IsFolder)
                    result.AddRange(GetFoldersTerminalLeaves(child));
                else
                    result.Add(child);
            }
            return result;
        }
    }
}