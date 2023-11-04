using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using KeeperDomain;

namespace Keeper2018
{
    public static class EntitiesToModels
    {
        private static readonly IMapper Mapper = new MapperConfiguration(
            cfg => cfg.AddProfile<MappingEntitiesToModelsProfile>()).CreateMapper();

        public static AccountItemModel Map(this Account account)
        {
            return new AccountItemModel(account.Id, account.Name, null)
            {
                Id = account.Id,
                IsFolder = account.IsFolder,
                IsExpanded = account.IsExpanded,
                AssociatedIncomeId = account.AssociatedIncomeId,
                AssociatedExpenseId = account.AssociatedExpenseId,
                AssociatedExternalId = account.AssociatedExternalId,
                ShortName = account.ShortName,
                ButtonName = account.ButtonName,
                Comment = account.Comment,
            };
        }

        public static DepositOfferModel Map(this DepositOffer depositOffer, Dictionary<int, AccountItemModel> acMoDict)
        {
            return new DepositOfferModel
            {
                Id = depositOffer.Id,
                Bank = acMoDict[depositOffer.BankId],
                Title = depositOffer.Title,
                IsNotRevocable = depositOffer.IsNotRevocable,
                RateType = depositOffer.RateType,
                IsAddLimited = depositOffer.IsAddLimited,
                AddLimitInDays = depositOffer.AddLimitInDays,
                MainCurrency = depositOffer.MainCurrency,
                DepositTerm = depositOffer.DepositTerm.Map(),
                Comment = depositOffer.Comment,
            };
        }

        private static DurationModel Map(this Duration duration)
        {
            return duration.IsPerpetual ? new DurationModel() : new DurationModel(duration.Value, duration.Scale);
        }

        public static TransactionModel Map(this Transaction transaction, Dictionary<int, AccountItemModel> acMoDict)
        {
            return new TransactionModel()
            {
                Id = transaction.Id,
                Timestamp = transaction.Timestamp,
                Receipt = transaction.Receipt,
                Operation = transaction.Operation,
                PaymentWay = transaction.PaymentWay,
                MyAccount = acMoDict[transaction.MyAccount],
                MySecondAccount = transaction.MySecondAccount == -1 ? null : acMoDict[transaction.MySecondAccount],
                Amount = transaction.Amount,
                AmountInReturn = transaction.AmountInReturn,
                Currency = transaction.Currency,
                CurrencyInReturn = transaction.CurrencyInReturn,
                Tags = transaction.Tags.MapTags(acMoDict),
                Comment = transaction.Comment,
            };
        }

        private static List<AccountItemModel> MapTags(this string tagStr, Dictionary<int, AccountItemModel> acMoDict)
        {
            var tags = new List<AccountItemModel>();
            if (tagStr == "" || tagStr == " ") return tags;

            var substrings = tagStr.Split('|');
            tags.AddRange(substrings
                .Select(substring => int.Parse(substring.Trim()))
                .Select(i => acMoDict[i]));

            return tags;
        }

        public static CardBalanceMemoModel Map(this CardBalanceMemo entity, AccountItemModel account)
        {
            var model = Mapper.Map<CardBalanceMemoModel>(entity);
            model.Account = account;
            return model;
        }

        public static InvestmentAssetModel Map(this InvestmentAsset asset, KeeperDataModel dataModel)
        {
            return new InvestmentAssetModel()
            {
                Id = asset.Id,
                TrustAccount = asset.Id == 0 ? null : dataModel.TrustAccounts.FirstOrDefault(t => t.Id == asset.TrustAccountId),
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

        public static InvestTranModel Map(this InvestmentTransaction transaction, KeeperDataModel dataModel)
        {
            return new InvestTranModel()
            {
                Id = transaction.Id,
                InvestOperationType = transaction.InvestOperationType,
                Timestamp = transaction.Timestamp,
                AccountItemModel = transaction.AccountId != 0 ? dataModel.AcMoDict[transaction.AccountId] : null,
                TrustAccount = dataModel.TrustAccounts.FirstOrDefault(t => t.Id == transaction.TrustAccountId),
                CurrencyAmount = transaction.CurrencyAmount,
                CouponAmount = transaction.CouponAmount,
                Currency = transaction.Currency,
                AssetAmount = transaction.AssetAmount,
                Asset = dataModel.InvestmentAssets.FirstOrDefault(a => a.Id == transaction.AssetId),
                BuySellFee = transaction.PurchaseFee,
                BuySellFeeCurrency = transaction.PurchaseFeeCurrency == 0 ? CurrencyCode.BYN : transaction.PurchaseFeeCurrency,
                FeePaymentOperationId = transaction.FeePaymentOperationId,
                Comment = transaction.Comment,
            };
        }

        public static ButtonCollectionModel Map(this ButtonCollection buttonCollection, Dictionary<int, AccountItemModel> acMoDict)
        {
            return new ButtonCollectionModel()
            {
                Id = buttonCollection.Id,
                Name = buttonCollection.Name,
                AccountModels = buttonCollection.AccountIds.Select(i => acMoDict[i]).ToList(),
            };
        }
    }
}