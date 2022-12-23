using System;

namespace KeeperDomain
{
    [Serializable]
    public class PayCard
    {
        public int Id { get; set; }
        public int DepositId { get; set; }

        public string CardNumber { get; set; }
        public string CardHolder { get; set; }

      //  public int CardOwner { get; set; }
        public bool IsMine { get; set; } // 0 - mine, 1 - julia

        public PaymentSystem PaymentSystem { get; set; }
        public bool IsVirtual { get; set; }
        public bool IsPayPass { get; set; } // if not virtual

        public string Dump()
        {
            return  Id + " ; " + DepositId + " ; " + CardNumber + " ; " + CardHolder + " ; " + IsMine + " ; " + 
                    PaymentSystem + " ; " + IsVirtual + " ; " + IsPayPass;
        }

    }
}