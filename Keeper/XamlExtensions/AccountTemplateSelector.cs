using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Keeper.DomainModel;

namespace Keeper.XamlExtensions
{
    class AccountTemplateSelector : DataTemplateSelector
    {
        public DataTemplate AmountStyleForByr { get; set; }
        public DataTemplate AmountStyleForOthers { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (!(item is Transaction)) return null;
            var tr = item as Transaction;
            return tr.Currency == CurrencyCodes.BYR ? AmountStyleForByr : AmountStyleForOthers;
        }
    }
}
