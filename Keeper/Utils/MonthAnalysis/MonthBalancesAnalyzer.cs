using System;
using System.Composition;
using Keeper.DomainModel.DbTypes;
using Keeper.DomainModel.Extentions;
using Keeper.DomainModel.MonthAnalysis;
using Keeper.DomainModel.Trans;
using Keeper.DomainModel.WorkTypes;
using Keeper.Utils.Rates;

namespace Keeper.Utils.MonthAnalysis
{
    [Export]
    public class MonthBalancesAnalyzer
    {
        private readonly KeeperDb _db;
        
        private readonly RateExtractor _rateExtractor;

        [ImportingConstructor]
        public MonthBalancesAnalyzer(KeeperDb db, RateExtractor rateExtractor)
        {
            _db = db;
            
            _rateExtractor = rateExtractor;
        }

        public ExtendedBalance GetExtendedBalanceBeforeDate(DateTime someDate)
        {
            var period = new Period(new DateTime(1,1,1), someDate);

            var allMine = _db.SeekAccount("Мои");
            var allMineBalance = _db.TransWithTags.Sum(t => t.MoneyBagForAccount(allMine, period));
            var cards = _db.SeekAccount("Карточки");
            var cardsBalance = _db.TransWithTags.Sum(t => t.MoneyBagForAccount(cards, period));
            var deposits = _db.SeekAccount("Депозиты");
            var depositsBalance = _db.TransWithTags.Sum(t => t.MoneyBagForAccount(deposits, period));


            var result = new ExtendedBalance();
            result.Common.MoneyBag = allMineBalance;
            result.Common.TotalInUsd = _rateExtractor.GetUsdEquivalent(allMineBalance, someDate);
            result.OnDeposits.MoneyBag = cardsBalance + depositsBalance;
            result.OnDeposits.TotalInUsd = _rateExtractor.GetUsdEquivalent(result.OnDeposits.MoneyBag, someDate);
            result.OnHands.MoneyBag = allMineBalance - result.OnDeposits.MoneyBag;
            result.OnHands.TotalInUsd = _rateExtractor.GetUsdEquivalent(result.OnHands.MoneyBag, someDate);
            return result;
        }
    }
}