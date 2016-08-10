using System.Windows;
using System.Windows.Controls;

namespace Keeper.XamlExtensions
{
    class AccountTemplateSelector : DataTemplateSelector
    {
        public DataTemplate AmountStyleForByr { get; set; }
        public DataTemplate AmountStyleForByrToExchange { get; set; }
        public DataTemplate AmountStyleForByrFromExchange { get; set; }
        public DataTemplate AmountStyleForOthers { get; set; }
        public DataTemplate AmountStyleForOthersToExchange { get; set; }
        public DataTemplate AmountStyleForOthersFromExchange { get; set; }
    }
}
