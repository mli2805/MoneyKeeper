using System.Linq;
using KeeperDomain;

namespace Keeper2018
{
    public class AccountItemModel : TreeViewItemModel
    {
        public string Img => Name.StartsWith("Б") ? "../../Resources/gsk.png" : "../../Resources/paycard.png";

        public AccountItemModel Bank { get; set; } // null if not in bank
        public Deposit Deposit { get; set; }

        public bool IsDeposit => Deposit != null;
        public bool IsFolder => Children.Any(); // in XAML
        public bool IsLeaf => !Children.Any(); // in XAML
        public bool IsCard => Deposit?.Card != null; // in XAML

        // my accounts do not use associations
        public int AssociatedIncomeId { get; set; } // for external
        public int AssociatedExpenseId { get; set; } // for external
        public int AssociatedExternalId { get; set; } // for tag

        public string ButtonName { get; set; } // face of shortcut button (if exists)

        public override string ToString() => Name;



        public AccountItemModel(int id, string name, TreeViewItemModel parent) : base(id, name, parent)
        {
        }

        public bool IsTag => Is(185) || Is(189);
        public bool IsMyAccount => Is(158); // in XAML
        public bool IsMyAccountsInBanksFolder => Is(159);
        public bool IsFolderOfClosed => Is(393);
    }
}