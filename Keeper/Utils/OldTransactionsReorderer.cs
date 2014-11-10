using System;
using System.Composition;
using Keeper.ByFunctional.EditingAccounts;
using Keeper.DomainModel;

namespace Keeper.Utils
{
  [Export]
  public class OldTransactionsReorderer
  {
    private readonly KeeperDb _db;
    private readonly AccountTreeStraightener _accountTreeStraightener;

    [ImportingConstructor]
    public OldTransactionsReorderer(KeeperDb db, AccountTreeStraightener accountTreeStraightener)
    {
      _db = db;
      _accountTreeStraightener = accountTreeStraightener;
    }

    public void MarkOldAvtoExpenseBySpecialArticle()
    {
      var newTransactionsArticle = _accountTreeStraightener.Seek("Все в кучу до 1 мая 2010 года", _db.Accounts);
      foreach (var transaction in _db.Transactions)
      {
        if (transaction.Operation == OperationType.Расход && transaction.Article.Name == "Обслуживание авто" && transaction.Timestamp < new DateTime(2010,5,1))
          transaction.Article = newTransactionsArticle;
      }
    }
  }
}
