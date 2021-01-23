using System;
using System.Collections.Generic;
using System.Linq;

namespace KeeperDomain
{
    [Serializable]
    public class DepositConditions
    {
        public int Id { get; set; } //PK
        // ID is necessary because Deposit can have more than one Essential 
        // and txt file should have ID to separate RateLines from different ConditionsMap
        public int DepositOfferId { get; set; } 
        public DateTime DateFrom { get; set; }
        public DepositCalculationRules CalculationRules { get; set; } = new DepositCalculationRules();
        public List<DepositRateLine> RateLines { get; set; } = new List<DepositRateLine>();

        public DepositConditions DeepCopy()
        {
            return new DepositConditions
            {
                CalculationRules = CalculationRules.ShallowCopy(),
                RateLines = new List<DepositRateLine>(RateLines.Select(l => l.ShallowCopy()))
            };
        }
        public string Comment { get; set; }

        // public string PartDump()
        // {
        //     return DepositOfferId + " ; " + Id + " ; " + CalculationRules.Dump() + " ; " + Comment;
        // }
        //
        public string Dump()
        {
            return Id + " ; " + DepositOfferId + " ; " + $"{DateFrom:dd/MM/yyyy}" + " ; " + Comment;
        }
    }
}