using System;

namespace KeeperDomain
{
    [Serializable]
    public class PayCard : IDumpable, IParsable<PayCard>
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

        public PayCard FromString(string s)
        {
            var substrings = s.Split(';');
            Id = int.Parse(substrings[0]);
         
            CardNumber = substrings[1].Trim();
            CardHolder = substrings[2].Trim();
            PaymentSystem = (PaymentSystem)Enum.Parse(typeof(PaymentSystem), substrings[3]);
            IsVirtual = Convert.ToBoolean(substrings[4]);
            IsPayPass = Convert.ToBoolean(substrings[5]);

            return this;
        }
    }
}