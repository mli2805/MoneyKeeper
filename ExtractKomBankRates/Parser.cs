using KeeperDomain.Exchange;

namespace ExtractKomBankRates
{
    public static class Parser
    {
        public static ExchangeRates? Parse(this string str)
        {
            var ss = str.Split(',');
            if (ss[1] != "BNB")
                return null;


            var result = new ExchangeRates();
            result.Date = Convert.ToDateTime(ss[3]);

            result.UsdToByn = double.Parse(ss[4]);
            result.BynToUsd = double.Parse(ss[5]);

            result.EurToByn = double.Parse(ss[6]);
            result.BynToEur = double.Parse(ss[7]);

            result.RubToByn = double.Parse(ss[8]);
            result.BynToRub = double.Parse(ss[9]);

            result.EurToUsd = double.Parse(ss[10]);
            result.UsdToEur = double.Parse(ss[11]);

            result.UsdToRub = double.Parse(ss[12]);
            result.RubToUsd = double.Parse(ss[13]);

            result.EurToRub = double.Parse(ss[14]);
            result.RubToEur = double.Parse(ss[15]);

            return result;
        }
    }
}
