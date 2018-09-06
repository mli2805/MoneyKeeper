using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using Keeper2018.Properties;

namespace Keeper2018
{
    public class Account : TreeViewItem
    {
        public int Id { get; set; }

        public Account Owner { get; set; } // property Parent is occupied in TreeViewItem

                                           // Items are in TreeViewItem
        public List<Account> Children => Items.Cast<Account>().ToList();

        public Account(string headerText)
        {
            Header = headerText;
            IsExpanded = true;
        }
    }

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

    public static class AccountsOldTxt
    {
        public static ObservableCollection<Account> LoadFromOldTxt()
        {
            var roots = new ObservableCollection<Account>();

            var content = File.ReadAllLines(GetAccountsFilename(), Encoding.GetEncoding("Windows-1251")).
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

        private static string GetAccountsFilename()
        {
            var path = AppDomain.CurrentDomain.BaseDirectory;
            var dataFolder = (string) Settings.Default["DataFolder"];
            var dataPath = Path.Combine(path, dataFolder);
            return Path.Combine(dataPath, "Accounts.txt");
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