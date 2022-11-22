using System;
using KeeperDomain;

namespace Keeper2018
{
    public class DepositVm
    {
        public int Id { get; set; }
        public string BankName { get; set; }
        public string DepoName { get; set; }

        public CurrencyCode MainCurrency { get; set; }

        public string RateTypeStr { get; set; }
        public string RateFormula { get; set; }
        public string AdditionsStr { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime FinishDate { get; set; }

        public Balance Balance { get; set; }
    }
}