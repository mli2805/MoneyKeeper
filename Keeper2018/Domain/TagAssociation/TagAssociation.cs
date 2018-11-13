using System;
using System.Windows.Media;

namespace Keeper2018
{
    [Serializable]
    public class TagAssociation
    {
        public int ExternalAccount { get; set; }
        public OperationType OperationType { get; set; }
        public int Tag { get; set; }
        public AssociationType Destination { get; set; }
    }

    public class TagAssociationModel : IComparable
    {
        public AccountModel ExternalAccount { get; set; }
        public OperationType OperationType { get; set; }
        public AccountModel Tag { get; set; }
        public AssociationType Destination { get; set; }

        public int CompareTo(object obj)
        {
            return string.Compare(ExternalAccount.Name, ((Account)obj).Name, StringComparison.Ordinal);
        }
        public Brush FontColor => OperationType.FontColor();
    }
}
