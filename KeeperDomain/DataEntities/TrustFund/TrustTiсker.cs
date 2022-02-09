using System;
using System.Globalization;

namespace KeeperDomain
{
    [Serializable]
    public class TrustTiсker
    {
        public int Id { get; set; }

        public string Ticker { get; set; }
        public string Title { get; set; }
        public SecuritiesType SecuritiesType { get; set; }

        public double BondCoupon { get; set; }
        public DateTime BondExpirationDate { get; set; } = DateTime.MaxValue;

        public string Comment { get; set; }

        public string Dump()
        {
            return Id + " ; " + Ticker + " ; " + Title + " ; " + SecuritiesType + " ; " + 
                   BondCoupon.ToString(new CultureInfo("en-US")) + " ; " + 
                   BondExpirationDate.ToString("dd/MM/yyyy") + " ; " + Comment;
        }
    }
}