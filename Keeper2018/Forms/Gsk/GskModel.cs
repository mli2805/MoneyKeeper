using System.Collections.Generic;

namespace Keeper2018
{
    public class GskModel
    {
        public List<PaymentLineModel> Rows { get; set; } = new List<PaymentLineModel>();
        public int NumberOfMadePayments { get; set; }
        public int NumberOfFuturePayments { get; set; }
        public int TotalNumberOfPayments { get; set; } = 480; // 40 years * 12

        public decimal PaidAmountInUsd { get; set; }
        public decimal ForecastAmountInUsd { get; set; }

        public decimal TotalAmountInUsd { get; set; }
    }
}
