using System.Linq;

namespace Keeper2018
{
    public static class AccountModelExt
    {
        public static bool AccountUsedInTransaction(this KeeperDataModel keeperDataModel, int accountId)
        {
            return keeperDataModel.Transactions.Values.Any(t =>
                t.MyAccount.Id == accountId ||
                (t.MySecondAccount != null && t.MySecondAccount.Id == accountId) ||
                t.Tags != null && t.Tags.Select(tag => tag.Id).Contains(accountId));
        }

        public static AccountModel AccountByTitle(this KeeperDataModel keeperDataModel, string accountTitle)
        {
            return keeperDataModel.AcMoDict.FirstOrDefault(p => p.Value.Name == accountTitle).Value;
        }

        public static IOrderedEnumerable<AccountModel> GetActiveCardsOrderedByFinishDate(this KeeperDataModel dataModel)
        {
            // 161 - папка Карточки

            return dataModel.AcMoDict.Values
                .Where(a => a.Is(161) && a.Deposit?.Card != null)
                .OrderBy(d => d.Deposit.FinishDate);
        }

        public static IOrderedEnumerable<AccountModel> GetOpenDepositsOrderedByFinishDate(this KeeperDataModel dataModel)
        {
            // 166 - папка Депозиты
            return dataModel.AcMoDict.Values
                .Where(a => !a.Children.Any() && a.Is(166))
                .OrderBy(d => d.Deposit.FinishDate);
        }

    }
}