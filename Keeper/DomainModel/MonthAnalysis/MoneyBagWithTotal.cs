namespace Keeper.DomainModel.WorkTypes
{
    public class MoneyBagWithTotal
    {
        public MoneyBag MoneyBag { get; set; }
        public decimal TotalInUsd { get; set; }
    }
}