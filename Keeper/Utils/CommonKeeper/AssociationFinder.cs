using System.Linq;

using Keeper.DomainModel;

namespace Keeper.Utils.CommonKeeper
{
  public class AssociationFinder
  {
    private readonly KeeperDb _db;

    public AssociationFinder(KeeperDb db)
    {
      _db = db;
    }

    public Account GetAssociation(Account account)
    {
      var association = (from a in _db.ArticlesAssociations
                         where a.ExternalAccount == account
                         select a).FirstOrDefault();
      return association == null ? null : association.AssociatedArticle;
    }

  }
}
