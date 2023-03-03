using System.Collections.Generic;

namespace Keeper2018
{
    public class CarReportTable
    {
        public string Russian;
        public string English;
        public List<CarReportTableRow> Table;

        public CarReportTable(string russian, string english, List<CarReportTableRow> table)
        {
            Russian = russian;
            English = english;
            Table = table;
        }
    }
}