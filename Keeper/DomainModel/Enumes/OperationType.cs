using System.Windows.Media;

namespace Keeper.DomainModel.Enumes
{
    public enum OperationType
    {
        �����,
        ������,
        �������,
        �����,
    };

    public static class OperationTypeExtantions
    {
        public static Brush FontColor(this OperationType operationType)
        {
            if (operationType == OperationType.�����) return Brushes.Blue;
            if (operationType == OperationType.������) return Brushes.Red;
            if (operationType == OperationType.�������) return Brushes.Black;
            if (operationType == OperationType.�����) return Brushes.Green;
            return Brushes.Gray;
        }

    }
}