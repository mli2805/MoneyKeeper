using System.Collections.Generic;
using System.Composition;
using System.Windows.Media;
using Keeper.DomainModel.Enumes;
using Keeper.Utils.DiagramDomainModel;

namespace Keeper.Utils.DiagramDataExtraction
{
    [Export]
    public class BalancesDiagramsDataFactory
    {
        private readonly BalancesDiagramsDataExtractor _balancesDiagramsDataExtractor;

        [ImportingConstructor]
        public BalancesDiagramsDataFactory(BalancesDiagramsDataExtractor balancesDiagramsDataExtractor)
        {
            _balancesDiagramsDataExtractor = balancesDiagramsDataExtractor;
        }

        public DiagramData DailyBalancesCtor()
        {
            return new DiagramData
            {
                Caption = "Располагаемые средства",
                Series = new List<DiagramSeries>
                {
                    new DiagramSeries
                    {
                        Points = _balancesDiagramsDataExtractor.GetBalances(Every.Day),
                        Index = 0,
                        Name = "Мои",
                        PositiveBrushColor = Brushes.Blue
                    },
                },
                Mode = DiagramMode.Lines,
                TimeInterval = Every.Day
            };
        }

        public DiagramData MonthlyResultsDiagramCtor()
        {
            var dataForDiagram = new List<DiagramSeries>
            {
                new DiagramSeries
                {
                    Name = "Сальдо",
                    PositiveBrushColor = Brushes.Blue,
                    NegativeBrushColor = Brushes.Red,
                    Index = 0,
                    Points = _balancesDiagramsDataExtractor.MonthlyResults(),
                }
            };

            return new DiagramData
            {
                Caption = "Сальдо",
                Series = dataForDiagram,
                Mode = DiagramMode.BarVertical,
                TimeInterval = Every.Month
            };
        }

    }
}
