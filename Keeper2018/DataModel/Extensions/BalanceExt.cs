using System;
using System.Linq;

namespace Keeper2018
{
    public static class BalanceExt
    {
        public static decimal ToUsd(this Balance balance, KeeperDataModel dataModel, DateTime date)
        {
            return balance.Currencies
                .Where(balanceCurrency => balanceCurrency.Value > 0)
                .Sum(balanceCurrency => 
                    dataModel.AmountInUsd(date, balanceCurrency.Key, balanceCurrency.Value));
        }
    }
}