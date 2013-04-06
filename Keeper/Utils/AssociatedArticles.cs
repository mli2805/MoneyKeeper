using System.Data.Entity;
using System.Linq;
using Caliburn.Micro;
using Keeper.DomainModel;

namespace Keeper.Utils
{
  public static class AssociatedArticles
  {
    public static KeeperDb Db { get { return IoC.Get<KeeperDb>(); } }

    public static Account GetAssociation(Account account)
    {
      var association = (from a in Db.ArticlesAssociations
                         where a.ExternalAccount == account
                         select a).FirstOrDefault();
      return association == null ? null : association.AssociatedArticle;
    }

  }
}
