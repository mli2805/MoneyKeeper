using System.Linq;

namespace Keeper2018
{
    public static class DepoPartIdProvider
    {
        public static int GetDepoConditionsMaxId(this KeeperDataModel dataModel)
        {
            return dataModel.DepositOffers
                .SelectMany(depositOffer => depositOffer.ConditionsMap.Values)
                .ToList()
                .Max(c => c.Id);
        }

        public static int GetDepoCalcRulesMaxId(this KeeperDataModel dataModel)
        {
            return dataModel.DepositOffers
                .SelectMany(depositOffer => depositOffer.ConditionsMap.Values)
                .Select(dc => dc.CalculationRules)
                .ToList()
                .Max(c => c.Id);
        }

        public static int GetDepoRateLinesMaxId(this KeeperDataModel dataModel)
        {
            return dataModel.DepositOffers
                .SelectMany(depositOffer => depositOffer.ConditionsMap.Values)
                .SelectMany(dc=>dc.RateLines)
                .ToList()
                .Max(c => c.Id);
        }
    }
}