using System;
using Keeper.Utils.Common;
using Keeper.Utils.Diagram;
using Keeper.Utils.DiagramDomainModel;

namespace Keeper.Controls.PeriodChoice
{
    public class PeriodChoiceControlPointsConvertor
    {
        private readonly YearMonth _firstDate;
        private readonly YearMonth _lastDate;
        private readonly double _delta;
        private readonly double _pointsInMonth;
        private readonly DiagramIntervalMode _intervalMode;

        public PeriodChoiceControlPointsConvertor(YearMonth firstDate, YearMonth lastDate, DiagramIntervalMode intervalMode)
        {
            _firstDate = firstDate;
            _lastDate = lastDate;
            _delta = YearMonth.PeriodInMonths(_lastDate, _firstDate);
            _pointsInMonth = 100/_delta;
            _intervalMode = intervalMode;
        }


        private YearMonth PointToYearMonth(double point)
        {
            var monthNumber = (int)(point / _pointsInMonth);
            return _firstDate.AddMonth(monthNumber);
        }
        public Tuple<YearMonth, YearMonth> PointsToYearMonth(double fromPoint, double toPoint)
        {
//            Console.WriteLine(" {0}   {1}", fromPoint, toPoint);
            return new Tuple<YearMonth, YearMonth>(PointToYearMonth(fromPoint), PointToYearMonth(toPoint));
        }

        public void YearMonthPeriodToPoints(Tuple<YearMonth, YearMonth> period, out double fromPoint, out double toPoint)
        {
            fromPoint = YearMonth.PeriodInMonths(period.Item1, _firstDate)/_delta;
            toPoint = YearMonth.PeriodInMonths(period.Item2, _firstDate)/_delta;
        }
    }
}
