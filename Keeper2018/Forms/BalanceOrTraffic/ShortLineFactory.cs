using KeeperDomain;

namespace Keeper2018
{
    public static class ShortLineFactory
    {
        public static string ShortLine(this KeeperDb db, Transaction tran, bool isInReturn, int sign)
        {
            var amount = isInReturn ? tran.AmountInReturn : tran.Amount;
            var currency = isInReturn ? tran.CurrencyInReturn : tran.Currency;
            var shortLine = $"{tran.Timestamp.Date.ToShortDateString()}  {db.AmountInUsdString(tran.Timestamp, currency, amount * sign)}";
            return $"  {shortLine}   {tran.Comment}";
        }

        public static string ShortLine(this KeeperDb db, Transaction tran, bool isInReturn, int sign, out decimal inUsd)
        {
            var amount = isInReturn ? tran.AmountInReturn : tran.Amount;
            var currency = isInReturn ? tran.CurrencyInReturn : tran.Currency;
            var shortLine = $"{tran.Timestamp.Date.ToShortDateString()}  {db.AmountInUsdString(tran.Timestamp, currency, amount * sign, out inUsd)}";
            return $"  {shortLine}   {tran.Comment}";
        }

        public static string ShortLineOneAccountExchange(this KeeperDb db, Transaction tran)
        {
            var minus = $"{db.AmountInUsdString(tran.Timestamp, tran.Currency, tran.Amount * -1)}";
            var plus = $"{db.AmountInUsdString(tran.Timestamp, tran.CurrencyInReturn, tran.AmountInReturn)}";
            var shortLine = $"{tran.Timestamp.Date.ToShortDateString()}  {minus} -> {plus}";
            return $"  {shortLine}   {tran.Comment}";
        }

    }
}