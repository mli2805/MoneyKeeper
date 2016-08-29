namespace Keeper.DomainModel.MonthAnalysis
{
    public class ExtendedIncomes
    {
        public ExtendedTraffic OnDeposits { get; set; }
        public ExtendedTraffic OnHands { get; set; }
        public decimal TotalInUsd { get { return OnDeposits.TotalInUsd + OnHands.TotalInUsd; } }

        public ExtendedIncomes()
        {
            OnHands = new ExtendedTraffic();
            OnDeposits = new ExtendedTraffic();
        }
    }
}