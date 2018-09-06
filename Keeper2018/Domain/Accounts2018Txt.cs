using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Keeper2018.Properties;

namespace Keeper2018
{
    public static class Accounts2018Txt
    {
        private static string GetAccountsFilename()
        {
            var path = AppDomain.CurrentDomain.BaseDirectory;
            var dataFolder = (string) Settings.Default["DataFolder"];
            var dataPath = Path.Combine(path, dataFolder);
            return Path.Combine(dataPath, "Accounts2018.txt");
        }

        public static ObservableCollection<Account> LoadFromTxt()
        {
            var content = File.ReadAllLines(GetAccountsFilename()).Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
            var roots = new ObservableCollection<Account>();
            foreach (var line in content)
            {
                var account = ParseAccount(line, out int ownerId);
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

        private static Account ParseAccount(string s, out int ownerId)
        {
            var substrings = s.Split(';');
            var account = new Account(substrings[1].Trim())
            {
                Id = Convert.ToInt32(substrings[0]),
                IsExpanded = substrings[3].Trim() == "expanded",
            };
            ownerId = Convert.ToInt32(substrings[2]);
            return account;
        }
        public static void SaveInTxt(ObservableCollection<Account> accounts)
        {
            var content = new List<string>();
            foreach (var root in accounts)
                content.AddRange(DumpAccountWithChildren(root));
            File.WriteAllLines(GetAccountsFilename(), content);
        }

        private static List<string> DumpAccountWithChildren(Account account, int offset = 0)
        {
            var result = new List<string> {DumpAccount(account, offset)};
            foreach (var child in account.Children)
                result.AddRange(DumpAccountWithChildren(child, offset + 2));
            return result;
        }

        private static string DumpAccount(Account account, int offset)
        {
            var ownerId = account.Owner?.Id ?? 0;
            var expanded = account.IsExpanded ? "expanded" : "collapsed";
            return account.Id + new string(' ', offset * 2) + " ; " + account.Header + " ; " + ownerId + " ; " + expanded;
        }
    }
}