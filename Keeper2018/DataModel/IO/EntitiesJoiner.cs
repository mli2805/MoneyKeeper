using System.Collections.Generic;
using System.Linq;
using KeeperDomain;

namespace Keeper2018
{
    public static class EntitiesJoiner
    {
        public static List<Account> JoinAccountParts(this KeeperBin bin)
        {
            var accounts = bin.AccountPlaneList;
            foreach (var deposit in bin.Deposits)
                accounts.First(a => a.Id == deposit.MyAccountId).Deposit = deposit;

            foreach (var card in bin.PayCards)
                accounts.First(a => a.Id == card.MyAccountId).Deposit.Card = card;
            return accounts;
        }

        public static List<CarVm> JoinCarParts(this KeeperBin bin)
        {
            var result = bin.Cars.Select(c => c.Map()).ToList();
            foreach (var car in result)
            {
                var arr = bin.YearMileages.Where(l => l.CarId == car.Id).ToArray();
                var prev = car.PurchaseMileage;
                foreach (var t in arr)
                {
                    var cc = t.Map();
                    cc.YearMileage = t.Mileage - prev;
                    car.YearsMileage.Add(cc);
                    prev = t.Mileage;
                }
            }
            return result;
        }

        public static List<DepositOfferModel> JoinDepoParts(this KeeperBin bin)
        {
            var result = bin.DepositOffers.Select(o => o.Map(bin.AccountPlaneList)).ToList();
            foreach (var depoOffer in result)
            {
                foreach (var depoCondition in bin.DepositConditions.Where(c => c.DepositOfferId == depoOffer.Id))
                {
                    depoCondition.CalculationRules =
                        bin.DepositCalculationRules.First(cr => cr.DepositOfferConditionsId == depoCondition.Id);

                    depoCondition.RateLines = bin.DepositRateLines
                        .Where(l => l.DepositOfferConditionsId == depoCondition.Id)
                        .OrderBy(r => r.DateFrom)
                        .ThenBy(r => r.AmountFrom).ToList();

                    depoOffer.ConditionsMap.Add(depoCondition.DateFrom, depoCondition);
                }
            }
            return result;
        }
    }
}
