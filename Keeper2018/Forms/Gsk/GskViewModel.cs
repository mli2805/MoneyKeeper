using System;
using System.Collections.ObjectModel;
using System.Linq;
using Caliburn.Micro;
using KeeperDomain;

namespace Keeper2018
{
    public class GskViewModel : Screen
    {
        private readonly KeeperDataModel _dataModel;
        private PaymentLineModel _selectedRow;
        public ObservableCollection<PaymentLineModel> Rows { get; set; }

        public PaymentLineModel SelectedRow
        {
            get => _selectedRow;
            set
            {
                if (Equals(value, _selectedRow)) return;
                _selectedRow = value;
                NotifyOfPropertyChange();
            }
        }

        public int NumberOfMadePayments { get; set; }
        public int NumberOfFuturePayments { get; set; }
        public int TotalNumberOfPayments { get; set; } = 480; // 40 years * 12

        public decimal PaidAmountInUsd { get; set; }
        public decimal ForecastAmountInUsd { get; set; }

        public decimal TotalAmountInUsd { get; set; }

        public GskViewModel(KeeperDataModel dataModel)
        {
            _dataModel = dataModel;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Погашение кредита ЖСК";
        }

        public void Initialize()
        {
            Rows = new ObservableCollection<PaymentLineModel>();
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
                Rows.Add(paymentLine);
                PaidAmountInUsd += amountIsUsd;
            }
            NumberOfMadePayments = list.Count;

            NumberOfFuturePayments = TotalNumberOfPayments - NumberOfMadePayments;
            ForecastAmountInUsd = NumberOfFuturePayments 
                                        * _dataModel.AmountInUsd(DateTime.Today, CurrencyCode.BYN, 9);
            TotalAmountInUsd = PaidAmountInUsd + ForecastAmountInUsd;

            SelectedRow = Rows.Last();
        }

    }
}
