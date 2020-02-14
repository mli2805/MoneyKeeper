using System.Collections.Generic;
using System.Collections.ObjectModel;

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
    }
}