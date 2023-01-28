using System;
using System.Linq;
using KeeperDomain;

namespace Keeper2018
{
    public static class DepositOfferModelExt
    {
        public static decimal GetCurrentRate(this DepositOfferModel depositOfferModel, DateTime openingDate, out string rateFormula)
        {
            var key = depositOfferModel.CondsMap.Keys.First();
            foreach (var condsMapKey in depositOfferModel.CondsMap.Keys.TakeWhile(condsMapKey => condsMapKey <= openingDate))
            {
                key = condsMapKey;
            }
            var conditions = depositOfferModel.CondsMap[key];

            rateFormula = depositOfferModel.RateType != RateType.Linked ? "" : conditions.RateFormula;
            return conditions.RateLines.Last().Rate;
        }
    }
}