using System.Collections.Generic;
using System.Linq;

namespace Keeper2018
{
   
    public static class AccountComboboxProvider
    {
        public static List<AccountModel> GetLeavesOf(this KeeperDataModel dataModel, string accountHeader)
        {
            var start = dataModel.AccountsTree.First(r => (string)r.Header == accountHeader);
            return GetLeaves(start);
        }
       
        private static List<AccountModel> GetLeaves(AccountModel start)
        {
            var result = new List<AccountModel>();
            foreach (var child in start.Children)
            {
                if (child.Children.Count == 0)
                    result.Add(child);
                else
                    result.AddRange(GetLeaves(child));
            }
            return result;
        }
    }
}