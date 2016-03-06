using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Keeper.DomainModel.DbTypes;
using Keeper.DomainModel.Enumes;

namespace Keeper.DomainModel.Transactions
{
    public static class TranExtentions
    {
        public static void CloneFrom(this TranWithTags tran, TranWithTags donor)
        {
            tran.Timestamp = donor.Timestamp;
            tran.Operation = donor.Operation;
            tran.MyAccount = donor.MyAccount;
            tran.MySecondAccount = donor.MySecondAccount;
            tran.Amount = donor.Amount;
            tran.Currency = donor.Currency;
            tran.AmountInReturn = donor.AmountInReturn;
            tran.CurrencyInReturn = donor.CurrencyInReturn;
            if (donor.Tags != null)
            {
                tran.Tags = new List<Account>();
                foreach (var tag in donor.Tags)
                {
                    tran.Tags.Add(tag);
                }
            }
            tran.Comment = donor.Comment;
        }
        public static decimal AmountForAccount(this TranWithTags tran, Account account, CurrencyCodes? currency, DateTime upToDateTime)
        {
            return tran.Timestamp <= upToDateTime ? AmountForAccount(tran, account, currency) : 0;
        }

        public static decimal AmountForAccount(this TranWithTags tran, Account account, CurrencyCodes? currency)
        {
            switch (tran.Operation)
            {
                case OperationType.Доход:
                    return tran.MyAccount.Is(account) && tran.Currency == currency ? tran.Amount : 0;
                case OperationType.Расход:
                    return tran.MyAccount.Is(account) && tran.Currency == currency ? -tran.Amount : 0;
                case OperationType.Перенос:
                    if (tran.MyAccount.Is(account) && tran.Currency == currency) return -tran.Amount;
                    return tran.MySecondAccount.Is(account) && tran.Currency == currency ? tran.Amount : 0;
                case OperationType.Обмен:
                    if (tran.MyAccount.Is(account))
                    {
                        if (tran.Currency == currency) return -tran.Amount;
                        if (tran.CurrencyInReturn == currency) return tran.AmountInReturn;
                    }
                    return 0;
                case OperationType.ОбменПеренос:
                case OperationType.Форекс:
                    if (tran.MyAccount.Is(account) && tran.Currency == currency) return -tran.Amount;
                    if (tran.MySecondAccount.Is(account) && tran.CurrencyInReturn == currency) return tran.AmountInReturn;
                    return 0;
                default:
                    return 0;
            }
        }
    }
}
