using System;
using System.Collections.Generic;
using System.Linq;
using KeeperDomain;

namespace Keeper2018
{
    public static class EntitiesToModels
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
            return new DepositOfferModel(depositOffer.Id)
            {
                Bank = accountPlaneList.First(a=>a.Id == depositOffer.BankId),
                Title = depositOffer.Title,
                IsNotRevocable = depositOffer.IsNotRevocable,
                MainCurrency = depositOffer.MainCurrency,
                ConditionsMap = depositOffer.ConditionsMap,
                Comment = depositOffer.Comment,
            };
        }

        public static TagAssociationModel Map(this TagAssociation tagAssociation, Dictionary<int, AccountModel> acMoDict)
        {
            return new TagAssociationModel
            {
                OperationType = tagAssociation.OperationType,
                ExternalAccount = acMoDict[tagAssociation.ExternalAccount],
                Tag = acMoDict[tagAssociation.Tag],
                Destination = tagAssociation.Destination,
            };
        }

      
        public static TransactionModel Map(this Transaction transaction, Dictionary<int, AccountModel> acMoDict, int transactionKey)
        {
            return new TransactionModel()
            {
                TransactionKey = transactionKey,
                Timestamp = transaction.Timestamp,
                Receipt = transaction.Receipt,
                Operation = transaction.Operation,
                PaymentWay = transaction.PaymentWay,
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