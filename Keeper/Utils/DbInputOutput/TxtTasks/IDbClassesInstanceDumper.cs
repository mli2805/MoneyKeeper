using Keeper.DomainModel;
using Keeper.DomainModel.DbTypes;
using Keeper.DomainModel.Deposit;
using Keeper.DomainModel.Transactions;
using Keeper.Utils.Common;

namespace Keeper.Utils.DbInputOutput.TxtTasks
{
  public interface IDbClassesInstanceDumper
  {
    string Dump(HierarchyItem<Account> account);
    string Dump(ArticleAssociation association);
    string Dump(CurrencyRate rate);
    string Dump(NbRate rate);
    string Dump(Transaction transaction);
    string Dump(Deposit deposit);
    string Dump(BankDepositRateLine bankDepositRateLine, int accountId);
  }
}