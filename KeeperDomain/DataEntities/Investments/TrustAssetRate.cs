using System;
using System.Globalization;

namespace KeeperDomain
{
    [Serializable]
    public class TrustAssetRate : IDumpable, IParsable<TrustAssetRate>
    {
        public int Id { get; set; }
        public int TickerId { get; set; } = 1;
        public DateTime Date { get; set; } = DateTime.Today;
        public decimal Value { get; set; }
        public int Unit { get; set; } = 1;
        public CurrencyCode Currency { get; set; } = CurrencyCode.USD;


        public string Dump()
        {
            return Id + " ; " + TickerId + " ; " + Date.ToString("dd/MM/yyyy") + " ; " + Unit + " ; " +
                   Value.ToString(new CultureInfo("en-US")) + " ; " + Currency;
        }

        public TrustAssetRate FromString(string s)
        {
            var substrings = s.Split(';');
            Id = int.Parse(substrings[0]);
            TickerId = int.Parse(substrings[1]);
            Date = DateTime.ParseExact(substrings[2].Trim(), "dd.MM.yyyy", CultureInfo.InvariantCulture);
            Unit = int.Parse(substrings[3]);
            Value = decimal.Parse(substrings[4], new CultureInfo("en-US"));
            Currency = (CurrencyCode)Enum.Parse(typeof(CurrencyCode), substrings[5]);
            return this;
        }
    }
}