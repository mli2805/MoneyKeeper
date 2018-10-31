using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Keeper2018
{
    public static class AccountMapper
    {
        public static ObservableCollection<Account> ToTree(List<SerializableAccount> list)
        {
            var roots = new ObservableCollection<Account>();
            foreach (var serializableAccount in list)
            {
                var account = new Account(serializableAccount.Header)
                {
                    Id = serializableAccount.Id,
                    IsExpanded = serializableAccount.IsExpanded,
                };

                if (serializableAccount.OwnerId == 0)
                {
                    roots.Add(account);
                }
                else
                {
                    var owner = DbUtils.GetById(serializableAccount.OwnerId, roots);
                    owner.Items.Add(account);
                    account.Owner = owner;
                }
               
            }
            return roots;
        }

      
        public static List<SerializableAccount> Flatten(ObservableCollection<Account> roots)
        {
            var result = new List<SerializableAccount>();
            foreach (var root in roots)
            {
                result.AddRange(Flat(root));
            }
            return result;
        }

        private static IEnumerable<SerializableAccount> Flat(Account account)
        {
            var serializableAccount = new SerializableAccount()
            {
                Id = account.Id,
                OwnerId = account.Owner?.Id ?? 0,
                Header = (string)account.Header,
                IsExpanded = account.IsExpanded,
            };
            yield return serializableAccount;

            foreach (var child in account.Children)
            {
                foreach (var grandChild in Flat(child))
                {
                    yield return grandChild;
                }
            }
        }
    }
}