using System;
using System.Windows.Media;
using KeeperDomain;

namespace Keeper2018
{
    public class DepositVm
    {
        public int Id { get; set; }
        public bool IsSelected { get; set; }
        public string BankName { get; set; }
        public string DepoName { get; set; }

        public CurrencyCode MainCurrency { get; set; }

        public RateType RateType { get; set; }
        public string RateFormula { get; set; }

        public string RateTypeStr =>
            RateType == RateType.Fixed ? "фикс" : RateType == RateType.Floating ? "плав" : RateFormula;

        public decimal Rate { get; set; }
        public string AdditionsStr { get; set; }
        public bool IsAddOpen { get; set; }

        public Brush BackgroundBrush => FinishDate < DateTime.Today
            ? Brushes.LightPink
            : IsAddOpen
                ? Brushes.PaleGreen
                : Brushes.LightGray;

        public DateTime StartDate { get; set; }
        public DateTime FinishDate { get; set; }

        public Balance Balance { get; set; }
    }
}