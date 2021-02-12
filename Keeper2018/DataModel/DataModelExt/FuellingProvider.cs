using System;
using System.Collections.Generic;
using System.Linq;
using KeeperDomain;

namespace Keeper2018
{
    public static class FuellingProvider
    {
        public static void FuellingJoinTransaction(this KeeperDataModel dataModel, List<Fuelling> fuellings)
        {
            dataModel.FuellingVms = new List<FuellingVm>();
            foreach (var fuelling in fuellings)
            {
                var tr = dataModel.Transactions[fuelling.TransactionId];
                var oneLitrePrice = Math.Abs(fuelling.Volume) < 0.01 ? 0 : tr.Amount / (decimal)fuelling.Volume;
                dataModel.FuellingVms.Add(new FuellingVm()
                {
                    Timestamp = tr.Timestamp,
                    Transaction = tr,
                    CarAccountId = tr.Tags.Select(t=>t.Id).Contains(718) ? 716 : 711,
                    Amount = tr.Amount,
                    Currency = tr.Currency,
                    Volume = fuelling.Volume,
                    FuelType = fuelling.FuelType,
                    Comment = tr.Comment,

                    OneLitrePrice = oneLitrePrice,
                    OneLitreInUsd = dataModel.AmountInUsd(tr.Timestamp, tr.Currency, oneLitrePrice),
                });
            }
        }
    }
}