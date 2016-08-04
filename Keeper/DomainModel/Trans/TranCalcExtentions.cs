using System;
using Keeper.DomainModel.DbTypes;
using Keeper.DomainModel.Enumes;
using Keeper.DomainModel.WorkTypes;
using Keeper.Utils.BalanceEvaluating.Ilya;

namespace Keeper.DomainModel.Trans
{
    static class TranCalcExtentions
    {
        #region Decimal functions for Account
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
        #region Decimal functions for Tag
        public static decimal AmountForTag(this TranWithTags tran, Account tag, CurrencyCodes? currency, Period period)
        {
            return period.ContainsButTimeNotChecking(tran.Timestamp) ? AmountForAccount(tran, tag, currency) : 0;
        }
        public static decimal AmountForTag(this TranWithTags tran, Account tag, CurrencyCodes? currency, DateTime upToDateTime)
        {
            return tran.Timestamp <= upToDateTime ? AmountForAccount(tran, tag, currency) : 0;
        }
        public static decimal AmountForTag(this TranWithTags tran, Account tag, CurrencyCodes? currency)
        {
            switch (tran.Operation)
            {
                case OperationType.Доход:
                    return tran.CollectionContainsTag(tag) && tran.Currency == currency ? tran.Amount : 0;
                case OperationType.Расход:
                    return tran.CollectionContainsTag(tag) && tran.Currency == currency ? -tran.Amount : 0;
                case OperationType.Перенос:
                    return 0;
                case OperationType.Обмен:
                    return 0;
                default:
                    return 0;
            }
        }

        #endregion

        #region MoneyBag functions
        public static MoneyBag MoneyBagForTag(this TranWithTags tran, Account tag, Period period)
        {
            return period.ContainsButTimeNotChecking(tran.Timestamp) ? MoneyBagForTag(tran, tag) : null;
        }
        public static MoneyBag MoneyBagForTag(this TranWithTags tran, Account tag, DateTime upToDateTime)
        {
            return tran.Timestamp <= upToDateTime ? MoneyBagForTag(tran, tag) : null;
        }
        public static MoneyBag MoneyBagForTag(this TranWithTags tran, Account tag)
        {
            if (!tran.CollectionContainsTag(tag)) return null;
            var result = new MoneyBag();
            switch (tran.Operation)
            {
                    case OperationType.Доход:
                        return new MoneyBag(new Money(tran.Currency.GetValueOrDefault(), tran.Amount));
                    case OperationType.Расход:
                        return new MoneyBag(new Money(tran.Currency.GetValueOrDefault(), -tran.Amount));
                    case OperationType.Перенос:
                        return null;
                    case OperationType.Обмен:
                        result = result - new Money(tran.Currency.GetValueOrDefault(), tran.Amount);
                        result = result + new Money(tran.CurrencyInReturn.GetValueOrDefault(), tran.AmountInReturn);
                        return result;
                    default:
                        return null;
            }
        }

        public static bool CollectionContainsTag(this TranWithTags tran, Account tag)
        {
            if (tran.Tags.Contains(tag)) return true;
            foreach (var child in tag.Children)
            {
                if (tran.Tags.Contains(child)) return true;
            }
            return false;
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
                    return tran.MyAccount.Is(account) ? new MoneyBag(new Money(tran.Currency.GetValueOrDefault(), tran.Amount)) : null;
                case OperationType.Расход:
                    return tran.MyAccount.Is(account) ? new MoneyBag(new Money(tran.Currency.GetValueOrDefault(), -tran.Amount)) : null;
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
    }
}
