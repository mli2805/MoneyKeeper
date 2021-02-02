using System.Collections.Generic;
using System.Linq;
using KeeperDomain;

namespace Keeper2018
{
    public static class EntitiesToModels
    {
        public static AccountModel Map(this Account account)
        {
            return new AccountModel(account.Header)
            {
                Id = account.Id,
                IsExpanded = account.IsExpanded,
                IsFolder = account.IsFolder,
                Deposit = account.Deposit,
            };
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