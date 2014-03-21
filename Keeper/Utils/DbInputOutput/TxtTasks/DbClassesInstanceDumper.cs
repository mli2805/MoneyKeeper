using System;
using System.Composition;
using System.Globalization;
using Keeper.DomainModel;
using Keeper.Utils.Common;

namespace Keeper.Utils.DbInputOutput.TxtTasks
{
  [Export]
  public class DbClassesInstanceDumper : IDbClassesInstanceDumper
  {
    public string Dump(HierarchyItem<Account> account)
    {
      var shiftedName = new string(' ', account.Level * 2) + account.Item.Name;
      var parentForDump = account.Item.Parent == null ? 0 : account.Item.Parent.Id;
      return account.Item.Id + " ; " + shiftedName + " ; " + parentForDump + " ; " + account.Item.IsExpanded + " ; " + account.Item.IsClosed;
    }
    public string Dump(ArticleAssociation association)
    {
      return association.ExternalAccount + " ; " +
             association.OperationType + " ; " +
             association.AssociatedArticle;
    }
    public string Dump(CurrencyRate rate)
    {
      return rate.BankDay.ToString(new CultureInfo("ru-Ru")) + " ; " +
             rate.Currency + " ; " +
             Math.Round(rate.Rate, 4);
    }
    public string Dump(Transaction transaction)
    {
      var s = Convert.ToString(transaction.Timestamp, new CultureInfo("ru-Ru")) + " ; " + transaction.Operation + " ; " +
              transaction.Debet + " ; " + transaction.Credit + " ; " + transaction.Amount + " ; " + transaction.Currency + " ; " +
              transaction.Amount2 + " ; ";

      if (transaction.Currency2 == null || transaction.Currency2 == 0) s = s + "null";
      else s = s + transaction.Currency2;

      s = s + " ; " + transaction.Article + " ; " + transaction.Comment;
      return s;
    }
  }
}