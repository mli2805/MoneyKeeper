using System;

namespace KeeperDomain
{
    [Serializable]
    public class PayCard
    {
        public int Id { get; set; }
        public int MyAccountId { get; set; }

        public string CardNumber { get; set; }
        public string CardHolder { get; set; }

        public PaymentSystem PaymentSystem { get; set; }
        public bool IsPayPass { get; set; }

        public string Dump()
        {
            return  MyAccountId + " ; " + CardNumber + " ; " + CardHolder + " ; " + 
                    PaymentSystem + " ; " + IsPayPass;
        }

    }
}