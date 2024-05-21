using System;
using System.Linq;
using System.Threading.Tasks;
using KeeperDomain;

namespace Keeper2018
{
    public static class MemoExt
    {
        public static bool HasLowBalanceAlarm(this KeeperDataModel keeperDataModel)
        {
            return keeperDataModel.CardBalanceMemoModels.Any(c => c.BalanceThreshold > c.CurrentBalance);
        }

        public static Task RefreshCardBalances(this KeeperDataModel keeperDataModel)
        {
            foreach (var m in keeperDataModel.CardBalanceMemoModels)
            {
                m.CurrentBalance = keeperDataModel.GetCurrentBalance(m.Account);
            }
            return Task.CompletedTask;
        }

        public static decimal GetCurrentBalance(this KeeperDataModel keeperDataModel, AccountItemModel account)
        {
            var accountCalculator =
                new TrafficOfAccountCalculator(keeperDataModel, account,
                    new Period(new DateTime(2001, 12, 31), DateTime.Today.GetEndOfDate()));
            var balance = accountCalculator.EvaluateBalance();
            return balance.Currencies.TryGetValue(CurrencyCode.BYN, out var currency) ? currency : 0;
        }

        public static decimal GetExpenseForCurrentMonth(this KeeperDataModel keeperDataModel, AccountItemModel account)
        {
            var period = DateTime.Today.GetFullMonthForDate();
            var expenseTrans = keeperDataModel.Transactions.Values
                .Where(t => period.Includes(t.Timestamp) && t.MyAccount == account && t.Operation == OperationType.Расход);
            return expenseTrans.Sum(t => t.Amount);
        }
    }
}
