using System.Windows.Media;

namespace Keeper2018
{
    public class CardBalanceMemoModel
    {
        public int Id { get; set; }
        public AccountItemModel Account { get; set; }
        public decimal BalanceThreshold { get; set; }
        public decimal CurrentBalance { get; set; }

        public SolidColorBrush RowBackground => BalanceThreshold > CurrentBalance ? Brushes.LightPink : Brushes.Transparent;
        public bool IsSelected { get; set; }
    }
}
