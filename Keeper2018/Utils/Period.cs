using System;

namespace Keeper2018
{
    public class Period
    {
        public DateTime StartDate { get; set; }
        public DateTime FinishMoment { get; set; }

        public Period()
        {
            
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