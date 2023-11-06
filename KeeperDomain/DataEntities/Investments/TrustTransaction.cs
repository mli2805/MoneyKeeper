using System;
using System.Globalization;

namespace KeeperDomain
{
    [Serializable]
    public class TrustTransaction : IDumpable, IParsable<TrustTransaction>
    {
        public int Id { get; set; }
        public InvestOperationType InvestOperationType { get; set; }
        public DateTime Timestamp { get; set; }

        public int AccountId { get; set; }
        public int TrustAccountId { get; set; }

        public decimal CurrencyAmount { get; set; }
        public decimal CouponAmount { get; set; }
        public CurrencyCode Currency { get; set; }

        public int AssetAmount { get; set; }
        public int AssetId { get; set; }

        public decimal PurchaseFee { get; set; }
        public CurrencyCode PurchaseFeeCurrency { get; set; } = CurrencyCode.BYN;
        public int FeePaymentOperationId { get; set; }

        public string Comment { get; set; }

        public string Dump()
        {
            return Id + " ; " + InvestOperationType + " ; " + Timestamp.ToString("dd/MM/yyyy HH:mm") + " ; " + 
                   AccountId + " ; " + TrustAccountId + " ; " +
                   CurrencyAmount.ToString(new CultureInfo("en-US")) + " ; " + 
                   CouponAmount.ToString(new CultureInfo("en-US")) + " ; " + Currency + " ; " +
                   AssetAmount.ToString(new CultureInfo("en-US")) + " ; " + AssetId + " ; " +
                   PurchaseFee.ToString(new CultureInfo("en-US")) + " ; " + PurchaseFeeCurrency + " ; " + FeePaymentOperationId + " ; " + 
                   Comment.Trim();
        }

        public TrustTransaction FromString(string s)
        {
            var substrings = s.Split(';');
            Id = int.Parse(substrings[0]);
            InvestOperationType = (InvestOperationType)Enum.Parse(typeof(InvestOperationType), substrings[1]);
            Timestamp = DateTime.ParseExact(substrings[2].Trim(), "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture);
            AccountId = int.Parse(substrings[3]);
            TrustAccountId = int.Parse(substrings[4]);

            CurrencyAmount = decimal.Parse(substrings[5], new CultureInfo("en-US"));
            CouponAmount = decimal.Parse(substrings[6], new CultureInfo("en-US"));
            Currency = (CurrencyCode)Enum.Parse(typeof(CurrencyCode), substrings[7]);

            AssetAmount = int.Parse(substrings[8]);
            AssetId = int.Parse(substrings[9]);

            PurchaseFee = decimal.Parse(substrings[10], new CultureInfo("en-US"));
            PurchaseFeeCurrency = (CurrencyCode)Enum.Parse(typeof(CurrencyCode), substrings[11]);
            FeePaymentOperationId = int.Parse(substrings[12]);

            Comment = substrings[13].Trim();
            return this;
        }
    }
}