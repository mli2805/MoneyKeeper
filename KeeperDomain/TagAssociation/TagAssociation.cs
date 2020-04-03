using System;

namespace KeeperDomain
{
    [Serializable]
    public class TagAssociation
    {
        public int ExternalAccount { get; set; }
        public OperationType OperationType { get; set; }
        public int Tag { get; set; }
        public AssociationType Destination { get; set; }

        public string Dump()
        {
            return ExternalAccount + " ; " +
                   Tag + " ; " +
                   OperationType + " ; " +
                   Destination;
        }
    }
}
