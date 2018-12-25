using System;
using System.Windows.Media;

namespace Keeper2018
{
    public class LineModel
    {
        public string ExternalAccount { get; set; }
        public OperationType OperationType { get; set; }
        public string Tag { get; set; }
        public AssociationType Destination { get; set; }

        public int CompareTo(object obj)
        {
            return string.Compare(ExternalAccount, ((string)obj), StringComparison.Ordinal);
        }
        public Brush FontColor => OperationType.FontColor();

        public override string ToString()
        {
            return $"{OperationType.ToString()} {ExternalAccount} {Tag} {Destination.ToString()}";
        }
    }
}