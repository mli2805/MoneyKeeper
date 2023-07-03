using KeeperDomain;

namespace Keeper2018
{
    public class AccountItemModel : TreeViewItemModel
    {
        public string Img => GetIconPath();
        public bool IsFolder { get; set; }

        public AccountItemModel Bank { get; set; } // null if not in bank
        public Deposit Deposit { get; set; }
        public PayCard PayCard { get; set; }

        public bool IsDeposit => Deposit != null;
        public bool IsCard => PayCard != null; // in XAML

        // my accounts do not use associations
        public int AssociatedIncomeId { get; set; } // for external
        public int AssociatedExpenseId { get; set; } // for external
        public int AssociatedExternalId { get; set; } // for tag

        public string ButtonName { get; set; } // face of shortcut button (if exists)
        public string Comment { get; set; }

        public override string ToString() => Name;



        public AccountItemModel(int id, string name, TreeViewItemModel parent) : base(id, name, parent)
        {
        }

        public bool IsIncomeTag => Is(185);
        public bool IsExpenseTag => Is(189);
        public bool IsTag => Is(185) || Is(189);

        public bool IsFolderOfClosed => Is(393);
        public bool IsFolderOfDeposits => Is(166);
        public bool IsFolderOfMyBankAccounts => Is(159);

        public bool IsDebt => Is(866);
        public bool IsFolderOfTrustAccounts => Is(902);
        public bool IsMyAccount => Is(158);

        private string GetIconPath()
        {
            if (IsFolder)
                return "../../Resources/tree16/yellow_folder.png";
            if (IsFolderOfClosed)
                return "../../Resources/tree16/cross.png";
            if (IsCard)
                return "../../Resources/tree16/paycard4.png";
            if (IsDeposit)
                return "../../Resources/tree16/deposit7.png";
            if (IsDebt)
                return "../../Resources/tree16/hand_point_left.png";
            if (IsFolderOfTrustAccounts)
                return "../../Resources/tree16/trust.png";
            if (IsFolderOfMyBankAccounts)
                return "../../Resources/tree16/account4.png";
            if (IsMyAccount)
                return "../../Resources/tree16/wallet2.png";
            if (IsIncomeTag)
                return "../../Resources/tree16/plus3.png";
            if (IsExpenseTag)
                return "../../Resources/tree16/minus3.png";

            return "../../Resources/tree16/counterparty.png";
        }
    }
}