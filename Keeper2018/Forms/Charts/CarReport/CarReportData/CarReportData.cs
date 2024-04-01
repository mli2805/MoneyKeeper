using System;
using System.Collections.Generic;

namespace Keeper2018
{
    public class CarReportData
    {
        public List<CarReportTable> Categories = new List<CarReportTable>();
        public DateTime StartDate;
        public DateTime FinishDate;
    }
}