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

        [ImportingConstructor]
        public RatesOxyplotDataProvider(KeeperDb db)
        {
            _db = db;
        }

        public DiagramData Get()
        {
            var diagramData = new DiagramData() {Caption = "Курсы валют", Mode = DiagramMode.Lines, Series = new List<DiagramSeries>(), TimeInterval = Every.Day};
            diagramData.Series.Add(GetNbUsdRate());
            return diagramData;
        }

        private DiagramSeries GetNbUsdRate()
        {
            var diagramSeries = new DiagramSeries(){Index = 0, Name = "Usd НБ РБ", PositiveBrushColor = System.Windows.Media.Brushes.Green, Points = new List<DiagramPoint>()};
            foreach (var rate in _db.OfficialRates.Where(rate => Math.Abs(rate.UsdRate) > 0.000000001))
            {
                diagramSeries.Points.Add(new DiagramPoint(rate.Date,rate.UsdRate));
            }
            return diagramSeries;
        }
    }
}
