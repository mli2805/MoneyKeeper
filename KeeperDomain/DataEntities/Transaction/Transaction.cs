using System;
using System.Globalization;

namespace KeeperDomain
{
    [Serializable]
    public class Transaction : IDumpable, IParsable<Transaction>
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
        public string Tags { get; set; }
        public string Comment { get; set; }

        public string Dump()
        {
            return Id + " ; " +
                   Timestamp.ToString("dd/MM/yyyy HH:mm") + " ; " +
                   Operation + " ; " + PaymentWay + " ; " + Receipt + " ; " +
                   MyAccount + " ; " + MySecondAccount + " ; " +
                   Amount.ToString(new CultureInfo("en-US")) + " ; " + Currency + " ; " +
                   AmountInReturn.ToString(new CultureInfo("en-US")) + " ; " + CurrencyInReturn + " ; " +
                   Tags + " ; " + Comment;
        }

        public Transaction FromString(string s)
        {
            var substrings = s.Split(';');
            Id = int.Parse(substrings[0].Trim());
            Timestamp = DateTime.ParseExact(substrings[1].Trim(), "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture);
            Operation = (OperationType)Enum.Parse(typeof(OperationType), substrings[2]);
            PaymentWay = (PaymentWay)Enum.Parse(typeof(PaymentWay), substrings[3]);
            Receipt = int.Parse(substrings[4].Trim());
            MyAccount = int.Parse(substrings[5].Trim());
            MySecondAccount = int.Parse(substrings[6].Trim());
            Amount = Convert.ToDecimal(substrings[7], new CultureInfo("en-US"));
            Currency = (CurrencyCode)Enum.Parse(typeof(CurrencyCode), substrings[8]);
            AmountInReturn = Convert.ToDecimal(substrings[9], new CultureInfo("en-US"));
            CurrencyInReturn = substrings[10].Trim() != ""
                ? (CurrencyCode)Enum.Parse(typeof(CurrencyCode), substrings[10])
                : CurrencyCode.USD;
            Tags = substrings[11].Trim();
            Comment = substrings[12].Trim();
            return this;
        }
    }
}
