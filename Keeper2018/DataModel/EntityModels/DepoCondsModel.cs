using System;
using System.Collections.Generic;
using System.Linq;
using KeeperDomain;

namespace Keeper2018
{
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

        public bool HasAdditionalProcent { get; set; }
        public double AdditionalProcent { get; set; }

        public List<DepositRateLine> RateLines { get; set; } = new List<DepositRateLine>();


        public string Comment { get; set; }
      
        public DepoCondsModel DeepCopy()
        {
            var deepCopy = new DepoCondsModel()
            {
                Id = Id,
                DepositOfferId = DepositOfferId,
                DateFrom = DateFrom,
            
                IsFactDays = IsFactDays,
                EveryStartDay = EveryStartDay,
                EveryFirstDayOfMonth = EveryFirstDayOfMonth,
                EveryLastDayOfMonth = EveryLastDayOfMonth,
                IsCapitalized = IsCapitalized,
                IsRateFixed = IsRateFixed,
                HasAdditionalProcent = HasAdditionalProcent,
                AdditionalProcent = AdditionalProcent,
            
                Comment = Comment,
            };
            deepCopy.RateLines = new List<DepositRateLine>(RateLines.Select(l => (DepositRateLine) l.Clone()));
            return deepCopy;
        }
    }
}