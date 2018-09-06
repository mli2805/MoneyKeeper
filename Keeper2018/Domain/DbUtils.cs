using System.Collections.Generic;

namespace Keeper2018
{
    public static class DbUtils
    {
        public static Account GetById(int id, ICollection<Account> roots)
        {
            foreach (var account in roots)
            {
                if (account.Id == id) return account;
                var acc = GetById(id, account.Children);
                if (acc != null) return acc;
            }
            return null;
        } 
    }
}