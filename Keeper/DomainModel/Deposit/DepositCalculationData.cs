using System.Collections.Generic;
using System.Composition;
using System.Windows.Media;
using Keeper.DomainModel.Enumes;
using Keeper.Utils.Rates;

namespace Keeper.DomainModel.Deposit
{
    [Export]
    public class DepositCalculationData
    {
        private readonly RateExtractor _rateExtractor;
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
        public CurrencyCodes CurrentCurrency { get; set; }
        public decimal CurrentProfitInUsd { get; set; }
        public decimal CurrentDevaluationInUsd { get; set; }

        public DepositEstimations Estimations { get; set; }


        public Brush FontColor { get { return State == DepositStates.Закрыт ? Brushes.Gray : State == DepositStates.Просрочен ? Brushes.Red : Brushes.Blue; } }

        [ImportingConstructor]
        public DepositCalculationData(RateExtractor rateExtractor)
        {
            _rateExtractor = rateExtractor;
            Estimations = new DepositEstimations();
        }

        public string CurrentBalanceToString()
        {
            if (CurrentCurrency == CurrencyCodes.USD) return $"{CurrentBalance:#,0.00} usd";

            decimal inUsd = CurrentBalance /(decimal)_rateExtractor.GetLastRate(CurrentCurrency);

            if (CurrentCurrency == CurrencyCodes.BYR) return $"{CurrentBalance:#,0} byr  ({inUsd:#,0.00} usd)";
            return $"{CurrentBalance:#,0.00} {CurrentCurrency.ToString().ToLower()}  ({inUsd:#,0.00} usd)";
        }
    }
}