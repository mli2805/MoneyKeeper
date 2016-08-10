using Keeper.DomainModel.WorkTypes;

namespace Keeper.DomainModel.MonthAnalysis
{
    public class MoneyBagWithTotal
    {
        public MoneyBag MoneyBag { get; set; }
        public decimal TotalInUsd { get; set; }
    }
}