using System;

namespace Keeper2018
{
    public class Period
    {
        public DateTime StartDate { get; set; } = new DateTime(2001,12,31);
        public DateTime FinishMoment { get; set; }

        public bool Includes(DateTime timestamp)
        {
            return timestamp > StartDate && timestamp < FinishMoment;
        }
    }
}