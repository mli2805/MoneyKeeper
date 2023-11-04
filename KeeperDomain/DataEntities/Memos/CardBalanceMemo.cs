using System;

namespace KeeperDomain
{
    [Serializable]
    public class CardBalanceMemo
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public decimal BalanceThreshold { get; set; }
    }
}
