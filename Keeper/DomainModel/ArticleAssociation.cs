using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keeper.DomainModel
{
  public class ArticleAssociation : IComparable
  {
    public int Id { get; set; }
    public Account ExternalAccount { get; set; }
    public OperationType OperationType { get; set; }
    public Account AssociatedArticle { get; set; }

    public string ToDumpWithNames()
    {
      return ExternalAccount + " ; " + OperationType + " ; " + AssociatedArticle;
    }

    public int CompareTo(object obj)
    {
      return System.String.Compare(ExternalAccount.Name, ((Account) obj).Name, System.StringComparison.Ordinal);
    }
  }

}
