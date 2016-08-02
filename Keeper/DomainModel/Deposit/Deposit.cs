using System;
using System.Linq;
using Keeper.DomainModel.DbTypes;
using Keeper.DomainModel.Extentions;
using Keeper.DomainModel.WorkTypes;

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
        public string ShortName { get; set; }

        [NonSerialized]
        private DepositCalculationData _calculationData;
        public DepositCalculationData CalculationData
        {
            get { return _calculationData; }
            set { _calculationData = value; }
        }

        public object Clone()
        {
            var newdDeposit = (Deposit)MemberwiseClone();
            return newdDeposit;
        }

        public DateTime GetDateOfLastProcentTransaction()
        {
            var lastProcentTransaction =
                CalculationData.Traffic.LastOrDefault(t => t.TransactionType == DepositTransactionTypes.Проценты);
            return lastProcentTransaction == null ? StartDate : lastProcentTransaction.Timestamp;
        }
        public DateTime GetLastDayWhichShouldBePaidInAnalyzedMonth(DateTime firstDayOfAnalyzedMonth)
        {
            var upToDate = firstDayOfAnalyzedMonth;
            if (DepositOffer.CalculatingRules.EveryStartDay)
                upToDate = new DateTime(firstDayOfAnalyzedMonth.Year, firstDayOfAnalyzedMonth.Month, StartDate.Day);
            if (DepositOffer.CalculatingRules.EveryLastDayOfMonth)
                upToDate = firstDayOfAnalyzedMonth.AddMonths(1);
            if (firstDayOfAnalyzedMonth.IsMonthTheSame(FinishDate) || upToDate > FinishDate) upToDate = FinishDate;
            return upToDate;
        }

      /// <summary>
      /// Если запрос про месяц в будущем , то предполагается что все , что должно быть оплачено в предыдущих месяцах , будет таки оплачено.
      /// Если по вкладу есть долги, предполагается , что все они будут оплачены в текущем месяце, 
      /// в будущих периодах только с даты последней оплаты предудущего месяца, по последнюю оплачиваемую дату анализируемого месяца.
      /// А вот если вопрос по текущему месяцу, то все что неоплачено должно быть здесь указано.
      /// </summary>
      /// <param name="firstDayOfAnalyzedMonth"></param>
      /// <returns></returns>
      public Period GetPeriodWhichShouldBePaidInAnalysedMonth(DateTime firstDayOfAnalyzedMonth)
        {
            var startOfPeriod = firstDayOfAnalyzedMonth.IsMonthTheSame(DateTime.Today)
                ? GetDateOfLastProcentTransaction().AddDays(1)
                : GetLastDayWhichShouldBePaidInAnalyzedMonth(firstDayOfAnalyzedMonth.AddMonths(-1));
            return new Period(startOfPeriod,GetLastDayWhichShouldBePaidInAnalyzedMonth(firstDayOfAnalyzedMonth));
        }


    }
}
