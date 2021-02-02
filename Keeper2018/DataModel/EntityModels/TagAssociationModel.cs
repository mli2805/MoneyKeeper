using KeeperDomain;

namespace Keeper2018
{
    public class TagAssociationModel
    {
        public int Id { get; set; }
        public AccountModel ExternalAccount { get; set; }
        public OperationType OperationType { get; set; }
        public AccountModel Tag { get; set; }
        public AssociationType Destination { get; set; }
    }
}