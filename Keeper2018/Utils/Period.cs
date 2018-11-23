using System;

namespace Keeper2018
{
    public class Period
    {
        public DateTime StartDate { get; set; }
        public DateTime FinishMoment { get; set; }

        public Period()
        {
            StartDate = new DateTime(2001,12,31);
        }

        public Period(DateTime startDate, DateTime finishMoment)
        {
            StartDate = startDate;
            FinishMoment = finishMoment;
        }

        public bool Includes(DateTime timestamp)
        {
            return timestamp > StartDate && timestamp < FinishMoment;
        }
    }

    
}