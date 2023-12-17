using System;
using System.Linq;
using System.Threading.Tasks;
using KeeperDomain;

namespace Keeper2018
{
    public static class MemoExt
    {
        public static bool HasAlarm(this KeeperDataModel keeperDataModel)
        {
            return keeperDataModel.CardBalanceMemoModels.Any(c => c.BalanceThreshold > c.CurrentBalance);
        }
        public static Task RememberAll(this KeeperDataModel keeperDataModel)
        {
            keeperDataModel.CardBalanceMemoModels.ForEach(m=> CheckCardThreshold(keeperDataModel, m));
            return Task.CompletedTask;
        }

        public static void CheckCardThreshold(this KeeperDataModel keeperDataModel, CardBalanceMemoModel memo)
        {
            var accountCalculator = 
                new TrafficOfAccountCalculator(keeperDataModel, memo.Account, 
                    new Period(new DateTime(2001, 12, 31), DateTime.Today.GetEndOfDate()));
            var balance = accountCalculator.EvaluateBalance();
            memo.CurrentBalance = balance.Currencies.ContainsKey(CurrencyCode.BYN) ? balance.Currencies[CurrencyCode.BYN] : 0;

        }
    }
}
