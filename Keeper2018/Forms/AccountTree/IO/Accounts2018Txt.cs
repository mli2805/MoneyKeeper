using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace Keeper2018
{
    public static class Accounts2018Txt
    {
        public static IEnumerable<Account> LoadFromTxt()
        {
            var content = File.ReadAllLines(DbIoUtils.GetOldTxtFullPath("Accounts2018.txt")).Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
            foreach (var line in content)
            {
                yield return ParseAccount(line);
            }
        }

        private static Account ParseAccount(string s)
        {
            var substrings = s.Split(';');
            var account = new Account()
            {
                Id = Convert.ToInt32(substrings[0]),
                OwnerId = Convert.ToInt32(substrings[2]),
                Header = substrings[1].Trim(),
                IsExpanded = substrings[3].Trim() == "expanded",
            };
            return account;
        }

        public static void SaveInTxt(ObservableCollection<AccountModel> accounts)
        {
            var content = new List<string>();
            foreach (var root in accounts)
                content.AddRange(DumpAccountWithChildren(root));
            File.WriteAllLines(DbIoUtils.GetOldTxtFullPath("Accounts2018.txt"), content);
        }

        private static List<string> DumpAccountWithChildren(AccountModel accountModel, int offset = 0)
        {
            var result = new List<string> { DumpAccount(accountModel, offset) };
            foreach (var child in accountModel.Children)
                result.AddRange(DumpAccountWithChildren(child, offset + 2));
            return result;
        }

        private static string DumpAccount(AccountModel accountModel, int offset)
        {
            var ownerId = accountModel.Owner?.Id ?? 0;
            var expanded = accountModel.IsExpanded ? "expanded" : "collapsed";
            return accountModel.Id + new string(' ', offset * 2) + " ; " + accountModel.Header + " ; " + ownerId + " ; " + expanded;
        }
    }
}