using System;
using System.Diagnostics.CodeAnalysis;

namespace Keeper2018
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class NbRbSiteRate
    {
        public int Cur_ID { get; set; }
        public DateTime Date { get; set; }
        public string Cur_Abbreviation { get; set; }
        public int Cur_Scale { get; set; }
        public string Cur_Name { get; set; }
        public double Cur_OfficialRate { get; set; }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class NbRbSiteShortRate
    {
        public int Cur_ID { get; set; }
        public DateTime Date { get; set; }
        public double Cur_OfficialRate { get; set; }
    }
}