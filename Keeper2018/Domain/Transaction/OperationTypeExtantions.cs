using System.Windows.Media;
using KeeperDomain;

namespace Keeper2018
{
    public static class OperationTypeExtantions
    {
        public static Brush FontColor(this OperationType operationType)
        {
            if (operationType == OperationType.Доход) return Brushes.Blue;
            if (operationType == OperationType.Расход) return Brushes.Red;
            if (operationType == OperationType.Перенос) return Brushes.Black;
            if (operationType == OperationType.Обмен) return Brushes.Green;
            return Brushes.Gray;
        }

    }
}