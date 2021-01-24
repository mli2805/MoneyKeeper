using System;
using System.Collections.Generic;
using System.Linq;

namespace KeeperDomain
{
    [Serializable]
    public class DepositConditions
    {
        public int Id { get; set; } //PK
        public int DepositOfferId { get; set; } 
        public DateTime DateFrom { get; set; }
        public DepositCalculationRules CalculationRules { get; set; }
        public List<DepositRateLine> RateLines { get; set; } = new List<DepositRateLine>();


        public DepositConditions(int id, int depositOfferId, DateTime dateFrom)
        {
            Id = id;
            DepositOfferId = depositOfferId;
            DateFrom = dateFrom;
            CalculationRules = new DepositCalculationRules() { DepositOfferConditionsId = id };
        }

        public DepositConditions DeepCopy()
        {
            return new DepositConditions(Id, DepositOfferId, DateFrom)
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