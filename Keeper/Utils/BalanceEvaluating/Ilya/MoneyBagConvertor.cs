using System;
using System.Composition;
using System.Linq;
using Keeper.DomainModel.Enumes;
using Keeper.Utils.Rates;

namespace Keeper.Utils.BalanceEvaluating.Ilya
{
    [Export]
    public class MoneyBagConvertor
    {
        private readonly RateExtractor _rateExtractor;

        [ImportingConstructor]
        public MoneyBagConvertor(RateExtractor rateExtractor)
        {
            _rateExtractor = rateExtractor;
        }

        public decimal MoneyBagToUsd(MoneyBag moneyBag, DateTime date)
        {
            var currencies = Enum.GetValues(typeof(CurrencyCodes)).OfType<CurrencyCodes>().ToList();
            decimal totalInUsd = 0;
            foreach (var currency in currencies)
            {
                totalInUsd += _rateExtractor.GetUsdEquivalent(moneyBag[currency], currency, date);
            }
            return totalInUsd;
        }


    }
}
