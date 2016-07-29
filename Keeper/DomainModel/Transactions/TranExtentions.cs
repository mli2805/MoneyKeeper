using System;
using System.Collections.Generic;
using System.Windows.Media;
using Keeper.DomainModel.DbTypes;
using Keeper.DomainModel.Enumes;
using Keeper.DomainModel.WorkTypes;
using Keeper.Utils.BalanceEvaluating.Ilya;

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


        #region Decimal functions
        public static decimal AmountForAccount(this TranWithTags tran, Account account, CurrencyCodes? currency, Period period)
        {
            return period.ContainsButTimeNotChecking(tran.Timestamp) ? AmountForAccount(tran, account, currency) : 0;
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
                    if (tran.MyAccount.Is(account) && tran.Currency == currency) return -tran.Amount;
                    if (tran.MySecondAccount.Is(account) && tran.CurrencyInReturn == currency) return tran.AmountInReturn;
                    return 0;
                default:
                    return 0;
            }
        }

        #endregion

        #region MoneyBag functions
        public static MoneyBag MoneyBagForAccount(this TranWithTags tran, Account account, Period period)
        {
            return period.ContainsButTimeNotChecking(tran.Timestamp) ? MoneyBagForAccount(tran, account) : null;
        }
        public static MoneyBag MoneyBagForAccount(this TranWithTags tran, Account account, DateTime upToDateTime)
        {
            return tran.Timestamp <= upToDateTime ? MoneyBagForAccount(tran, account) : null;
        }
        public static MoneyBag MoneyBagForAccount(this TranWithTags tran, Account account)
        {
            var result = new MoneyBag();
            switch (tran.Operation)
            {
                case OperationType.Доход:
                    return tran.MyAccount.Is(account)  ? new MoneyBag(new Money(tran.Currency.GetValueOrDefault(), tran.Amount)) : null;
                case OperationType.Расход:
                    return tran.MyAccount.Is(account)  ? new MoneyBag(new Money(tran.Currency.GetValueOrDefault(), -tran.Amount)) : null;
                case OperationType.Перенос:
                    if (tran.MyAccount.Is(account)) result = result - new Money(tran.Currency.GetValueOrDefault(), tran.Amount);
                    if (tran.MySecondAccount.Is(account)) result = result + new Money(tran.Currency.GetValueOrDefault(), tran.Amount);
                    return result;
                case OperationType.Обмен:
                    if (tran.MyAccount.Is(account)) result = result - new Money(tran.Currency.GetValueOrDefault(), tran.Amount);
                    if (tran.MySecondAccount.Is(account)) result = result + new Money(tran.CurrencyInReturn.GetValueOrDefault(), tran.AmountInReturn);
                    return result;
                default:
                    return null;
            }
        }
#endregion

        public static Brush TranFontColor(this TranWithTags tran)
        {
            if (tran.Operation == OperationType.Доход) return Brushes.Blue;
            if (tran.Operation == OperationType.Расход) return Brushes.Red;
            if (tran.Operation == OperationType.Перенос) return Brushes.Black;
            if (tran.Operation == OperationType.Обмен) return Brushes.Green;
            return Brushes.Gray;
        }
    }
}
