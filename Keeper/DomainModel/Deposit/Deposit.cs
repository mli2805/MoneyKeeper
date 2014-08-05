using System;

namespace Keeper.DomainModel.Deposit
{
    [Serializable]
    public class Deposit : ICloneable
    {
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

        public object Clone()
        {
            var newdDeposit = (Deposit)this.MemberwiseClone();
            return newdDeposit;
        }
    }
}
