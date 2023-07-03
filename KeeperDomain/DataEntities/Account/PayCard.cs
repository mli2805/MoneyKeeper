using System;

namespace KeeperDomain
{
    [Serializable]
    public class PayCard
    {
        public int Id { get; set; }
        public int MyAccountId { get; set; }

        public string Serial { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime FinishDate { get; set; }
        
        public string CardNumber { get; set; }
        public string CardHolder { get; set; }

        public bool IsMine { get; set; } // 0 - mine, 1 - julia

        public PaymentSystem PaymentSystem { get; set; }
        public bool IsVirtual { get; set; }
        public bool IsPayPass { get; set; } // if not virtual

        public string ShortName { get; set; }
        public string Comment { get; set; }

        public string Dump()
        {
            return  Id + " ; " + MyAccountId + " ; " + Serial +" ; " + 
                    $"{StartDate.Date:dd/MM/yyyy}" + " ; " + $"{FinishDate.Date:dd/MM/yyyy}" + " ; " + 
                    CardNumber + " ; " + CardHolder + " ; " + IsMine + " ; " + 
                    PaymentSystem + " ; " + IsVirtual + " ; " + IsPayPass + " ; " + 
                    ShortName + " ; " + (Comment?.Replace("\r\n", "|") ?? "");
        }

    }
}