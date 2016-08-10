namespace Keeper.DomainModel.WorkTypes
{
    public class ExtendedBalance
    {
        public MoneyBagWithTotal Common { get; set; }
        public MoneyBagWithTotal OnHands { get; set; }
        public MoneyBagWithTotal OnDeposits { get; set; }

        public ExtendedBalance()
        {
            Common = new MoneyBagWithTotal();
            OnHands = new MoneyBagWithTotal();
            OnDeposits = new MoneyBagWithTotal();
        }
    }
}