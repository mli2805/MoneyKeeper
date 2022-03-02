using System;
using System.Collections.Generic;
using System.Linq;

namespace Keeper2018
{
    public class TrustAccountBalanceOnDate
    {
        public DateTime Date { get; set; }
        public decimal Cash { get; set; }
        public List<InvestmentAssetEvaluation> Assets { get; } = new List<InvestmentAssetEvaluation>();

        public TrustAccountBalanceOnDate()
        {
        }

        public TrustAccountBalanceOnDate(TrustAccountBalanceOnDate source)
        {
            Date = source.Date;
            Cash = source.Cash;
            Assets = new List<InvestmentAssetEvaluation>(source.Assets.Select(a=>a.ShallowCopy()));
        }
    }
}