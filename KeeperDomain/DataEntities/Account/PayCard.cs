using System;

namespace KeeperDomain
{
    [Serializable]
    public class PayCard
    {
        public int Id { get; set; } // совпадает с ID Account'a и BankAccount'a
        
        public string CardNumber { get; set; }
        public string CardHolder { get; set; }

        public PaymentSystem PaymentSystem { get; set; }
        public bool IsVirtual { get; set; }
        public bool IsPayPass { get; set; } // if not virtual


        public string Dump()
        {
            return  Id + " ; " + 
                    CardNumber + " ; " + CardHolder + " ; " + 
                    PaymentSystem + " ; " + IsVirtual + " ; " + IsPayPass;
        }

    }
}