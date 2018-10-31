using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Keeper2018
{
    public static class AccountMapper
    {
        public static List<SerializableAccount> Map(ObservableCollection<Account> roots)
        {
            var serializableRoots = new List<SerializableAccount>();
            foreach (var account in roots)
            {
                var serializedAccount = Map(account);
                serializableRoots.Add(serializedAccount);
            }
            return serializableRoots;
        }

        private static SerializableAccount Map(Account account)
        {
            if (account == null) return null;
            var result = new SerializableAccount
            {
                Id = account.Id,
                OwnerId = account.Owner?.Id ?? 0,
                Children = new List<SerializableAccount>(),
                Header = (string) account.Header,
                IsExpanded = account.IsExpanded,
            };

            foreach (var child in account.Children)
            {
                result.Children.Add(Map(child));
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

        private static Account Map(SerializableAccount serializableAccount, ICollection<Account> roots)
        {
            var result = new Account(serializableAccount.Header)
            {
                Id = serializableAccount.Id,
                Owner = serializableAccount.OwnerId == 0 ? null : DbUtils.GetById(serializableAccount.OwnerId, roots),
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