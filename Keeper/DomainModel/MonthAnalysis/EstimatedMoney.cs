using Keeper.DomainModel.Enumes;

namespace Keeper.DomainModel.WorkTypes
{
    public class EstimatedMoney
    {
        public decimal Amount { get; set; }
        public CurrencyCodes Currency { get; set; }
        public string ArticleName { get; set; }
    }
}