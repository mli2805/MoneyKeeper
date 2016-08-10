using System.Collections.Generic;

namespace Keeper.DomainModel.WorkTypes
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