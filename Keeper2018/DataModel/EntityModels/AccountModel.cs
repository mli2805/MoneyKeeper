using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using KeeperDomain;

namespace Keeper2018
{
    public class AccountModel : TreeViewItem
    {
        public int Id { get; set; }

        public AccountModel Owner { get; set; } // property Parent is occupied in TreeViewItem
        public AccountModel Bank { get; set; } // null if not in bank

        // Items are in TreeViewItem
        public List<AccountModel> Children => Items.Cast<AccountModel>().ToList();
        public Deposit Deposit { get; set; }

        public new string Name => (string)Header;
        public bool IsDeposit => Deposit != null;
        public bool IsFolder => Children.Any(); // in XAML
        public bool IsLeaf => !Children.Any(); // in XAML
        public bool IsCard => Deposit?.Card != null; // in XAML

        // my accounts do not use associations
        public int AssociatedIncomeId { get; set; } // for external
        public int AssociatedExpenseId { get; set; } // for external
        public int AssociatedExternalId { get; set; } // for tag

        public string ButtonName { get; set; } // face of shortcut button (if exists)

        public override string ToString() => (string)Header;
       

        public AccountModel(string headerText)
        {
            Header = headerText;
            IsExpanded = true;
        }

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

        public AccountModel IsC(AccountModel accountModel)
        {
            if (Equals(accountModel)) return this;
            if (Owner == null) return null;
            if (Owner.Equals(accountModel)) return this;
            return Owner.IsC(accountModel);
        }

        public bool IsTag => Is(185) || Is(189);
        public bool IsMyAccount => Is(158); // in XAML
        public bool IsMyAccountsInBanksFolder => Is(159);
        public bool IsFolderOfClosed => Is(393);
    }

}
