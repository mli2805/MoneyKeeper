using System;

namespace Keeper2018
{
    public class ReportLine
    {
        public DateTime Date { get; set; }
        public Balance Before { get; set; }
        public Balance Income { get; set; }
        public Balance Outcome { get; set; }
        public Balance After { get; set; }
        public string Comment { get; set; }

        public DepositOperationType Type { get; set; }
    }

    public enum DepositOperationType
    {
        Contribution,
        Revenue,
        Consumption,
        Exchange,
    }
}