namespace Keeper.DomainModel.Deposit
{
    public class DepositEstimations
    {
        public Period PeriodForThisMonthPayment { get; set; }
        public decimal ProcentsInThisMonth { get; set; }
        public decimal CurrencyRateOnThisMonthPayment { get; set; }

        public Period PeriodForUpToEndPayment { get; set; }
        public decimal ProcentsUpToFinish { get; set; }
        public decimal CurrencyRateOnFinish { get; set; }

        public decimal DevaluationInUsd { get; set; }
        public decimal ProfitInUsd { get; set; }
    }
}