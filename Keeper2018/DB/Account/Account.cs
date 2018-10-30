using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;

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

    [Serializable]
    public class SerializableAccount
    {
        public int Id;
        public SerializableAccount Owner;
        public List<SerializableAccount> Children;
        public string Header;
        public bool IsExpanded;
    }

    public static class AccountMapper
    {
        public static List<SerializableAccount> Map(ObservableCollection<Account> roots)
        {
            var serializableRoots = new List<SerializableAccount>();
            foreach (var account in roots)
            {
                var serializedAccount = Map(account, serializableRoots);
                serializableRoots.Add(serializedAccount);
            }
            return serializableRoots;
        }

        public static SerializableAccount GetById(int id, ICollection<SerializableAccount> roots)
        {
            foreach (var account in roots)
            {
                if (account.Id == id) return account;
                var acc = GetById(id, account.Children);
                if (acc != null) return acc;
            }
            return null;
        }

        public static SerializableAccount Map(Account account, ICollection<SerializableAccount> roots)
        {
            if (account == null) return null;
            var result = new SerializableAccount
            {
                Id = account.Id,
                Children = new List<SerializableAccount>(),
                Header = (string) account.Header,
                IsExpanded = account.IsExpanded,
                Owner = account.Owner.Id == 0 ? null : GetById(account.Owner.Id, roots),
            };

            foreach (var child in account.Children)
            {
                result.Children.Add(Map(child, roots));
            }
            return result;
        }

        public static ObservableCollection<Account> Map(List<SerializableAccount> serializableRoots)
        {
            var roots = new ObservableCollection<Account>();
            foreach (var serializableAccount in serializableRoots)
            {
                var root = Map(serializableAccount, roots);
                roots.Add(root);
            }
            return roots;
        }

        public static Account Map(SerializableAccount serializableAccount, ICollection<Account> roots)
        {
            var result = new Account(serializableAccount.Header)
            {
                Id = serializableAccount.Id,
                Owner = serializableAccount.Owner.Id == 0 ? null : DbUtils.GetById(serializableAccount.Owner.Id, roots),
                IsExpanded = serializableAccount.IsExpanded,
            };
            foreach (var child in serializableAccount.Children)
            {
                result.Items.Add(Map(child, roots));
            }
            return result;
        }
    }
}