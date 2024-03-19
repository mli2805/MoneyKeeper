using System.Linq;
using System.Windows.Media;
using KeeperDomain;

namespace Keeper2018
{
    public static class ColoredLineFactory
    {
        public static ListLine ColoredLine(this KeeperDataModel dataModel, TransactionModel tran, bool isInReturn, int sign)
        {
            var amount = isInReturn ? tran.AmountInReturn : tran.Amount;
            var currency = isInReturn ? tran.CurrencyInReturn : tran.Currency;
            var shortLine = $@"{tran.Timestamp.Date.ToShortDateString()}  {dataModel.AmountInUsdString(tran.Timestamp, currency, amount * sign)}";
            return new ListLine($"  {shortLine}   {GetPp(tran)} {tran.Comment}", GetColor(tran, sign));
        }

        private static string GetPp(TransactionModel tran)
        {
            if (tran.Tags.Any(t => t.Id == 208)) return "%%";
            if (tran.Tags.Any(t => t.Id == 701)) return "манибэк";
            return "";
        }

        public static ListLine ColoredLine(this KeeperDataModel dataModel, TransactionModel tran, bool isInReturn, int sign, out decimal inUsd)
        {
            var amount = isInReturn ? tran.AmountInReturn : tran.Amount;
            var currency = isInReturn ? tran.CurrencyInReturn : tran.Currency;
            var shortLine = $"{tran.Timestamp.Date.ToShortDateString()}  {dataModel.AmountInUsdString(tran.Timestamp, currency, amount * sign, out inUsd)}";
            return new ListLine($"  {shortLine}   {tran.Comment}", GetColor(tran, sign));
        }

        public static ListLine ColoredLineOneAccountExchange(this KeeperDataModel dataModel, TransactionModel tran)
        {
            var minus = $"{dataModel.AmountInUsdString(tran.Timestamp, tran.Currency, tran.Amount * -1)}";
            var plus = $"{dataModel.AmountInUsdString(tran.Timestamp, tran.CurrencyInReturn, tran.AmountInReturn)}";
            var shortLine = $"{tran.Timestamp.Date.ToShortDateString()}  {minus} -> {plus}";
            return new ListLine($"  {shortLine}   {tran.Comment}", Brushes.Black);
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