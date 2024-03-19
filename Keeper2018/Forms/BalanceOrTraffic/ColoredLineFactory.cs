using System;
using System.Linq;
using System.Windows.Media;
using KeeperDomain;

namespace Keeper2018
{
    public static class ColoredLineFactory
    {
        public static Tuple<string, Brush> ColoredLine(this KeeperDataModel dataModel, TransactionModel tran, bool isInReturn, int sign)
        {
            var amount = isInReturn ? tran.AmountInReturn : tran.Amount;
            var currency = isInReturn ? tran.CurrencyInReturn : tran.Currency;
            var shortLine = $@"{tran.Timestamp.Date.ToShortDateString()}  {dataModel.AmountInUsdString(tran.Timestamp, currency, amount * sign)}";
            var pp = tran.Tags.Any(t => t.Id == 208) ? "%%" : "";
            return new Tuple<string, Brush>($"  {shortLine}   {pp} {tran.Comment}", GetColor(tran, sign));
        }

        public static Tuple<string, Brush> ColoredLine(this KeeperDataModel dataModel, TransactionModel tran, bool isInReturn, int sign, out decimal inUsd)
        {
            var amount = isInReturn ? tran.AmountInReturn : tran.Amount;
            var currency = isInReturn ? tran.CurrencyInReturn : tran.Currency;
            var shortLine = $"{tran.Timestamp.Date.ToShortDateString()}  {dataModel.AmountInUsdString(tran.Timestamp, currency, amount * sign, out inUsd)}";
            return new Tuple<string, Brush>($"  {shortLine}   {tran.Comment}", GetColor(tran, sign));
        }

        public static Tuple<string, Brush> ColoredLineOneAccountExchange(this KeeperDataModel dataModel, TransactionModel tran)
        {
            var minus = $"{dataModel.AmountInUsdString(tran.Timestamp, tran.Currency, tran.Amount * -1)}";
            var plus = $"{dataModel.AmountInUsdString(tran.Timestamp, tran.CurrencyInReturn, tran.AmountInReturn)}";
            var shortLine = $"{tran.Timestamp.Date.ToShortDateString()}  {minus} -> {plus}";
            return new Tuple<string, Brush>($"  {shortLine}   {tran.Comment}", Brushes.Black);
        }

        private static Brush GetColor(TransactionModel tran, int sign)
        {
            if (tran.Operation == OperationType.Доход)
            {
                return Brushes.Blue;
            }

            if (tran.Operation == OperationType.Расход)
            {
                return Brushes.Red;
            }

            return sign == 1 ? Brushes.DarkBlue : Brushes.DarkRed;
        }
    }
}