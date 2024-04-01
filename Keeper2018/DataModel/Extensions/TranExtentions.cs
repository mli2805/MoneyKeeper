using System.Collections.Generic;

namespace Keeper2018
{
    public static class TranExtentions
    {
        public static TransactionModel Clone(this TransactionModel tran)
        {
            var result = new TransactionModel
            {
                Id = tran.Id,
                Timestamp = tran.Timestamp,
                Operation = tran.Operation,
                PaymentWay = tran.PaymentWay,
                MyAccount = tran.MyAccount,
                MySecondAccount = tran.MySecondAccount,
                Counterparty = tran.Counterparty,
                Category = tran.Category,
                Amount = tran.Amount,
                Currency = tran.Currency,
                AmountInReturn = tran.AmountInReturn,
                CurrencyInReturn = tran.CurrencyInReturn
            };
            if (tran.Tags != null)
            {
                result.Tags = new List<AccountItemModel>();
                foreach (var tag in tran.Tags)
                {
                    result.Tags.Add(tag);
                }
            }
            result.Comment = tran.Comment;
            return result;
        }

        public static void CopyInto(this TransactionModel tran, TransactionModel destinationTran)
        {
            destinationTran.Id = tran.Id;
            destinationTran.Timestamp = tran.Timestamp;
            destinationTran.Operation = tran.Operation;
            destinationTran.PaymentWay = tran.PaymentWay;
            destinationTran.MyAccount = tran.MyAccount;
            destinationTran.MySecondAccount = tran.MySecondAccount;
            destinationTran.Counterparty = tran.Counterparty;
            destinationTran.Category = tran.Category;
            destinationTran.Amount = tran.Amount;
            destinationTran.Currency = tran.Currency;
            destinationTran.AmountInReturn = tran.AmountInReturn;
            destinationTran.CurrencyInReturn = tran.CurrencyInReturn;

            destinationTran.Tags?.Clear();
            if (tran.Tags != null)
            {
                if (destinationTran.Tags == null) destinationTran.Tags = new List<AccountItemModel>();
                foreach (var tag in tran.Tags)
                {
                    destinationTran.Tags.Add(tag);
                }
            }
            destinationTran.Comment = tran.Comment;
        }
    }
}
