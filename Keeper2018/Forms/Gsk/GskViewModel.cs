using System;
using System.Linq;
using Caliburn.Micro;
using KeeperDomain;

namespace Keeper2018
{
    public class GskViewModel : Screen
    {
        private readonly KeeperDataModel _dataModel;
        public GskModel Model { get; set; }

        public GskViewModel(KeeperDataModel dataModel)
        {
            _dataModel = dataModel;
        }

        public void Initialize()
        {
            Model = new GskModel();
            var list = _dataModel.Transactions.Values
                .Where(t => t.Tags.Select(tt=>tt.Id)
                    .Contains(285)).ToList(); // погашение кредита ЖСК
            foreach (var tr in list)
            {
                var paymentLine = new PaymentLineModel()
                {
                    Date = tr.Timestamp.ToShortDateString(),
                    Sum = _dataModel.AmountInUsdString(tr.Timestamp, tr.Currency, tr.Amount,  out decimal amountIsUsd),
                };
                Model.Rows.Add(paymentLine);
                Model.PaidAmountInUsd += amountIsUsd;
            }
            Model.NumberOfMadePayments = list.Count;

            Model.NumberOfFuturePayments = Model.TotalNumberOfPayments - Model.NumberOfMadePayments;
            Model.ForecastAmountInUsd = Model.NumberOfFuturePayments 
                                        * _dataModel.AmountInUsd(DateTime.Today, CurrencyCode.BYN, 9);
            Model.TotalAmountInUsd = Model.PaidAmountInUsd + Model.ForecastAmountInUsd;
        }
    }
}
