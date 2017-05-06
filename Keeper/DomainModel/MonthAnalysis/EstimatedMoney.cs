using Keeper.DomainModel.Enumes;

namespace Keeper.DomainModel.MonthAnalysis
{
    public class EstimatedMoney
    {
        public decimal Amount { get; set; }
        public CurrencyCodes Currency { get; set; }
        public string ArticleName { get; set; }
    }
}