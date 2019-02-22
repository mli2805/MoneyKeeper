using System;
using System.Collections.Generic;
using System.Globalization;

namespace Keeper2018
{
    [Serializable]
    public class Transaction
    {
        public DateTime Timestamp;
        public OperationType Operation;
        public int Receipt;
        public int MyAccount;
        public int MySecondAccount;
        public decimal Amount;
        public decimal AmountInReturn;
        public CurrencyCode Currency;
        public CurrencyCode? CurrencyInReturn;
        public List<int> Tags;
        public string Comment;

        public string Dump()
        {
           // return Convert.ToString(Timestamp, new CultureInfo("ru-RU")) + " ; " +
            return Timestamp.ToString("dd/MM/yyyy HH:mm") + " ; " +
                   Operation + " ; " + Receipt + " ; " +
                   MyAccount + " ; " + MySecondAccount + " ; " +
                   Amount.ToString(new CultureInfo("en-US")) + " ; " + Currency + " ; " +
                   AmountInReturn.ToString(new CultureInfo("en-US")) + " ; " + CurrencyInReturn + " ; " +
                   Dump(Tags) + " ; " + Comment;
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
