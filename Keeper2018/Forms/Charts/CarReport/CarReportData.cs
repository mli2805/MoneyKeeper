using System;
using System.Collections.Generic;

namespace Keeper2018
{
    public class CarReportData
    {
        public List<PdfReportTable> Tags = new List<PdfReportTable>();
        public DateTime StartDate;
        public DateTime FinishDate;
    }
}