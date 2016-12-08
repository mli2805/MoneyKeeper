using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using Keeper.DomainModel.DbTypes;
using Keeper.DomainModel.Enumes;
using Keeper.Utils.DiagramDomainModel;
using OxyPlot;

namespace Keeper.Utils.DiagramOxyPlots
{
    [Export]
    public class RatesOxyplotDataProvider
    {
        private readonly KeeperDb _db;

        public DiagramData Get()
        {
            var diagramData = new DiagramData() {Caption = "Курсы валют", Mode = DiagramMode.Lines, Series = new List<DiagramSeries>(), TimeInterval = Every.Day};
            diagramData.Series.Add(GetNbUsdRate());
            diagramData.Series.Add(GetNbEurRate());
            diagramData.Series.Add(GetNbRurRate());
            diagramData.Series.Add(GetNbBasket1());
            diagramData.Series.Add(GetNbBasket2());
            diagramData.Series.Add(GetNbBasket3());
            diagramData.Series.Add(GetNbEurUsdRate());
            diagramData.Series.Add(GetMyUsdRate());
            diagramData.Series.Add(GetRurUsdRate());
            return diagramData;
        }

        [ImportingConstructor]
        public RatesOxyplotDataProvider(KeeperDb db)
        {
            _db = db;
        }

        private DiagramSeries GetNbUsdRate()
        {
            var diagramSeries = new DiagramSeries(){Index = 0, Name = "Usd НБ РБ", OxyColor = OxyColors.Green, Points = new List<DiagramPoint>()};
            foreach (var rate in _db.OfficialRates)
            {
                diagramSeries.Points.Add(new DiagramPoint(rate.Date,rate.UsdRate));
            }
            return diagramSeries;
        }

        private DiagramSeries GetNbEurRate()
        {
            var diagramSeries = new DiagramSeries() { Index = 1, Name = "Euro НБ РБ", OxyColor = OxyColors.Blue, Points = new List<DiagramPoint>() };
            foreach (var rate in _db.OfficialRates.Where(rate => rate.Date > new DateTime(1999,1,10)))
            {
                diagramSeries.Points.Add(new DiagramPoint(rate.Date,rate.EurRate));
            }
            return diagramSeries;
        }

        private DiagramSeries GetNbRurRate()
        {
            var diagramSeries = new DiagramSeries() { Index = 2, Name = "Rur НБ РБ", OxyColor = OxyColors.Red, Points = new List<DiagramPoint>() };
            foreach (var rate in _db.OfficialRates)
            {
                diagramSeries.Points.Add(new DiagramPoint(rate.Date, rate.RurRate));
            }
            return diagramSeries;
        }

        private DiagramSeries GetNbBasket1()
        {
            var diagramSeries = new DiagramSeries() { Index = 31, Name = "Корзина НБ РБ 33-33-33", OxyColor = OxyColors.Orange, Points = new List<DiagramPoint>() };
            foreach (var rate in _db.OfficialRates.Where(rate => rate.Date >= new DateTime(2009,1,1)))
            {
                diagramSeries.Points.Add(new DiagramPoint(rate.Date, rate.Busket1));
            }
            return diagramSeries;
        }

        private DiagramSeries GetNbBasket2()
        {
            var diagramSeries = new DiagramSeries() { Index = 32, Name = "Корзина НБ РБ 30-30-40", OxyColor = OxyColors.DarkOrange, Points = new List<DiagramPoint>() };
            foreach (var rate in _db.OfficialRates.Where(rate => rate.Date >= new DateTime(2009,1,1)))
            {
                diagramSeries.Points.Add(new DiagramPoint(rate.Date, rate.Busket2));
            }
            return diagramSeries;
        }

        private DiagramSeries GetNbBasket3()
        {
            var diagramSeries = new DiagramSeries() { Index = 33, Name = "Корзина НБ РБ 30-20-50", OxyColor = OxyColors.OrangeRed, Points = new List<DiagramPoint>() };
            foreach (var rate in _db.OfficialRates.Where(rate => rate.Date >= new DateTime(2009,1,1)))
            {
                diagramSeries.Points.Add(new DiagramPoint(rate.Date, rate.Busket3));
            }
            return diagramSeries;
        }

        private DiagramSeries GetNbEurUsdRate()
        {
            var diagramSeries = new DiagramSeries() { Index = 5, Name = "Eur/Usd (НБ РБ)", OxyColor = OxyColors.Blue, Points = new List<DiagramPoint>() };
            foreach (var rate in _db.OfficialRates.Where(rate => rate.Date > new DateTime(1999, 1, 10)))
            {
                diagramSeries.Points.Add(new DiagramPoint(rate.Date, rate.EurRate/rate.UsdRate));
            }
            return diagramSeries;
        }

        private DiagramSeries GetMyUsdRate()
        {
            var diagramSeries = new DiagramSeries() { Index = 4, Name = "Usd мой", OxyColor = OxyColors.LightGreen, Points = new List<DiagramPoint>() };
            foreach (var rate in _db.CurrencyRates.Where(rate => rate.Currency == CurrencyCodes.BYR))
            {
                diagramSeries.Points.Add(new DiagramPoint(rate.BankDay, rate.Rate));
            }
            return diagramSeries;
        }
        private DiagramSeries GetRurUsdRate()
        {
            var diagramSeries = new DiagramSeries() { Index = 6, Name = "Rur / Usd ", OxyColor = OxyColors.LightPink, Points = new List<DiagramPoint>() };
            foreach (var rate in _db.OfficialRates)
            {
                diagramSeries.Points.Add(new DiagramPoint(rate.Date, rate.UsdRate / rate.RurRate));
            }
            return diagramSeries;
        }
    }
}
