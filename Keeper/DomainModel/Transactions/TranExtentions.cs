using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Keeper.DomainModel.DbTypes;
using Keeper.DomainModel.Enumes;

namespace Keeper.DomainModel.Transactions
{
    public static class TranExtentions
    {
        public static TranWithTags Clone(this TranWithTags tran)
        {
            var result = new TranWithTags
            {
                Timestamp = tran.Timestamp,
                Operation = tran.Operation,
                MyAccount = tran.MyAccount,
                MySecondAccount = tran.MySecondAccount,
                Amount = tran.Amount,
                Currency = tran.Currency,
                AmountInReturn = tran.AmountInReturn,
                CurrencyInReturn = tran.CurrencyInReturn
            };
            if (tran.Tags != null)
            {
                result.Tags = new List<Account>();
                foreach (var tag in tran.Tags)
                {
                    result.Tags.Add(tag);
                }
            }
            result.Comment = tran.Comment;
            return result;
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
