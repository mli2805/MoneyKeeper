using System.Collections.Generic;
using System.Windows.Media;
using KeeperDomain;

namespace Keeper2018
{
    public class AssetAnalysisModel
    {
        public List<string> Content { get; set; } = new List<string>();
        public decimal ProfitInUsd { get; set; }
        public Brush Brush => ProfitInUsd > 0 ? Brushes.Blue : Brushes.Red;
    }

    public class AssetOnPeriodReportModel
    {
        public  Period Period { get; set; }

        public List<string> BeforeState { get; set; }
        public List<string> BeforeFeesAndCoupons { get; set; }

        public List<string> OnStartState { get; set; }
        public List<string> OnStartFeesAndCoupons { get; set; }
        public AssetAnalysisModel OnStartAnalysis { get; set; }

        public List<string> InBetweenTrans { get; set; }
        public List<string> InBetweenFeesAndCoupons { get; set; }

        public List<string> AtEndState { get; set; }
        public List<string> AtEndFeesAndCoupons { get; set; }
        public AssetAnalysisModel AtEndAnalysis { get; set; }
    }
}
