using System;
using System.Collections.Generic;
using System.Linq;
using Keeper.DomainModel;

namespace Keeper.ViewModels
{
  /// <summary>
  /// Filter's list for combobox on Transactions window
  /// </summary>
  public static class FilterListsForComboboxes
  {
    public static List<AccountFilter> DebetFilterList { get; private set; }
    public static List<AccountFilter> CreditFilterList { get; private set; }
    public static List<AccountFilter> ArticleFilterList { get; private set; }

    static FilterListsForComboboxes()
    {
      DebetFilterList = new List<AccountFilter>();

      // <no filter>
      var filter = new AccountFilter();
      DebetFilterList.Add(filter);

      var debetAccounts = UsefulLists.AllAccounts;
      foreach (var account in debetAccounts)
      {
        filter = new AccountFilter(account);
        DebetFilterList.Add(filter);
      }

      CreditFilterList = DebetFilterList;

      ArticleFilterList = new List<AccountFilter>();
      // <no filter>
      filter = new AccountFilter();
      ArticleFilterList.Add(filter);

      var articleAccounts = UsefulLists.AllArticles;
      foreach (var account in articleAccounts)
      {
        filter = new AccountFilter(account);
        ArticleFilterList.Add(filter);
      }

    }
  }
}
