using System;
using System.Collections.Generic;
using System.Linq;

namespace Keeper2018
{
    public class DepositReportModel
    {
        private readonly KeeperDb _db;
        public DepositReportModel(KeeperDb db) { _db = db; }

        public Deposit Deposit
        {
            get => _deposit;
            set
            {
                _deposit = value;
                DepositOffer = _db.Bin.DepositOffers.First(o => o.Id == _deposit.DepositOfferId);
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


        private decimal DevaluationUsd =>
            _db.BalanceInUsd(DateTime.Today.GetEndOfDate(), Balance) - ContributionUsd - RevenueUsd + ConsumptionUsd;

        private decimal FinResultUsd => RevenueUsd + DevaluationUsd;


        public string ContributionStr => $"Внесено {Contribution} ( {ContributionUsd:0.00} usd )";
        public string RevenueStr => $"+ Проценты {Revenue} ( {RevenueUsd:0.00} usd )";
        public string СonsumptionStr => $"- Снято {Сonsumption} ( {ConsumptionUsd:0.00} usd )";
        public string BalanceStr => $"- Остаток {_db.BalanceInUsdString(DateTime.Today.GetEndOfDate(), Balance)}";
        public string DevaluationStr => $"= Курсовые разницы {DevaluationUsd:0.00} usd";
        public string FinResultStr => $"Текущий профит {FinResultUsd:0.00} usd";



        public OneRate RateStart;
        public OneRate RateNow;
        public OneRate RateForecast;
        private Deposit _deposit;
        public string RateStr { get; set; }

        public Balance AllRevenue { get; set; } = new Balance();
        public string AllRevenueStr => $"Ожидается процентов {_db.BalanceInUsdString(DateTime.Today.GetEndOfDate(), AllRevenue)}";
        public string Forecast { get; set; }

    }

    public static class DepositReportModelExt
    {
        public static void Foresee(this DepositReportModel model, KeeperDb db)
        {
            ForeseeRate(model, db);



        }

        private static void ForeseeRate(DepositReportModel model, KeeperDb db)
        {
            if (model.DepositOffer.MainCurrency != CurrencyCode.USD)
            {
                model.RateStart = db.GetRate(model.Deposit.StartDate, model.DepositOffer.MainCurrency);
                model.RateNow = db.GetRate(DateTime.Today, model.DepositOffer.MainCurrency);

                model.RateForecast = new OneRate();
                model.RateForecast.Unit = model.RateNow.Unit;
                var k = (model.RateNow.Value - model.RateStart.Value) / (DateTime.Today - model.Deposit.StartDate).Days;
                model.RateForecast.Value = model.RateStart.Value + k * (model.Deposit.FinishDate - model.Deposit.StartDate).Days;

                model.RateStr =
                    $"Курс по НБ:  {model.RateStart.Value:0.0000} => {model.RateNow.Value:0.0000} => {model.RateForecast.Value:0.0000}";
            }
        }
    }
}