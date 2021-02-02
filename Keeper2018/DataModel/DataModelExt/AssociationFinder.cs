using System.Linq;
using KeeperDomain;

namespace Keeper2018
{
    public static class AssociationFinder
    {

        public static AccountModel GetAssociation(this KeeperDataModel dataModel, TransactionModel tran, AccountModel tag)
        {
            var associatedTag = dataModel.GetTagToTagAssociation(tran, tag);
            return (associatedTag == null || tran.Tags.Contains(associatedTag)) ? null : associatedTag;
        }

        private static AccountModel GetTagToTagAssociation(this KeeperDataModel dataModel, TransactionModel tran, AccountModel tag)
        {
            var association = (from a in dataModel.TagAssociations
                               where a.ExternalAccount == tag.Id && a.OperationType == tran.Operation
                               select a).FirstOrDefault();
            if (association != null) return dataModel.AcMoDict[association.Tag];

            var reverseAssociation = (from a in dataModel.TagAssociations
                                      where (a.Destination == AssociationType.TwoWay || a.Destination == AssociationType.RightToLeft)
                                            && a.Tag == tag.Id && a.OperationType == tran.Operation
                                      select a).FirstOrDefault();

            return reverseAssociation == null ? null : dataModel.AcMoDict[reverseAssociation.ExternalAccount];
        }

    }
}
