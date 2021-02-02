using System.Windows.Media;
using KeeperDomain;

namespace Keeper2018
{
    public class TagAssociationModel
    {
        public int Id { get; set; }
        public string ExternalAccount { get; set; }
        public OperationType OperationType { get; set; }
        public string Tag { get; set; }
        public AssociationType Destination { get; set; }

        public Brush FontColor => OperationType.FontColor();
    }
}