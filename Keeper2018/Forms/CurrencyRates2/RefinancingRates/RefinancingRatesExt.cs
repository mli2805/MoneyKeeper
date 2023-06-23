using System.Linq;
using KeeperDomain;

namespace Keeper2018
{
    public static class RefinancingRatesExt
    {
        public static void UpdateDepoRatesLinkedToCp(this KeeperDataModel keeperDataModel)
        {
            var id = keeperDataModel.GetDepoRateLinesMaxId();

            foreach (var depositOfferModel in 
                     keeperDataModel.DepositOffers.Where(o => o.RateType == RateType.Linked))
            {
                // русские буквы СР
                foreach (var conditions in 
                         depositOfferModel.CondsMap.Values.Where(c=>c.RateFormula.Contains("СР")))
                {
                    id = UpdateRateLinesInConditions(keeperDataModel, conditions, id);
                }
            }
        }

        public static int UpdateRateLinesInConditions(this KeeperDataModel keeperDataModel, DepoCondsModel conditions, int id)
        {
            // ставка принятая на момент начала депозита
            if (conditions.RateLines.Count == 0)
            {
                var lastBeforeDepositStarts =
                    keeperDataModel.RefinancingRates.Last(r => r.Date.Date <= conditions.DateFrom.Date);
                var depositRateLine = CreateDepositRateLine(lastBeforeDepositStarts, conditions, ++id);
                depositRateLine.DateFrom = conditions.DateFrom;
                conditions.RateLines.Add(depositRateLine);
            }

            var lastDate = conditions.RateLines.Last().DateFrom;
            foreach (var refinancingRate in keeperDataModel.RefinancingRates.Where(r => r.Date.Date > lastDate))
            {
                conditions.RateLines.Add(CreateDepositRateLine(refinancingRate, conditions, ++id));
            }

            return id;
        }

        private static DepositRateLine CreateDepositRateLine(RefinancingRate refinancingRate, DepoCondsModel conditions, int id)
        {
            return new DepositRateLine()
            {
                Id = id,
                DepositOfferConditionsId = conditions.Id,
                DateFrom = refinancingRate.Date,
                AmountFrom = 0,
                AmountTo = 999_999_999_999,
                Rate = (decimal)RateFormula.Calculate(conditions.RateFormula, refinancingRate.Value),
            };
        }
    }
}