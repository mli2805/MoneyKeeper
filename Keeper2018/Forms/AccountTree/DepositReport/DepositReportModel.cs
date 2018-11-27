using System;
using System.Collections.Generic;

namespace Keeper2018
{
    public class Money
    {
        public decimal Amount { get; set; }
        public CurrencyCode Currency { get; set; }

        public override string ToString()
        {
            return $"{Amount} {Currency.ToString().ToLower()}";
        }
    }
    public class DepositReportTrafficLine
    {
        public DateTime Date { get; set; }
        public Money Before { get; set; }
        public Money Income { get; set; }
        public Money Outcome { get; set; }
        public Money After { get; set; }
        public string Comment { get; set; }

    }
    public class DepositReportModel
    {
        public string DepositName { get; set; }
        public string DepositState { get; set; }

        public List<DepositReportTrafficLine> Traffic { get; set; } = new List<DepositReportTrafficLine>();

        public string Balance { get; set; }
        public string Facts { get; set; }
        public string MonthInterest { get; set; }
        public string AllInterests { get; set; }
        public string Forecast { get; set; }
    }
}