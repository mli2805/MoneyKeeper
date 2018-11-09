using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Keeper2018
{
    public static class AccountsOldTxt
    {
        public static IEnumerable<Account> LoadFromOldTxt()
        {
            var content = File.ReadAllLines(DbUtils.GetTxtFullPath("Accounts.txt"), Encoding.GetEncoding("Windows-1251")).
                Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
            foreach (var line in content)
            {
                yield return AccountFromString(line);
            }
        }


        private static Account AccountFromString(string s)
        {
            var substrings = s.Split(';');
            var account = new Account()
            {
                Id = Convert.ToInt32(substrings[0]),
                OwnerId = Convert.ToInt32(substrings[2]),
                Header = substrings[1].Trim(),
                IsExpanded = Convert.ToBoolean(substrings[5])
            };
            //account.IsFolder = Convert.ToBoolean(substrings[3]);
            //account.IsClosed = Convert.ToBoolean(substrings[4]);
            return account;
        }


    }
}