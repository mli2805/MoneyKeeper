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
            dataModel.FuellingVms = new List<FuellingModel>();
            foreach (var fuelling in fuellings)
            {
                var tr = dataModel.Transactions[fuelling.TransactionId];

                var fuellingModel = new FuellingModel()
                {
                    Id = fuelling.Id,
                    Timestamp = tr.Timestamp,
                    Transaction = tr,
                    CarAccountId = tr.Tags.Select(t => t.Id).Contains(718) ? 716 : 711,
                    Amount = tr.Amount,
                    Currency = tr.Currency,
                    Volume = fuelling.Volume,
                    FuelType = fuelling.FuelType,
                    Comment = tr.Comment,

                };

                var oneLitrePrice = Math.Abs(fuelling.Volume) < 0.01 ? 0 : tr.Amount / (decimal)fuelling.Volume;
                switch (tr.Currency)
                {
                    case CurrencyCode.BYR:
                    case CurrencyCode.BYN:
                    {
                        fuellingModel.OneLitrePrice = oneLitrePrice;
                        fuellingModel.OneLitreInUsd = dataModel.AmountInUsd(tr.Timestamp, tr.Currency, oneLitrePrice);
                        break;
                    }
                    case CurrencyCode.EUR:
                    {
                        var rate = dataModel.GetRate(tr.Timestamp, tr.Currency);
                        fuellingModel.OneLitrePrice = oneLitrePrice * (decimal)rate.Value;
                        var rateUsd = dataModel.GetRate(tr.Timestamp, CurrencyCode.EUR, true);
                        fuellingModel.OneLitreInUsd = oneLitrePrice * (decimal)rateUsd.Value;
                        break;
                    }
                    case CurrencyCode.USD:
                    {
                        var rate = dataModel.GetRate(tr.Timestamp, tr.Timestamp < new DateTime(2016, 7, 1) ? CurrencyCode.BYR : CurrencyCode.BYN);
                        fuellingModel.OneLitrePrice = oneLitrePrice * (decimal)rate.Value;
                        fuellingModel.OneLitreInUsd = oneLitrePrice;
                        break;
                    }
                }

                dataModel.FuellingVms.Add(fuellingModel);
            }
        }
    }
}