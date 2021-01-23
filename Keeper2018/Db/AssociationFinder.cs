using System.Linq;
using KeeperDomain;

namespace Keeper2018
{
    public class AssociationFinder
    {
        private readonly KeeperDb _db;

        public AssociationFinder(KeeperDb db)
        {
            _db = db;
        }

        public AccountModel GetAssociation(TransactionModel tran, AccountModel tag)
        {
            var associatedTag = GetTagToTagAssociation(tran, tag);
            return (associatedTag == null || tran.Tags.Contains(associatedTag)) ? null : associatedTag;
        }

        private AccountModel GetTagToTagAssociation(TransactionModel tran, AccountModel tag)
        {
            var association = (from a in _db.TagAssociationModels
                               where a.ExternalAccount.Id == tag.Id && a.OperationType == tran.Operation
                               select a).FirstOrDefault();
            if (association != null) return association.Tag;

            var reverseAssociation = (from a in _db.TagAssociationModels
                           where a.Destination == AssociationType.TwoWay && a.Tag.Id == tag.Id && a.OperationType == tran.Operation
                                      select a).FirstOrDefault();

            return reverseAssociation?.ExternalAccount;
        }

    }
}
