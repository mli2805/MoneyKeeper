using System;
using System.Collections.Generic;
using System.Globalization;

namespace KeeperDomain
{
    [Serializable]
    public class Transaction
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

        private string Dump(List<int> tags)
        {
            if (tags == null || tags.Count == 0) return " ";
            string result = "";
            foreach (var t in tags)
            {
                result = result + t + " | ";
            }
            result = result.Substring(0, result.Length - 3);
            return result;
        }

    }
}
