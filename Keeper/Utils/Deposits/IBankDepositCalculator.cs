using Keeper.DomainModel;
using Keeper.DomainModel.Deposit;

namespace Keeper.Utils.Deposits
{
    public interface IBankDepositCalculator
    {
        decimal GetThisMonthEstimatedProcents(Deposit deposit);
        decimal GetUpToEndEstimatedProcents(Deposit deposit);
        decimal GetEstimatedProcentsForPeriod(Deposit deposit, Period period);
    }
}
