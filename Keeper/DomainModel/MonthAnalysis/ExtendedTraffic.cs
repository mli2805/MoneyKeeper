using System.Collections.Generic;

namespace Keeper.DomainModel.MonthAnalysis
{
    public class ExtendedTraffic
    {
        public List<TranForAnalysis> Trans { get; set; }
        public decimal TotalInUsd { get; set; }

        public ExtendedTraffic()
        {
            Trans = new List<TranForAnalysis>();
        }
    }
}