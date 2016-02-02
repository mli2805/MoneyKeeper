using Keeper.DomainModel;
using Keeper.DomainModel.Transactions;

namespace Keeper.ByFunctional.BalanceEvaluating
{
    public static class TrExtensions
    {
        public static bool f(this TrBase transaction, Account account)
        {
            return true;
        }

    }
}
