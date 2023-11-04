using System;
using System.Linq;
using KeeperDomain;

namespace Keeper2018
{
    public static class MemoExt
    {
        public static bool HasAlarm(this KeeperDataModel keeperDataModel)
        {
            return keeperDataModel.CardBalanceMemoModels.Any(c => c.BalanceThreshold > c.CurrentBalance);
        }
        public static void RememberAll(this KeeperDataModel keeperDataModel)
        {
            keeperDataModel.CardBalanceMemoModels.ForEach(m=> CheckCardThreshold(keeperDataModel, m));
        }

        private static void CheckCardThreshold(KeeperDataModel keeperDataModel, CardBalanceMemoModel memo)
        {
            var accountCalculator = 
                new TrafficOfAccountCalculator(keeperDataModel, memo.Account, 
                    new Period(new DateTime(2001, 12, 31), DateTime.Today.GetEndOfDate()));
            var balance = accountCalculator.EvaluateBalance();
            memo.CurrentBalance = balance.Currencies[CurrencyCode.BYN];

        }
    }
}
