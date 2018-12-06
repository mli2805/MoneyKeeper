using System;
using System.Collections.Generic;
using System.Linq;

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
        public string FinResultStr => $"Текущий профит ${FinResultUsd:0.00}";


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
        public string Forecast => $"Итоговый результат ${ForecastUsd:0.00}";
        public string Forecast2 { get; set; }
    }

    public static class DepositReportModelExt
    {
        public static void SummarizeFacts(this DepositReportModel model, KeeperDb db)
        {
            foreach (var reportLine in model.Traffic.Where(t => t.Type == DepositOperationType.Contribution))
            {
                model.Contribution.AddBalance(reportLine.Income);
                model.ContributionUsd += db.BalanceInUsd(reportLine.Date, reportLine.Income);
            }

            foreach (var reportLine in model.Traffic.Where(t => t.Type == DepositOperationType.Revenue))
            {
                model.Revenue.AddBalance(reportLine.Income);
                model.RevenueUsd += db.BalanceInUsd(reportLine.Date, reportLine.Income);
            }

            foreach (var reportLine in model.Traffic.Where(t => t.Type == DepositOperationType.Consumption))
            {
                model.Сonsumption.AddBalance(reportLine.Outcome);
                model.ConsumptionUsd += db.BalanceInUsd(reportLine.Date, reportLine.Outcome);
            }

            model.DevaluationUsd = db.BalanceInUsd(DateTime.Today.GetEndOfDate(), model.Balance)
                                   - model.ContributionUsd - model.RevenueUsd + model.ConsumptionUsd;
            model.FinResultUsd = model.RevenueUsd + model.DevaluationUsd;
        }

        public static void Foresee(this DepositReportModel model, KeeperDb db)
        {
            ForeseeRate(model, db);
            ForeseeRevenue(model);

            model.BalanceAtEndUsd = model.IsInUsd
                ? model.Balance.Currencies[CurrencyCode.USD]
                : model.Balance.Currencies[model.DepositOffer.MainCurrency] / (decimal)model.RateForecast.Value;
            model.ForecastUsd = model.BalanceAtEndUsd + model.MoreRevenueUsd - model.ContributionUsd + model.ConsumptionUsd;

            var procent = model.ForecastUsd / model.ContributionUsd * 100;
            var yearProcent = procent / (model.Deposit.FinishDate - model.Deposit.StartDate).Days * 365;
            model.Forecast2 = $"( {procent:0.00}%  ({yearProcent:0.00}% годовых )";
        }

        private static void ForeseeRevenue(DepositReportModel model)
        {
            var lastRevenue = model.Traffic.LastOrDefault(t => t.Type == DepositOperationType.Revenue);
            var lastRevenueDate = lastRevenue?.Date ?? model.Deposit.StartDate.AddDays(-1);

            model.MoreRevenue = model.DepositOffer.GetRevenue(model.Deposit, lastRevenueDate,
                model.Balance.Currencies[model.DepositOffer.MainCurrency]);
            model.MoreRevenueUsd = model.IsInUsd
                ? model.MoreRevenue
                : model.MoreRevenue / (decimal)((model.RateNow.Value + model.RateForecast.Value) / 2);
        }

        private static void ForeseeRate(DepositReportModel model, KeeperDb db)
        {
            if (!model.IsInUsd)
            {
                model.RateStart = db.GetRate(model.Deposit.StartDate, model.DepositOffer.MainCurrency);
                model.RateNow = db.GetRate(DateTime.Today, model.DepositOffer.MainCurrency);

                model.RateForecast = new OneRate();
                model.RateForecast.Unit = model.RateNow.Unit;
                var k = (model.RateNow.Value - model.RateStart.Value) / (DateTime.Today - model.Deposit.StartDate).Days;
                model.RateForecast.Value = model.RateStart.Value + k * (model.Deposit.FinishDate - model.Deposit.StartDate).Days;

                model.RateStr1 =
                    $"Курс по НБ:  {model.RateStart.Value:0.0000} => {model.RateNow.Value:0.0000} => {model.RateForecast.Value:0.0000}";

                var nowDeltaRate = model.RateNow.Value - model.RateStart.Value;
                var nowProcent = nowDeltaRate / model.RateStart.Value * 100;

                var deltaRate = model.RateForecast.Value - model.RateStart.Value;
                var procent = deltaRate / model.RateStart.Value * 100;

                var yearProcent = procent / (model.Deposit.FinishDate - model.Deposit.StartDate).Days * 365;
                model.RateStr2 = $"( уже {nowProcent:0.00}%;  к концу {procent:0.00}%  ( {yearProcent:0.00}% годовых )";
            }
        }
    }
}