using System;
using Keeper.DomainModel.DbTypes;
using Keeper.DomainModel.Enumes;

namespace Keeper.DomainModel.WorkTypes
{
    public class TranForAnalysis
    {
        public DateTime Timestamp { get; set; }
        public decimal Amount { get; set; }
        public CurrencyCodes Currency { get; set; }
        public decimal AmountInUsd { get; set; }
        public Account Category { get; set; }
        public string Comment { get; set; }
        public bool IsDepositTran { get; set; }
        public string DepoName { get; set; }
    }
}