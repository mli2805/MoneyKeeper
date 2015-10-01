using System;
using Keeper.DomainModel;
using Keeper.Utils.Common;
using Keeper.Utils.Diagram;

namespace Keeper.Utils.OxyPlots
{
    public class BlackBox
    {
        private readonly YearMonth _firstDate;
        private readonly YearMonth _lastDate;
        private readonly DiagramIntervalMode _intervalMode;

        public BlackBox(YearMonth firstDate, YearMonth lastDate, DiagramIntervalMode intervalMode)
        {
            _firstDate = firstDate;
            _lastDate = lastDate;
            _intervalMode = intervalMode;
        }

        private DateTime DateFromIndex(double index)
        {
            var year = (int)index/12;
            var month = (int)(index - year*12);
            return _firstDate.AddYears(year).AddMonths(month);
        }
        public Tuple<double, double> DatesToPoints(DateTime fromDate, DateTime toDate)
        {
            double fromDatePosition = DateIndex(fromDate) / DateIndex(_lastDate) * 100;
            double toDatePosition = DateIndex(toDate) / (DateIndex(_lastDate) + 1) * 100;

            return new Tuple<double, double>(fromDatePosition, toDatePosition);
        }

        public Tuple<double, double> PeriodToPoints(Period period)
        {
            return DatesToPoints(period.Start, period.Finish);
        }

        public Tuple<DateTime, DateTime> PointsToDates(double fromPoint, double toPoint)
        {
            var lastDateIndex = DateIndex(_lastDate);
            var firstDateIndex = DateIndex(_firstDate);
            var indexFrom = fromPoint/100 * (lastDateIndex - firstDateIndex) + firstDateIndex;
            var indexTo = toPoint / 100 * (lastDateIndex - firstDateIndex) + firstDateIndex;

            return new Tuple<DateTime, DateTime>(DateFromIndex(indexFrom), DateFromIndex(indexTo));
        }

    }
}
