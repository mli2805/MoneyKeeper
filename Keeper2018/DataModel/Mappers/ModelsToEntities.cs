using System.Linq;
using KeeperDomain;

namespace Keeper2018
{
    public static class ModelsToEntities
    {
        public static Transaction Map(this TransactionModel transactionModel)
        {
            return new Transaction()
            {
                Timestamp = transactionModel.Timestamp,
                Receipt = transactionModel.Receipt,
                Operation = transactionModel.Operation,
                PaymentWay = transactionModel.PaymentWay,
                MyAccount = transactionModel.MyAccount.Id,
                MySecondAccount = transactionModel.MySecondAccount?.Id ?? -1,
                Amount = transactionModel.Amount,
                AmountInReturn = transactionModel.AmountInReturn,
                Currency = transactionModel.Currency,
                CurrencyInReturn = transactionModel.CurrencyInReturn,
                Tags = transactionModel.Tags.Select(t => t.Id).ToList(),
                Comment = transactionModel.Comment,
            };
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
                BankId = depositOfferModel.Bank.Id,
                Title = depositOfferModel.Title,
                IsNotRevocable = depositOfferModel.IsNotRevocable,
                MainCurrency = depositOfferModel.MainCurrency,
                ConditionsMap = depositOfferModel.ConditionsMap,
                Comment = depositOfferModel.Comment,
            };
        }
    }
}