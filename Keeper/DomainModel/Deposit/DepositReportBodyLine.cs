using System;

namespace Keeper.DomainModel.Deposit
{
    public class DepositReportBodyLine
    {
        public DateTime Day { get; set; }
        public decimal BeforeOperation { get; set; }
        public decimal IncomeColumn { get; set; }
        public decimal ExpenseColumn { get; set; }
        public decimal AfterOperation { get; set; }
        public string Comment { get; set; }
    }
}