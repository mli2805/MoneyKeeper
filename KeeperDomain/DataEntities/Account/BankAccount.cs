using System;

namespace KeeperDomain
{
    [Serializable]
    public class BankAccount
    {
        public int Id { get; set; }
        public int MyAccountId { get; set; }

        public int BankId { get; set; }
        public CurrencyCode MainCurrency { get; set; }
        public bool IsMine { get; set; } 

    }
}