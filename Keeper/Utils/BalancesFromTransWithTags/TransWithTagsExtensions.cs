using Keeper.DomainModel.DbTypes;
using Keeper.DomainModel.Enumes;
using Keeper.DomainModel.Transactions;

namespace Keeper.Utils.BalancesFromTransWithTags
{
    public static class TransWithTagsExtensions
    {
        public static bool EitherDebitOrCreditIs(this TranWithTags tran, Account account)
        {
            return tran.MyAccount.Is(account) != tran.MySecondAccount.Is(account);
        }

        public static decimal AmountWithSign(this TranWithTags tran, Account account, CurrencyCodes currency)
        {
            switch (tran.Operation)
            {
                    case OperationType.Доход: return tran.MyAccount.Is(account) ? tran.Amount : 0;
                    case OperationType.Расход: return tran.MyAccount.Is(account) ? -tran.Amount : 0;
                    case OperationType.Перенос: return tran.MyAccount.Is(account) ? 
                                                        tran.MySecondAccount.Is(account) ? 0 : -tran.Amount : 
                                                        tran.MySecondAccount.Is(account) ? tran.Amount : 0;
                    case OperationType.Обмен:
                        if (tran.MyAccount.Is(account) && tran.Currency == currency) return -tran.Amount;
                        if (tran.MySecondAccount.Is(account) && tran.CurrencyInReturn == currency) return tran.Amount;
                        return 0;
                    default: return 0;
            }
        }
    }
}
