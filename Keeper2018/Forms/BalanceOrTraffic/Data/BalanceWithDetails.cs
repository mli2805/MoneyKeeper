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
                yield return $"{line.Amount} {line.Currency.ToString().ToLower()};  {line.AmountInUsd:0.00} usd;  {line.PercentOfBalance:0.00}% ";
            }

            yield return $"Итого: {TotalInUsd:0.00} usd";
        }
    }
}