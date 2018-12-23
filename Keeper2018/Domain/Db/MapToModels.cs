using System;
using System.Collections.Generic;
using System.Linq;

namespace Keeper2018
{
    public static class MapToModels
    {
        public static AccountModel Map(this Account account, Dictionary<int, AccountModel> acMoDict)
        {
            var accountModel = new AccountModel(account.Header)
            {
                Id = account.Id,
                IsExpanded = account.IsExpanded,
                IsFolder = account.IsFolder,
                Deposit = account.Deposit,
            };

            try
            {
                acMoDict.Add(accountModel.Id, accountModel);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            if (account.OwnerId != 0)
            {
                var ownerModel = acMoDict[account.OwnerId];
                ownerModel.Items.Add(accountModel);
                accountModel.Owner = ownerModel;
            }

            return accountModel;
        }

        public static DepositOfferModel Map(this DepositOffer depositOffer, List<Account> accountPlaneList)
        {
            return new DepositOfferModel
            {
                Id = depositOffer.Id,
                Bank = accountPlaneList.First(a=>a.Id == depositOffer.Bank),
                Title = depositOffer.Title,
                MainCurrency = depositOffer.MainCurrency,
                Essentials = depositOffer.Essentials,
                Comment = depositOffer.Comment,
            };
        }

        public static LineModel Map(this TagAssociation tagAssociation, Dictionary<int, AccountModel> acMoDict)
        {
            return new LineModel
            {
                OperationType = tagAssociation.OperationType,
                ExternalAccount = acMoDict[tagAssociation.ExternalAccount].Name,
                Tag = acMoDict[tagAssociation.Tag].Name,
                Destination = tagAssociation.Destination,
            };
        }

        public static TransactionModel Map(this Transaction transaction, Dictionary<int, AccountModel> acMoDict)
        {
            return new TransactionModel()
            {
                Timestamp = transaction.Timestamp,
                OrdinalInDate = transaction.OrdinalInDate,
                Operation = transaction.Operation,
                MyAccount = acMoDict[transaction.MyAccount],
                MySecondAccount = transaction.MySecondAccount == -1 ? null : acMoDict[transaction.MySecondAccount],
                Amount = transaction.Amount,
                AmountInReturn = transaction.AmountInReturn,
                Currency = transaction.Currency,
                CurrencyInReturn = transaction.CurrencyInReturn,
                Tags = transaction.Tags.Select(t => acMoDict[t]).ToList(),
                Comment = transaction.Comment,
            };
        }
    }
}