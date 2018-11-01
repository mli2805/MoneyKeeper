using System.Windows.Media;

namespace Keeper2018
{
    public enum OperationType
    {
        Доход,
        Расход,
        Перенос,
        Обмен,
    };

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
