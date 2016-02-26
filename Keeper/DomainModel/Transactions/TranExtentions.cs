namespace Keeper.DomainModel.Transactions
{
    public static class TranExtentions
    {
        public static decimal AmountForAccount(this TranWithTags tran, Account account, CurrencyCodes? currency, Period period)
        {
            return period.ContainsAndTimeWasChecked(tran.Timestamp) ? AmountForAccount(tran, account, currency) : 0;
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
