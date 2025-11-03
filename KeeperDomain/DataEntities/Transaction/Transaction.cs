using System;
using System.ComponentModel.DataAnnotations;
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
        public int Counterparty { get; set; }
        public int Category { get; set; }
        public decimal Amount { get; set; }
        public decimal AmountInReturn { get; set; }
        public CurrencyCode Currency { get; set; }
        public CurrencyCode? CurrencyInReturn { get; set; }
        [MaxLength(100)] public string Tags { get; set; }
        [MaxLength(250)] public string Comment { get; set; }

        public string Dump()
        {
            return Id + " ; " +
                   Timestamp.ToString("dd/MM/yyyy HH:mm") + " ; " +
                   Operation + " ; " + PaymentWay + " ; " + Receipt + " ; " +
                   MyAccount + " ; " + MySecondAccount + " ; " +
                   Counterparty + " ; " + Category + " ; " +
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
            Counterparty = int.Parse(substrings[7].Trim());
            Category = int.Parse(substrings[8].Trim());
            Amount = Convert.ToDecimal(substrings[9], new CultureInfo("en-US"));
            Currency = (CurrencyCode)Enum.Parse(typeof(CurrencyCode), substrings[10]);
            AmountInReturn = Convert.ToDecimal(substrings[11], new CultureInfo("en-US"));
            CurrencyInReturn = substrings[12].Trim() != ""
                ? (CurrencyCode)Enum.Parse(typeof(CurrencyCode), substrings[12])
                : CurrencyCode.USD;
            Tags = substrings[13].Trim();
            Comment = substrings[14].Trim();
            return this;
        }
    }
}
