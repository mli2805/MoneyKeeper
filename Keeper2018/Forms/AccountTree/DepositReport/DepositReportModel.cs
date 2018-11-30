using System;
using System.Collections.Generic;

namespace Keeper2018
{
    public class DepositReportModel
    {
        private readonly KeeperDb _db;

        public DepositReportModel(KeeperDb db)
        {
            _db = db;
        }

        public string DepositName { get; set; }
        public string DepositState { get; set; }
        public string BalanceInHeader => $"Остаток составляет {_db.BalanceInUsdString(DateTime.Today.GetEndOfDate(), Balance)}";


        public List<ReportLine> Traffic { get; set; } = new List<ReportLine>();
        public Balance Contribution { get; set; } = new Balance(); // I put on depo
        public decimal ContributionUsd { get; set; }
        public Balance Revenue { get; set; } = new Balance(); // Bank put procents
        public decimal RevenueUsd { get; set; }
        public Balance Сonsumption { get; set; } = new Balance(); // I get from depo
        public decimal ConsumptionUsd { get; set; }

        public Balance Balance { get; set; } = new Balance();

        private decimal DevaluationUsd => 
            _db.BalanceInUsd(DateTime.Today.GetEndOfDate(), Balance) - ContributionUsd - RevenueUsd + ConsumptionUsd;

        private decimal FinResultUsd => RevenueUsd + DevaluationUsd;


        public string ContributionStr => $"Внесено {Contribution} ( {ContributionUsd:0.00} usd )";
        public string RevenueStr => $"+ Проценты {Revenue} ( {RevenueUsd:0.00} usd )";
        public string СonsumptionStr => $"- Снято {Сonsumption} ( {ConsumptionUsd:0.00} usd )";
        public string BalanceStr => $"- Остаток {_db.BalanceInUsdString(DateTime.Today.GetEndOfDate(), Balance)}";
        public string DevaluationStr => $"= Курсовые разницы {DevaluationUsd:0.00} usd";
        public string FinResultStr => $"= Курсовые разницы {FinResultUsd:0.00} usd";




        public string MonthInterest { get; set; }
        public string AllInterests { get; set; }
        public string Forecast { get; set; }

    }
}