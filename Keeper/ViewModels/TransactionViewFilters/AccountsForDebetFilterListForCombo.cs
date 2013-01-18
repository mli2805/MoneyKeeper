using System;
using System.Collections.Generic;
using System.Linq;
using Keeper.DomainModel;

namespace Keeper.ViewModels
{
  /// <summary>
  /// Filter's list for combobox on Transactions window
  /// </summary>
  public static class AccountsForDebetFilterListForCombo
  {
    public static List<AccountFilter> FilterList { get; private set; }

    static AccountsForDebetFilterListForCombo()
    {
      FilterList = new List<AccountFilter>();

      // <no filter>
      var filter = new AccountFilter();
      FilterList.Add(filter);

      var debetAccounts = 

      foreach (var account in debetAccounts)
      {
        filter = new AccountFilter(account);
        FilterList.Add(filter);
      }

    }
  }
}
