using System;

namespace Keeper.DomainModel.Deposit
{
    public class DepositReportBodyLine
    {
        public DateTime Day { get; set; }
        public string BeforeOperation { get; set; }
        public string IncomeColumn { get; set; }
        public string ExpenseColumn { get; set; }
        public string AfterOperation { get; set; }
        public string Comment { get; set; }
    }
}