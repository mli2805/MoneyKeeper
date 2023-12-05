namespace Keeper2018
{
    public static class NickNames
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

        public static AccountItemModel MineRoot(this KeeperDataModel dataModel)
        {
            return dataModel.AcMoDict[158];
        }

        public static AccountItemModel IncomeRoot(this KeeperDataModel dataModel)
        {
            return dataModel.AcMoDict[185];
        }

        public static AccountItemModel ExpenseRoot(this KeeperDataModel dataModel)
        {
            return dataModel.AcMoDict[189];
        }
    }
   
}