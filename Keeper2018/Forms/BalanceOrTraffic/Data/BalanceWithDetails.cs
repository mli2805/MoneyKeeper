using System.Collections.Generic;

namespace Keeper2018
{
    public class BalanceWithDetails
    {
        public List<BalanceDetailedLine> Lines = new List<BalanceDetailedLine>();
        public decimal TotalInUsd;

        public IEnumerable<string> ToStrings()
        {
            foreach (var line in Lines)
            {
                yield return $"{line.Currency.ToString().ToUpper()} {line.Amount:0,0.00};    {line.PercentOfBalance:0.00}% ";
            }

            yield return $"Итого: {TotalInUsd:0,0.00} usd";
        }
    }
}