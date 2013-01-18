using System.Collections.Generic;
using Keeper.DomainModel;

namespace Keeper.ViewModels
{
  /// <summary>
  /// Filter's list for combobox on CurrencyRates window
  /// </summary>
  public static class CurrencyRatesFilterListForCombo
  {
    public static List<CurrencyRatesFilter> FilterList { get; private set; }

    static CurrencyRatesFilterListForCombo()
    {
      FilterList = new List<CurrencyRatesFilter>();

      // <no filter>
      var filter = new CurrencyRatesFilter();
      FilterList.Add(filter);

      // one filter for each currency in my enam, except USD
      foreach (var currencyCode in UsefulLists.CurrencyList)
      {
        if (currencyCode == CurrencyCodes.USD) continue;
        filter = new CurrencyRatesFilter(currencyCode);
        FilterList.Add(filter);
      }

    }
  }
}