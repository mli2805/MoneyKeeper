namespace Keeper2018
{
    public static class Folder
    {
        public static int MyAccounts = 158;
        public static int BankAccounts = 159;
        public static int PayCards = 161;
        public static int Deposits = 166;
        public static int Trusts = 902;
        public static int Debts = 866;

        public static int Closed = 393;

        public static int IncomeTags = 185;
        public static int ExpenseTags = 189;

        public static bool IsTag(this AccountItemModel account) { return account.Is(IncomeTags) || account.Is(ExpenseTags); }

        public static bool IsMyAccount(this AccountItemModel account) { return account.Is(MyAccounts); }
    }
}