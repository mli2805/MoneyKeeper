using System;
using System.Windows;
using System.Windows.Controls;
using Keeper.DomainModel.DbTypes;
using Keeper.DomainModel.Enumes;
using Keeper.DomainModel.Trans;

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

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (!(item is Transaction)) return null;
            var tr = item as Transaction;

            if (tr.Guid != Guid.Empty)
            {
                if (tr.Operation == OperationType.Расход)
                    return tr.Currency == CurrencyCodes.BYR ? AmountStyleForByrToExchange : AmountStyleForOthersToExchange;
                else
                    return tr.Currency == CurrencyCodes.BYR ? AmountStyleForByrFromExchange : AmountStyleForOthersFromExchange;
            }
            else
                return tr.Currency == CurrencyCodes.BYR ? AmountStyleForByr : AmountStyleForOthers;

        }
    }
}
