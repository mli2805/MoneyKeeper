using System;
using System.Collections.ObjectModel;

namespace Keeper.DomainModel
{
    [Serializable]
    public class Deposit : ICloneable
    {
          public Account Bank { get; set; }
          public string Title { get; set; }
          public CurrencyCodes Currency { get; set; }
          public DepositProcentsCalculatingRules ProcentsCalculatingRules { get; set; }
          public ObservableCollection<DepositRateLine> RateLines { get; set; }

        public BankDepositOffer DepositOffer { get; set; }

        public string AgreementNumber { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime FinishDate { get; set; }
        public string Comment { get; set; }
        public Account ParentAccount { get; set; }

        [NonSerialized]
        private DepositCalculatedTotals _calculatedTotals;
        public DepositCalculatedTotals CalculatedTotals
        {
            get { return _calculatedTotals; }
            set { _calculatedTotals = value; }
        }

        public Deposit()
        {
            ProcentsCalculatingRules = new DepositProcentsCalculatingRules();
            RateLines = new ObservableCollection<DepositRateLine>();
        }

        public object Clone()
        {
            var newdDeposit = (Deposit)this.MemberwiseClone();
            if (RateLines != null) newdDeposit.RateLines = new ObservableCollection<DepositRateLine>(RateLines);
            return newdDeposit;
        }
    }
}
