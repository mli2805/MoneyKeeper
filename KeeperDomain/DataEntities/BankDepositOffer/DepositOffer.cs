using System;

namespace KeeperDomain
{
    [Serializable]
    public class DepositOffer
    {
        public int Id { get; set; } //PK
        public int BankId { get; set; }
        public string Title { get; set; }
        public bool IsNotRevocable { get; set; }
        public CurrencyCode MainCurrency { get; set; }

        public string Comment { get; set; }

        public string Dump()
        {
            return Id + " ; " + BankId + " ; " + Title + " ; " + IsNotRevocable + " ; " + MainCurrency + " ; " + Comment;
        }
    }
}