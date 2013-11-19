using System.Linq;
using Caliburn.Micro;
using Keeper.DomainModel;

namespace Keeper.Utils
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
