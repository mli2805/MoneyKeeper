using System;

namespace Keeper2018
{
    public class PointsPeriodConvertor
    {
        private readonly Tuple<DateTime, DateTime> _initialPeriod;
        private readonly double _percentForOneInterval;
        private readonly DiagramIntervalMode _intervalMode;

        public PointsPeriodConvertor(Tuple<DateTime, DateTime> initialPeriod, DiagramIntervalMode intervalMode)
        {
            double intervalCount;
            _initialPeriod = initialPeriod;
            _intervalMode = intervalMode;

            switch (intervalMode)
            {
                case DiagramIntervalMode.Years:
                    intervalCount = initialPeriod.Item2.Year - initialPeriod.Item1.Year + 1; break;
                case DiagramIntervalMode.Months:
                    intervalCount = initialPeriod.Item2.MonthsBeetween(initialPeriod.Item1); break;
                case DiagramIntervalMode.Days:
                    intervalCount = (initialPeriod.Item2 - initialPeriod.Item1).Days + 1; break;
                //case DiagramIntervalMode.WholePeriod:
                default: intervalCount = 1; break;
            }

            _percentForOneInterval = 100 / intervalCount;
        }

        private DateTime PointToDatetime(double point)
        {
            var intervalNumber = (int)(point / _percentForOneInterval);
            switch (_intervalMode)
            {
                case DiagramIntervalMode.Years: return _initialPeriod.Item1.AddYears(intervalNumber);
                case DiagramIntervalMode.Months: return _initialPeriod.Item1.AddMonths(intervalNumber);
                case DiagramIntervalMode.Days: return _initialPeriod.Item1.AddDays(intervalNumber);
                //case DiagramIntervalMode.WholePeriod:
                default:
                    return _initialPeriod.Item1;
            }
        }

        public Tuple<DateTime, DateTime> PointsToPeriod(double fromPoint, double toPoint)
        {
            return new Tuple<DateTime, DateTime>(PointToDatetime(fromPoint), PointToDatetime(toPoint));
        }

        private double DatetimeToPoint(DateTime day)
        {
            int intervalNumber;
            switch (_intervalMode)
            {
                case DiagramIntervalMode.Years:
                    intervalNumber = day.Year - _initialPeriod.Item1.Year + 1;
                    break;
                case DiagramIntervalMode.Months:
                    intervalNumber = day.Year * 12 + day.Month - (_initialPeriod.Item1.Year * 12 +
                                     _initialPeriod.Item1.Month) + 1;
                    break;
                    case DiagramIntervalMode.Days:
                        intervalNumber = (day - _initialPeriod.Item1).Days + 1;
                        break;
                default: intervalNumber = 1; break;
            }

            return intervalNumber * _percentForOneInterval;
        }

        public Tuple<double, double> PeriodToPoints(Tuple<DateTime, DateTime> period)
        {
            return new Tuple<double, double>(DatetimeToPoint(period.Item1), DatetimeToPoint(period.Item2));
        }
    }
}