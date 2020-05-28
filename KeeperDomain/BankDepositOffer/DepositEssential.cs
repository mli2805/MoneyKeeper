using System;
using System.Collections.Generic;
using System.Linq;

namespace KeeperDomain
{
    [Serializable]
    public class DepositEssential
    {
        public int Id { get; set; } //PK
        // ID is necessary because Deposit can have more than one Essential 
        // and txt file should have ID to separate RateLines from different Essentials
        public int DepositOfferId { get; set; } 
        public DepositCalculationRules CalculationRules { get; set; } = new DepositCalculationRules();
        public List<DepositRateLine> RateLines { get; set; } = new List<DepositRateLine>();

        public DepositEssential DeepCopy()
        {
            return new DepositEssential
            {
                CalculationRules = CalculationRules.ShallowCopy(),
                RateLines = new List<DepositRateLine>(RateLines.Select(l => l.ShallowCopy()))
            };
        }
        public string Comment { get; set; }

        public string PartDump()
        {
            return DepositOfferId + " ; " + Id + " ; " + CalculationRules.Dump() + " ; " + Comment;
        }
    }
}