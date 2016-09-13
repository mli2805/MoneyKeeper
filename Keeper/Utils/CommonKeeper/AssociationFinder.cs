using System.Composition;
using System.Linq;
using Keeper.DomainModel.DbTypes;
using Keeper.DomainModel.Enumes;
using Keeper.DomainModel.Trans;

namespace Keeper.Utils.CommonKeeper
{
    [Export]
    public class AssociationFinder
    {
        private readonly KeeperDb _db;

        [ImportingConstructor]
        public AssociationFinder(KeeperDb db)
        {
            _db = db;
        }

        public Account GetAssociation(TranWithTags tran, Account tag)
        {
            var associatedTag = GetTagToTagAssociation(tran, tag);
            return (associatedTag == null || tran.Tags.Contains(associatedTag)) ? null : associatedTag;
        }

        public Account GetBank(Account account)
        {

            return account.IsDeposit() ? account.Deposit.DepositOffer.BankAccount : null;
        }

        private Account GetTagToTagAssociation(TranWithTags tran, Account tag)
        {
            var association = (from a in _db.ArticlesAssociations
                               where a.ExternalAccount == tag && a.OperationType == tran.Operation
                               select a).FirstOrDefault();
            if (association != null) return association.AssociatedArticle;

            var reverseAssociation = (from a in _db.ArticlesAssociations
                           where a.IsTwoWay && a.AssociatedArticle == tag && a.OperationType == tran.Operation
                                      select a).FirstOrDefault();

            return reverseAssociation?.ExternalAccount;
        }

    }
}
