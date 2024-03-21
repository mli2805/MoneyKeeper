namespace Keeper2018
{
    public static class NickNames
    {
        public static readonly int MyAccounts = 158;
        public static readonly int BankAccounts = 159;
        public static readonly int PayCards = 161;
        public static readonly int Deposits = 166;
        public static readonly int Trusts = 902;
        public static readonly int Debts = 866;

        public static readonly int Closed = 393;

        public static readonly int IncomeTags = 185;
        public static readonly int ExpenseTags = 189;

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

        public static AccountItemModel MoneyBackTag(this KeeperDataModel dataModel)
        {
            return dataModel.AcMoDict[701];
        }

        public static AccountItemModel PercentsTag(this KeeperDataModel dataModel)
        {
            return dataModel.AcMoDict[208];
        }
        public static AccountItemModel CardFeeTag(this KeeperDataModel dataModel)
        {
            return dataModel.AcMoDict[847];
        }
    }

}