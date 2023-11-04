namespace Keeper2018
{
    public class CardBalanceMemoModel
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public decimal BalanceThreshold { get; set; }
        public decimal CurrentBalance { get; set; }
    }
}
