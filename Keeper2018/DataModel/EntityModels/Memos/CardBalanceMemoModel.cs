using System.Windows.Media;

namespace Keeper2018
{
    public class CardBalanceMemoModel
    {
        public int Id { get; set; }
        public AccountItemModel Account { get; set; }
        public decimal BalanceThreshold { get; set; }
        public decimal CurrentBalance { get; set; }

        public decimal ExpenseNotLess { get; set; }
        public decimal ExpenseNotMore { get; set; }
        public decimal CurrentExpense { get; set; }

        public string Comment { get; set; }

        public SolidColorBrush RowBackground => BalanceThreshold > CurrentBalance ? Brushes.LightPink : Brushes.Transparent;
        public bool IsSelected { get; set; }
    }
}
