using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace Keeper.DomainModel.Deposit
{
    public class DepositCalculationData
    {
        public DepositStates State { get; set; }
        public List<DepositTransaction> Traffic { get; set; }
        public List<DepositDailyLine> DailyTable { get; set; }
        public decimal TotalMyIns { get; set; }
        public decimal TotalPercent { get; set; }
        public decimal TotalPercentInUsd { get; set; }
        public decimal TotalMyOuts { get; set; }
        public decimal TotalMyOutsInUsd { get; set; }
        public decimal CurrentBalance { get { return TotalMyIns + TotalPercent - TotalMyOuts; } }
        public decimal CurrentProfitInUsd { get; set; }
        public decimal CurrentDevaluationInUsd { get; set; }

        public DepositEstimations Estimations { get; set; }


        public Brush FontColor { get { return State == DepositStates.Закрыт ? Brushes.Gray : State == DepositStates.Просрочен ? Brushes.Red : Brushes.Blue; } }
    }

    public class DepositEstimations
    {
        public decimal ProcentsInThisMonth { get; set; }
        public decimal CurrencyRateOnThisMonthPayment { get; set; }
        public decimal ProcentsUpToFinish { get; set; }
        public decimal CurrencyRateOnFinish { get; set; }
        public decimal DevaluationInUsd { get; set; }
        public decimal ProfitInUsd { get; set; }
    }
}