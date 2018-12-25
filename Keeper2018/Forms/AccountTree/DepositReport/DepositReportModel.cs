using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace Keeper2018
{
    public class DepositReportModel
    {
        private readonly KeeperDb _db;
        public DepositReportModel(KeeperDb db) { _db = db; }

        public bool IsInUsd;
        public Deposit Deposit
        {
            get => _deposit;
            set
            {
                _deposit = value;
                DepositOffer = _db.Bin.DepositOffers.First(o => o.Id == _deposit.DepositOfferId);
                IsInUsd = DepositOffer.MainCurrency == CurrencyCode.USD;
            }
        }

        public DepositOffer DepositOffer { get; private set; }
        public Balance Balance { get; set; } = new Balance();
        public decimal AmountInUsd { get; set; }


        public string DepositName { get; set; }
        public string DepositState => AmountInUsd == 0 ? "Депозит закрыт." : "Действующий депозит.";
        public string BalanceInHeader => $"Остаток составляет {_db.BalanceInUsdString(DateTime.Today.GetEndOfDate(), Balance)}";

        public List<ReportLine> Traffic { get; set; } = new List<ReportLine>();
        public Balance Contribution { get; set; } = new Balance(); // I put on depo
        public decimal ContributionUsd { get; set; }
        public Balance Revenue { get; set; } = new Balance(); // Bank put procents
        public decimal RevenueUsd { get; set; }
        public Balance Сonsumption { get; set; } = new Balance(); // I get from depo
        public decimal ConsumptionUsd { get; set; }


        public decimal DevaluationUsd { get; set; }
        public decimal FinResultUsd { get; set; }


        public string ContributionStr => $"Внесено {Contribution}" + (IsInUsd ? "" : $" ( ${ContributionUsd:0.00} )");
        public string RevenueStr => $"+ Проценты {Revenue}" + (IsInUsd ? "" : $" ( ${RevenueUsd:0.00} )");
        public string СonsumptionStr => $"- Снято {Сonsumption}" + (IsInUsd ? "" : $" ( ${ConsumptionUsd:0.00} )");
        public string BalanceStr => $"- Остаток {_db.BalanceInUsdString(DateTime.Today.GetEndOfDate(), Balance)}";
        public string DevaluationStr => IsInUsd ? "" : $"= Курсовые разницы ${DevaluationUsd:0.00}";
        public Brush DevaluationBrush => DevaluationUsd > 0 ? Brushes.Blue : Brushes.Red;
        public string FinResultStr => $"Текущий профит ${FinResultUsd:0.00}";
        public Brush FinResultBrush => FinResultUsd > 0 ? Brushes.Blue : Brushes.Red;


        public OneRate RateStart;
        public OneRate RateNow;
        public OneRate RateForecast;
        private Deposit _deposit;
        public string RateStr1 { get; set; }
        public string RateStr2 { get; set; }
        public decimal MoreRevenue { get; set; }  // forecast
        public decimal MoreRevenueUsd { get; set; }  // forecast
        public decimal BalanceAtEndUsd { get; set; }
        public string MoreRevenueStr1 => $"Ожидается процентов {MoreRevenue:0.00} {DepositOffer.MainCurrency.ToString().ToLower()}";
        public string MoreRevenueStr2 => IsInUsd ? "" : $"( по среднему курсу {((RateNow.Value + RateForecast.Value) / 2):0.0000} = ${MoreRevenueUsd:0.00} )";
        public string BalanceAtEnd1 => $"Остаток на конец {Balance}";
        public string BalanceAtEnd2 => IsInUsd ? "" : $"( по курсу {RateForecast.Value:0.0000} = ${BalanceAtEndUsd:0.00} )";


        public decimal ForecastUsd { get; set; }
        public string Forecast => $"Итоговый профит ${ForecastUsd:0.00}";
        public Brush ForecastBrush => ForecastUsd > 0 ? Brushes.Blue : Brushes.Red;
        public string Forecast2 { get; set; }
    }
}