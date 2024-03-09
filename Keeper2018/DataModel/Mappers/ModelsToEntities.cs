﻿using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using KeeperDomain;

namespace Keeper2018
{
    public static class ModelsToEntities
    {
        private static readonly IMapper Mapper = new MapperConfiguration(
            cfg => cfg.AddProfile<MappingModelsToEntitiesProfile>()).CreateMapper();
      
        public static Transaction Map(this TransactionModel transactionModel)
        {
            return new Transaction()
            {
                Id = transactionModel.Id,
                Timestamp = transactionModel.Timestamp,
                Receipt = transactionModel.Receipt,
                Operation = transactionModel.Operation,
                PaymentWay = transactionModel.PaymentWay,
                MyAccount = transactionModel.MyAccount.Id,
                MySecondAccount = transactionModel.MySecondAccount?.Id ?? -1,
                Amount = transactionModel.Amount,
                AmountInReturn = transactionModel.AmountInReturn,
                Currency = transactionModel.Currency,
                CurrencyInReturn = transactionModel.CurrencyInReturn,
                Tags = transactionModel.Tags.MapTags(),
                Comment = transactionModel.Comment,
            };
        }

        private static string MapTags(this List<AccountItemModel> tags)
        {
            if (tags == null || tags.Count == 0) return " ";
            string result = "";
            foreach (var t in tags)
            {
                result = result + t.Id + " | ";
            }
            result = result.Substring(0, result.Length - 3);
            return result;
        }

        public static Account Map(this AccountItemModel model)
        {
            return new Account()
            {
                Id = model.Id,
                ParentId = model.Parent?.Id ?? 0,
                Name = model.Name,
                IsFolder = model.IsFolder,
                IsExpanded = model.IsExpanded,
                AssociatedIncomeId = model.AssociatedIncomeId,
                AssociatedExpenseId = model.AssociatedExpenseId,
                AssociatedExternalId = model.AssociatedExternalId,
                ShortName = model.ShortName,
                ButtonName = model.ButtonName,
                Comment = model.Comment,
            };
        }

        // public static BankAccount Map(this BankAccountModel model)
        // {
        //     return new BankAccount()
        //     {
        //         Id = model.Id,
        //         BankId = model.BankId,
        //         DepositOfferId = model.DepositOfferId,
        //         MainCurrency = model.MainCurrency,
        //         AgreementNumber = model.AgreementNumber,
        //         ReplenishDetails = model.ReplenishDetails,
        //         StartDate = model.StartDate,
        //         FinishDate = model.FinishDate,
        //         IsMine = model.IsMine,
        //     };
        // }


        public static DepositOffer Map(this DepositOfferModel depositOfferModel)
        {
            return new DepositOffer()
            {
                Id = depositOfferModel.Id,
                BankId = depositOfferModel.Bank.Id,
                Title = depositOfferModel.Title,
                IsNotRevocable = depositOfferModel.IsNotRevocable,
                RateType = depositOfferModel.RateType,
                IsAddLimited = depositOfferModel.IsAddLimited,
                AddLimitInDays = depositOfferModel.AddLimitInDays,
                MainCurrency = depositOfferModel.MainCurrency,
                DepositTerm = depositOfferModel.DepositTerm.Map(),
                Comment = depositOfferModel.Comment,
            };
        }

        private static Duration Map(this DurationModel durationModel)
        {
            return durationModel.IsPerpetual ? new Duration() : new Duration(durationModel.Value, durationModel.Scale);
        }

       
        public static Fuelling Map(this FuellingModel fuellingModel)
        {
            return new Fuelling()
            {
                Id = fuellingModel.Id,
                TransactionId = fuellingModel.Transaction.Id,
                CarAccountId = fuellingModel.CarAccountId,
                Volume = fuellingModel.Volume,
                FuelType = fuellingModel.FuelType,
            };
        }

        public static CardBalanceMemo Map(this CardBalanceMemoModel cardBalanceMemoModel)
        {
            var entity = Mapper.Map<CardBalanceMemo>(cardBalanceMemoModel);
            entity.AccountId = cardBalanceMemoModel.Account.Id;
            return entity;
        }

        public static TrustAsset Map(this TrustAssetModel asset)
        {
            return new TrustAsset()
            {
                Id = asset.Id,
                TrustAccountId = asset.TrustAccount?.Id ?? 0,
                Ticker = asset.Ticker,
                Title = asset.Title,
                StockMarket = asset.StockMarket,
                AssetType = asset.AssetType,
                Nominal = asset.Nominal,
                //BondCouponPeriodDays = asset.BondCouponPeriodDays,
                BondCouponPeriod = asset.BondCouponPeriod,
                CouponRate = asset.CouponRate,
                PreviousCouponDate = asset.PreviousCouponDate,
                BondExpirationDate = asset.BondExpirationDate,
                Comment = asset.Comment,
            };
        }

        public static TrustTransaction Map(this TrustTranModel transaction)
        {
            return new TrustTransaction()
            {
                Id = transaction.Id,
                InvestOperationType = transaction.InvestOperationType,
                Timestamp = transaction.Timestamp,
                AccountId = transaction.AccountItemModel?.Id ?? 0,
                TrustAccountId = transaction.TrustAccount?.Id ?? 0,
                CurrencyAmount = transaction.CurrencyAmount,
                CouponAmount = transaction.CouponAmount,
                Currency = transaction.Currency,
                AssetAmount = transaction.AssetAmount,
                AssetId = transaction.Asset?.Id ?? 0,
                PurchaseFee = transaction.BuySellFee,
                PurchaseFeeCurrency = transaction.BuySellFeeCurrency,
                FeePaymentOperationId = transaction.FeePaymentOperationId,
                Comment = transaction.Comment,
            };
        }

        public static ButtonCollection Map(this ButtonCollectionModel model)
        {
            return new ButtonCollection()
            {
                Id = model.Id,
                Name = model.Name,
                AccountIds = model.AccountModels.Select(m => m.Id).ToList(),
            };
        }

    }
}