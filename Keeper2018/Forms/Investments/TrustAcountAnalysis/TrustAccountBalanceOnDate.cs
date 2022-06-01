using System;
using System.Collections.Generic;
using System.Linq;

namespace Keeper2018
{
    public class TrustAccountBalanceOnDate
    {
        public DateTime Date { get; set; }
        public decimal BaseFee { get; set; }
        public decimal TopUp { get; set; }
        public decimal Withdraw { get; set; }
        public decimal Cash { get; set; }
        public decimal NotPaidFees { get; set; }
        public List<InvestmentAssetOnDate> Assets { get; } = new List<InvestmentAssetOnDate>();

        public TrustAccountBalanceOnDate()
        {
        }

        public TrustAccountBalanceOnDate(TrustAccountBalanceOnDate source)
        {
            Date = source.Date;
            Cash = source.Cash;
            Assets = new List<InvestmentAssetOnDate>(source.Assets.Select(a=>a.ShallowCopy()));
        }
    }
}