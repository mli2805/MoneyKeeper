using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using Keeper.DomainModel;
using Keeper.Utils.Common;
using Keeper.Utils.DiagramDomainModel;

namespace Keeper.Utils.OxyPlots
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
            diagramData.Series.Add(GetNbBasket());
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
            var diagramSeries = new DiagramSeries(){Index = 0, Name = "Usd НБ РБ", OxyColor = OxyPlot.OxyColors.Green, Points = new List<DiagramPoint>()};
            foreach (var rate in _db.OfficialRates)
            {
                diagramSeries.Points.Add(new DiagramPoint(rate.Date,rate.UsdRate));
            }
            return diagramSeries;
        }

        private DiagramSeries GetNbEurRate()
        {
            var diagramSeries = new DiagramSeries() { Index = 1, Name = "Euro НБ РБ", OxyColor = OxyPlot.OxyColors.Blue, Points = new List<DiagramPoint>() };
            foreach (var rate in _db.OfficialRates.Where(rate => rate.Date > new DateTime(1999,1,10)))
            {
                diagramSeries.Points.Add(new DiagramPoint(rate.Date,rate.EurRate));
            }
            return diagramSeries;
        }

        private DiagramSeries GetNbRurRate()
        {
            var diagramSeries = new DiagramSeries() { Index = 2, Name = "Rur НБ РБ", OxyColor = OxyPlot.OxyColors.Red, Points = new List<DiagramPoint>() };
            foreach (var rate in _db.OfficialRates)
            {
                diagramSeries.Points.Add(new DiagramPoint(rate.Date, rate.RurRate));
            }
            return diagramSeries;
        }

        private DiagramSeries GetNbBasket()
        {
            var diagramSeries = new DiagramSeries() { Index = 3, Name = "Корзина НБ РБ", OxyColor = OxyPlot.OxyColors.Orange, Points = new List<DiagramPoint>() };
            foreach (var rate in _db.OfficialRates.Where(rate => rate.Date >= new DateTime(2015,1,1)))
            {
                diagramSeries.Points.Add(new DiagramPoint(rate.Date, Math.Pow(rate.UsdRate,0.3)*Math.Pow(rate.EurRate,0.3)*Math.Pow(rate.RurRate,0.4)));
            }
            return diagramSeries;
        }

        private DiagramSeries GetNbEurUsdRate()
        {
            var diagramSeries = new DiagramSeries() { Index = 5, Name = "Eur/Usd (НБ РБ)", OxyColor = OxyPlot.OxyColors.Blue, Points = new List<DiagramPoint>() };
            foreach (var rate in _db.OfficialRates.Where(rate => rate.Date > new DateTime(1999, 1, 10)))
            {
                diagramSeries.Points.Add(new DiagramPoint(rate.Date, rate.EurRate/rate.UsdRate));
            }
            return diagramSeries;
        }

        private DiagramSeries GetMyUsdRate()
        {
            var diagramSeries = new DiagramSeries() { Index = 4, Name = "Usd мой", OxyColor = OxyPlot.OxyColors.LightGreen, Points = new List<DiagramPoint>() };
            foreach (var rate in _db.CurrencyRates.Where(rate => rate.Currency == CurrencyCodes.BYR))
            {
                diagramSeries.Points.Add(new DiagramPoint(rate.BankDay, rate.Rate));
            }
            return diagramSeries;
        }
        private DiagramSeries GetRurUsdRate()
        {
            var diagramSeries = new DiagramSeries() { Index = 6, Name = "Rur / Usd ", OxyColor = OxyPlot.OxyColors.LightPink, Points = new List<DiagramPoint>() };
            foreach (var rate in _db.OfficialRates)
            {
                diagramSeries.Points.Add(new DiagramPoint(rate.Date, rate.UsdRate / rate.RurRate));
            }
            return diagramSeries;
        }
    }
}
