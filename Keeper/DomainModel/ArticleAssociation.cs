using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keeper.DomainModel
{
  public class ArticleAssociation
  {
    public int Id { get; set; }
    public Account ExternalAccount { get; set; }
    public Account AssociatedArticle { get; set; }

    public string ToDumpWithNames()
    {
      return ExternalAccount + " ; " + AssociatedArticle;
    }
  }

}
