﻿using System;

namespace Keeper2018
{
    [Serializable]
    public class OfficialRates
    {
        public DateTime Date { get; set; }
        public NbRbRates NbRates { get; set; } = new NbRbRates();
        public CbrRate CbrRate { get; set; } = new CbrRate();

    }
}