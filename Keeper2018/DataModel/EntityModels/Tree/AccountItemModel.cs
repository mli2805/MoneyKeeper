namespace Keeper2018
{
    public class AccountItemModel : TreeViewItemModel
    {
        public string Img => GetIconPath();
        public bool IsFolder { get; set; }

        public BankAccountModel BankAccount { get; set; }
        public bool IsBankAccount => BankAccount != null;

        public bool IsDeposit => BankAccount != null && BankAccount.Deposit != null;
        public bool IsCard => BankAccount != null && BankAccount.PayCard != null; // in XAML

        // my accounts do not use associations
        public int AssociatedIncomeId { get; set; } // for external
        public int AssociatedExpenseId { get; set; } // for external
        public int AssociatedExternalId { get; set; } // for tag

        public string ShortName { get; set; }
        public string ButtonName { get; set; } // face of shortcut button (if exists)
        public string Comment { get; set; }

        public override string ToString() => Name;

        public string GetToolTip => IsCard ? BankAccount.PayCard.CardNumber : null;


        public AccountItemModel(int id, string name, TreeViewItemModel parent) : base(id, name, parent)
        {
        }

        private string GetIconPath()
        {
            if (IsFolder)
                return "../../Resources/tree16/yellow_folder.png";
            if (Is(NickNames.Closed))
                return "../../Resources/tree16/cross.png";
            if (IsCard)
                return "../../Resources/tree16/paycard4.png";
            if (IsDeposit)
                return "../../Resources/tree16/deposit7.png";
            if (Is(NickNames.Debts))
                return "../../Resources/tree16/hand_point_left.png";
            if (Is(NickNames.Trusts))
                return "../../Resources/tree16/trust.png";
            if (Is(NickNames.BankAccounts))
                return "../../Resources/tree16/account4.png";
            if (Is(NickNames.MyAccounts))
                return "../../Resources/tree16/wallet2.png";
            if (Is(NickNames.IncomeTags))
                return "../../Resources/tree16/plus3.png";
            if (Is(NickNames.ExpenseTags))
                return "../../Resources/tree16/minus3.png";

            return "../../Resources/tree16/counterparty.png";
        }
    }
}