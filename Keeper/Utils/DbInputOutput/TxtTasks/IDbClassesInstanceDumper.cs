using Keeper.DomainModel;
using Keeper.Utils.Common;

namespace Keeper.Utils.DbInputOutput.TxtTasks
{
  public interface IDbClassesInstanceDumper
  {
    string Dump(HierarchyItem<Account> account);
    string Dump(ArticleAssociation association);
    string Dump(CurrencyRate rate);
    string Dump(Transaction transaction);
  }
}