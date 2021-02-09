using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using KeeperDomain;

namespace Keeper2018
{
    public class AccountModel : TreeViewItem, IComparable
    {
        public int Id { get; set; }

        public AccountModel Owner { get; set; } // property Parent is occupied in TreeViewItem

        // Items are in TreeViewItem
        public List<AccountModel> Children => Items.Cast<AccountModel>().ToList();
        public Deposit Deposit { get; set; }

        public new string Name => (string)Header;
        public bool IsDeposit => Deposit != null;
        public bool IsCard => Deposit?.Card != null;

        public override string ToString() => (string)Header;
        public int CompareTo(object obj)
        {
            return string.Compare(Name, ((AccountModel)obj).Name, StringComparison.Ordinal);
        }

        public AccountModel(string headerText)
        {
            Header = headerText;
            IsExpanded = true;
        }

        public bool IsFolder { get; set; }

        public bool Is(AccountModel accountModel)
        {
            if (Equals(accountModel)) return true;
            return Owner != null && Owner.Is(accountModel);
        }

        public bool Is(int accountId)
        {
            if (accountId == Id) return true;
            return Owner != null && Owner.Is(accountId);
        }

        // возвращает первого потомка accountModel'a , если this не сам accountModel.
        public AccountModel IsC(AccountModel accountModel)
        {
            if (Equals(accountModel)) return this;
            if (Owner == null) return null;
            if (Owner.Equals(accountModel)) return this;
            return Owner.IsC(accountModel);
        }

        public bool IsTag => Is(185) || Is(189);
    }

}
