using System;
using System.Globalization;

namespace KeeperDomain
{
    [Serializable]
    public class Transaction : IDumpable
    {
        public int Id { get; set; } //PK
        public DateTime Timestamp { get; set; }
        public OperationType Operation { get; set; }
        public PaymentWay PaymentWay { get; set; }
        public int Receipt { get; set; }
        public int MyAccount { get; set; }
        public int MySecondAccount { get; set; }
        public decimal Amount { get; set; }
        public decimal AmountInReturn { get; set; }
        public CurrencyCode Currency { get; set; }
        public CurrencyCode? CurrencyInReturn { get; set; }
        // public List<int> Tags { get; set; }
        public string Tags { get; set; }
        public string Comment { get; set; }

        public string Dump()
        {
            return Id  + " ; " +
                   Timestamp.ToString("dd/MM/yyyy HH:mm") + " ; " +
                   Operation + " ; " + PaymentWay + " ; " + Receipt + " ; " +
                   MyAccount + " ; " + MySecondAccount + " ; " +
                   Amount.ToString(new CultureInfo("en-US")) + " ; " + Currency + " ; " +
                   AmountInReturn.ToString(new CultureInfo("en-US")) + " ; " + CurrencyInReturn + " ; " +
                   Tags + " ; " + Comment;
        }
        
    }
}
