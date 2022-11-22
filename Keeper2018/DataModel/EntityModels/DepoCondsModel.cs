using System;
using System.Collections.Generic;
using KeeperDomain;

namespace Keeper2018
{
    [Serializable]
    public class DepoCondsModel
    {
        public int Id { get; set; }
        public int DepositOfferId { get; set; }
        public DateTime DateFrom { get; set; }


        public bool IsFactDays { get; set; } // true 28-31/365 false 30/360
        public bool EveryStartDay { get; set; } 
        public bool EveryFirstDayOfMonth { get; set; }
        public bool EveryLastDayOfMonth { get; set; }
        public bool IsCapitalized { get; set; }

        public bool IsRateFixed { get; set; }
        public RateType RateType { get; set; }

        public bool HasAdditionalPercent { get; set; }
        public double AdditionalPercent { get; set; }

        public List<DepositRateLine> RateLines { get; set; } = new List<DepositRateLine>();

        public string Comment { get; set; }
    }
}