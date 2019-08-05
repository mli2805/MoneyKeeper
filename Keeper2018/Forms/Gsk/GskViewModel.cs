using System;
using System.Linq;
using Caliburn.Micro;

namespace Keeper2018
{
    public class GskViewModel : Screen
    {
        private readonly KeeperDb _db;
        public GskModel Model { get; set; }

        public GskViewModel(KeeperDb db)
        {
            _db = db;
        }

        public void Initialize()
        {
            Model = new GskModel();
            var list = _db.Bin.Transactions.Values.Where(t => t.Tags.Contains(285)).ToList(); // погашение кредита ЖСК
            foreach (var tr in list)
            {
                var paymentLine = new PaymentLineModel()
                {
                    Date = tr.Timestamp.ToShortDateString(),
                    Sum = _db.AmountInUsdString(tr, out decimal amountIsUsd),
                };
                Model.Rows.Add(paymentLine);
                Model.PaidAmountInUsd += amountIsUsd;
            }
            Model.NumberOfMadePayments = list.Count;

            Model.NumberOfFuturePayments = Model.TotalNumberOfPayments - Model.NumberOfMadePayments;
            Model.ForecastAmountInUsd = Model.NumberOfFuturePayments 
                                        * _db.AmountInUsd(DateTime.Today, CurrencyCode.BYN, 9);
            Model.TotalAmountInUsd = Model.PaidAmountInUsd + Model.ForecastAmountInUsd;
        }
    }
}
