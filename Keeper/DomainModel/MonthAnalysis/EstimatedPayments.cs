using System.Collections.Generic;

namespace Keeper.DomainModel.MonthAnalysis
{
    public class EstimatedPayments
    {
        public List<EstimatedMoney> Payments { get; set; }
        public decimal EstimatedSum { get; set; }
        public decimal TotalInUsd { get; set; }

        public EstimatedPayments()
        {
            Payments = new List<EstimatedMoney>();
        }
    }
}