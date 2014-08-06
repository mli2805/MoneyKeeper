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
        private DepositCalculationData _calculationData;
        public DepositCalculationData CalculationData
        {
            get { return _calculationData; }
            set { _calculationData = value; }
        }

        public object Clone()
        {
            var newdDeposit = (Deposit)this.MemberwiseClone();
            return newdDeposit;
        }
    }
}
