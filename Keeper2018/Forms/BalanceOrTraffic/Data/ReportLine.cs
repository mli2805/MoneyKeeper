using System;

namespace Keeper2018
{
    public class ReportLine
    {
        public DateTime Date { get; set; }
        public BalanceOfAccount Before { get; set; }
        public BalanceOfAccount Income { get; set; }
        public BalanceOfAccount Outcome { get; set; }
        public BalanceOfAccount After { get; set; }
        public string Comment { get; set; }

    }
}