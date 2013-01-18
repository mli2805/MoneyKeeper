using System.Data.Entity;
using System.Linq;
using Caliburn.Micro;
using Keeper.DomainModel;

namespace Keeper.Utils
{
  public static class AssociatedArticles
  {
    public static KeeperDb Db { get { return IoC.Get<KeeperDb>(); } }


    static AssociatedArticles()
    {
      Db.Accounts.Load();
    }

    public static Account GetAssociation(Account account)
    {
      var association = (from a in Db.ArticlesAssociations.Local
                         where a.ExternalAccount == account
                         select a).FirstOrDefault();
      return association == null ? null : association.AssociatedArticle;
    }

  }
}
