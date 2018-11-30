namespace Keeper2018
{
    public static class ReportLineFactory
    {
        public static ReportLine ReportLine(this KeeperDb db, Balance before, TransactionModel tran, bool isInReturn, int sign)
        {
            var line = new ReportLine();
            line.Date = tran.Timestamp.Date;
            line.Before = before;
            line.After = new Balance(line.Before);

            var amount = isInReturn ? tran.AmountInReturn : tran.Amount;
            // ReSharper disable once PossibleInvalidOperationException
            var currency = isInReturn ? (CurrencyCode)tran.CurrencyInReturn : tran.Currency;
            if (sign == 1)
            {
                line.Income = new Balance(currency, amount);
                line.After.Add(currency, amount);
            }
            else
            {
                line.Outcome = new Balance(currency, amount);
                line.After.Sub(currency, amount);
            }

            line.Comment = tran.Comment;
            return line;
        }

        public static ReportLine ReportLineOneAccountExchange(this KeeperDb db, Balance before, TransactionModel tran)
        {
            var line = new ReportLine();
            line.Date = tran.Timestamp.Date;
            line.Before = before;
            line.After = new Balance(line.Before);

            // ReSharper disable once PossibleInvalidOperationException
            line.Income = new Balance((CurrencyCode)tran.CurrencyInReturn, tran.AmountInReturn);
            line.After.Add((CurrencyCode)tran.CurrencyInReturn, tran.AmountInReturn);
            line.Outcome = new Balance(tran.Currency, tran.Amount);
            line.After.Sub(tran.Currency, tran.Amount);

            line.Comment = tran.Comment;
            return line;
        }
    }
}