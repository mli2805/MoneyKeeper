using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Keeper2018
{
    public static class KeeperDbExt
    {
        public static void AssociationsToModels(this KeeperDb db)
        {
            db.AssociationModels = new ObservableCollection<TagAssociationModel>();
            foreach (var tagAssociation in db.Bin.TagAssociations)
            {
                db.AssociationModels.Add(CreateAssociationModel(db, tagAssociation));
            }
        }

        private static TagAssociationModel CreateAssociationModel(this KeeperDb db, TagAssociation tagAssociation)
        {
            return new TagAssociationModel
            {
                OperationType = tagAssociation.OperationType,
                ExternalAccount = db.AcMoDict[tagAssociation.ExternalAccount],
                Tag = db.AcMoDict[tagAssociation.Tag],
                Destination = tagAssociation.Destination,
            };
        }

        public static void TransToModels(this KeeperDb db)
        {
            db.TransactionModels = new ObservableCollection<TransactionModel>();
            foreach (var transaction in db.Bin.Transactions)
            {
                db.TransactionModels.Add(CreateTransModel(db, transaction));
            }
        }

        private static TransactionModel CreateTransModel(this KeeperDb db, Transaction transaction)
        {
            return new TransactionModel()
            {
                Timestamp = transaction.Timestamp,
                Operation = transaction.Operation,
                MyAccount = db.AcMoDict[transaction.MyAccount],
                MySecondAccount = transaction.MySecondAccount == -1 ? null : db.AcMoDict[transaction.MySecondAccount],
                Amount = transaction.Amount,
                AmountInReturn = transaction.AmountInReturn,
                Currency = transaction.Currency,
                CurrencyInReturn = transaction.CurrencyInReturn,
                Tags = transaction.Tags.Select(t => db.AcMoDict[t]).ToList(),
                Comment = transaction.Comment,
            };
        }


        public static void FillInTheTree(this KeeperDb db)
        {
            db.AccountsTree = new ObservableCollection<AccountModel>();
            db.AcMoDict = new Dictionary<int, AccountModel>();
            foreach (var account in db.Bin.AccountPlaneList)
            {
                var accountModel = new AccountModel(account.Header)
                {
                    Id = account.Id,
                    IsExpanded = account.IsExpanded,
                };
                db.AcMoDict.Add(accountModel.Id, accountModel);

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
            db.Bin.AccountPlaneList = new List<Account>();
            foreach (var root in db.AccountsTree)
            {
                db.Bin.AccountPlaneList.AddRange(FlattenOne(root));
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