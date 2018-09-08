using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;

namespace Keeper2018
{
    public static class AccountsOldTxt
    {
        public static ObservableCollection<Account> LoadFromOldTxt()
        {
            var roots = new ObservableCollection<Account>();

            var content = File.ReadAllLines(DbUtils.GetTxtFullPath("Accounts.txt"), Encoding.GetEncoding("Windows-1251")).
                Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
            foreach (var line in content)
            {
                var account = AccountFromString(line, out int ownerId);
                if (ownerId == 0)
                    roots.Add(account);
                else
                {
                    account.Owner = DbUtils.GetById(ownerId, roots);
                    account.Owner.Items.Add(account);
                }
            }

            return roots;
        }

      
        private static Account AccountFromString(string s, out int ownerId)
        {
            var substrings = s.Split(';');
            var account = new Account(substrings[1].Trim())
            {
                Id = Convert.ToInt32(substrings[0]),
                IsExpanded = Convert.ToBoolean(substrings[5])
            };
            ownerId = Convert.ToInt32(substrings[2]);
            //account.IsFolder = Convert.ToBoolean(substrings[3]);
            //account.IsClosed = Convert.ToBoolean(substrings[4]);
            return account;
        }

       
    }
}