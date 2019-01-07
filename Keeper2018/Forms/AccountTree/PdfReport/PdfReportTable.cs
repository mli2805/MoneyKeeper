using System.Collections.Generic;

namespace Keeper2018
{
    public class PdfReportTable
    {
        public string Russian;
        public string English;
        public List<PdfReportTableRow> Table;

        public PdfReportTable(string russian, string english, List<PdfReportTableRow> table)
        {
            Russian = russian;
            English = english;
            Table = table;
        }
    }
}