using System;

namespace Keeper2018
{
    [Serializable]
    public class PayCard
    {
        public int MyAccountId;

        public string CardNumber { get; set; }
        public string CardHolder { get; set; }

        public PaymentSystem PaymentSystem { get; set; }
        public bool IsPayPass { get; set; }

        public string Dump()
        {
            return  MyAccountId + " ; " + CardNumber + " ; " + 
                    PaymentSystem + " ; " + IsPayPass;
        }

    }
}