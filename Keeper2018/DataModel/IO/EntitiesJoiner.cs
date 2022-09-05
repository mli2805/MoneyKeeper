using System.Collections.Generic;
using System.Linq;
using KeeperDomain;

namespace Keeper2018
{
    public static class EntitiesJoiner
    {
        public static List<CarModel> JoinCarParts(this KeeperBin bin)
        {
            var result = bin.Cars.Select(c => c.Map()).ToList();
            foreach (var car in result)
            {
                var arr = bin.YearMileages.Where(l => l.CarId == car.Id).ToArray();
                var prev = car.PurchaseMileage;
                foreach (var t in arr)
                {
                    var cc = t.Map();
                    cc.Mileage = t.Odometer - prev;
                    car.YearsMileage.Add(cc);
                    prev = t.Odometer;
                }
            }
            return result;
        }

        public static List<DepositOfferModel> JoinDepoParts(this KeeperBin bin, Dictionary<int, AccountModel> acMoDict)
        {
            var result = bin.DepositOffers.Select(o => o.Map(acMoDict)).ToList();
            foreach (var depoOffer in result)
            {
                // if (depoOffer.DepositTerm == null)
                // {
                //     depoOffer.IsTimeless = true;
                //     depoOffer.DepositTerm = new Duration(0, Durations.Days);
                // }

                foreach (var depoCondition in bin.DepoNewConds.Where(c => c.DepositOfferId == depoOffer.Id))
                {
                    var depoCondsModel = depoCondition.Map();

                    depoCondsModel.RateLines = bin.DepositRateLines
                        .Where(l => l.DepositOfferConditionsId == depoCondition.Id)
                        .OrderBy(r => r.DateFrom)
                        .ThenBy(r => r.AmountFrom).ToList();

                    depoOffer.CondsMap.Add(depoCondition.DateFrom, depoCondsModel);
                }
            }
            return result;
        }
    }
}
