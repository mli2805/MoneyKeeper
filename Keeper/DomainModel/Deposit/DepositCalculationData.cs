using System.Collections.Generic;
using System.Windows.Media;

namespace Keeper.DomainModel.Deposit
{
    public class DepositCalculationData
    {
        public DepositStates State { get; set; }
        public List<DepositTransaction> Traffic { get; set; }
        public List<DepositDailyLine> DailyTable { get; set; }
        public decimal TotalMyIns { get; set; }
        public decimal TotalMyInsInUsd { get; set; }
        public decimal TotalPercent { get; set; }
        public decimal TotalPercentInUsd { get; set; }
        public decimal TotalMyOuts { get; set; }
        public decimal TotalMyOutsInUsd { get; set; }
        public decimal CurrentBalance { get { return TotalMyIns + TotalPercent - TotalMyOuts; } }
        public decimal CurrentProfitInUsd { get; set; }
        public decimal CurrentDevaluationInUsd { get; set; }

        public DepositEstimations Estimations { get; set; }


        public Brush FontColor { get { return State == DepositStates.Закрыт ? Brushes.Gray : State == DepositStates.Просрочен ? Brushes.Red : Brushes.Blue; } }

        public DepositCalculationData()
        {
            Estimations = new DepositEstimations();
        }
    }
}