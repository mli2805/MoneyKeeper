using System;
using System.Collections.ObjectModel;

namespace Keeper.DomainModel
{
    [Serializable]
    public class Deposit : ICloneable
    {
        public Deposit()
        {
            ProcentsEvaluated = new DepositProcentsCalculatingRules();
            DepositRateLines = new ObservableCollection<DepositRateLine>();
        }

        public Account ParentAccount { get; set; }
        public Account Bank { get; set; }
        public string Title { get; set; }
        public string AgreementNumber { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime FinishDate { get; set; }
        public CurrencyCodes Currency { get; set; }

        public ObservableCollection<DepositRateLine> DepositRateLines { get; set; }
        public string Comment { get; set; }

        public DepositProcentsCalculatingRules ProcentsEvaluated { get; set; }

        [NonSerialized]
        private DepositCalculatedTotals _evaluations;
        public DepositCalculatedTotals Evaluations
        {
            get { return _evaluations; }
            set { _evaluations = value; }
        }

        public object Clone()
        {
            var newdDeposit = (Deposit)this.MemberwiseClone();
            if (DepositRateLines != null) newdDeposit.DepositRateLines = new ObservableCollection<DepositRateLine>(DepositRateLines);
            return newdDeposit;
        }
    }
}
