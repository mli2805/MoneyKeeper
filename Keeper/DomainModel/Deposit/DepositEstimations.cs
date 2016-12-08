using Keeper.DomainModel.WorkTypes;

namespace Keeper.DomainModel.Deposit
{
    public class DepositEstimations
    {
        public Period PeriodForThisMonthPayment { get; set; }
        public decimal ProcentsInThisMonth { get; set; }

        public Period PeriodForUpToEndPayment { get; set; }
        public decimal ProcentsUpToFinish { get; set; }

        public decimal DevaluationInUsd { get; set; }
        public decimal ProfitInUsd { get; set; }
    }
}