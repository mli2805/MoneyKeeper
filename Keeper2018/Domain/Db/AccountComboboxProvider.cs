using System.Collections.Generic;
using System.Linq;

namespace Keeper2018
{
    public static class AccountComboboxProvider
    {
        public static List<AccountModel> GetLeavesOf(this KeeperDb db, string accountHeader)
        {
            var start = db.AccountsTree.First(r => (string)r.Header == accountHeader);
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


        public static List<Account> GetAccountsOf(this KeeperDb db, string name)
        {
            var start = KeeperDbExt.GetByName(name, db.AccountsTree);
            return db.GetEndAccountsOf(start);
        }
        private static List<Account> GetEndAccountsOf(this KeeperDb db, AccountModel start)
        {
            var result = new List<Account>();
            foreach (var child in start.Children)
            {
                if (child.Children.Count == 0)
                    result.Add(db.AccountPlaneList.First(a=>a.Id == child.Id));
                else
                    result.AddRange(db.GetEndAccountsOf(child));
            }
            return result;
        }

    }
}