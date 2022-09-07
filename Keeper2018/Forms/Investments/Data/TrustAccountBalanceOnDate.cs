using System;
using System.Collections.Generic;

namespace Keeper2018
{
    public class TrustAccountBalanceOnDate
    {
        public DateTime Date { get; set; }
        public decimal Cash { get; set; }

        public decimal OperationsFee { get; set; }
        public decimal BaseFee { get; set; }
        public decimal AllPaidFees { get; set; }
        public decimal NotPaidFees { get; set; }

        public decimal TopUp { get; set; }
        public decimal ReceivedCoupon { get; set; }
        public decimal Withdraw { get; set; }
        public decimal Externals { get; set; }
        public decimal AllCurrentActives { get; set; }
        public decimal FinResult { get; set; }

        public decimal Balance => Cash + AllCurrentActives;

        public List<InvestmentAssetOnDate> Assets { get; } = new List<InvestmentAssetOnDate>();
    }
}