using System.Globalization;
using System.Windows.Media;

namespace Keeper2018
{
    public class CardPaymentsLimitsModel
    {
        public AccountItemModel Account { get; set; }
        public decimal CurrentBalance { get; set; }

        public decimal ExpenseNotLess { get; set; }
        public string NotLess => ExpenseNotLess == 0 ? "-" : ExpenseNotLess.ToString(CultureInfo.InvariantCulture);
        public decimal ExpenseNotMore { get; set; }
        public string NotMore => ExpenseNotMore == 0 ? "-" : ExpenseNotMore.ToString(CultureInfo.InvariantCulture);
        public decimal CurrentExpense { get; set; }

        public string Comment { get; set; }

        public SolidColorBrush RowBackground => GetRowBackground();
        public bool IsSelected { get; set; }

        private SolidColorBrush GetRowBackground()
        {
            if (CurrentExpense < ExpenseNotLess)
                return Brushes.LightPink; 
            if (ExpenseNotMore == 0) return Brushes.Transparent;
            if (CurrentExpense > ExpenseNotMore)
                return Brushes.LightPink;
            if (CurrentExpense > ExpenseNotMore * 0.9M) return Brushes.Goldenrod;
            return Brushes.Transparent;
        }

    }
}
