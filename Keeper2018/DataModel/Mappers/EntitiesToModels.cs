using System.Collections.Generic;
using System.Linq;
using KeeperDomain;

namespace Keeper2018
{
    public static class EntitiesToModels
    {
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

        public static BankAccountModel Map(this BankAccount bankAccount)
        {
            return new BankAccountModel()
            {
                Id = bankAccount.Id,
                BankId = bankAccount.BankId,
                DepositOfferId = bankAccount.DepositOfferId,
                MainCurrency = bankAccount.MainCurrency,
                AgreementNumber = bankAccount.AgreementNumber,
                ReplenishDetails = bankAccount.ReplenishDetails,
                StartDate = bankAccount.StartDate,
                FinishDate = bankAccount.FinishDate,
                IsMine = bankAccount.IsMine,
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

        public static DepoCondsModel Map(this DepoNewConds depoConds)
        {
            return new DepoCondsModel()
            {
                Id = depoConds.Id,
                DepositOfferId = depoConds.DepositOfferId,
                DateFrom = depoConds.DateFrom,

                RateFormula = depoConds.RateFormula,
                IsFactDays = depoConds.IsFactDays,
                EveryStartDay = depoConds.EveryStartDay,
                EveryFirstDayOfMonth = depoConds.EveryFirstDayOfMonth,
                EveryLastDayOfMonth = depoConds.EveryLastDayOfMonth,
                EveryNDays = depoConds.EveryNDays,
                NDays = depoConds.NDays,
                IsCapitalized = depoConds.IsCapitalized,
                HasAdditionalPercent = depoConds.HasAdditionalProcent,
                AdditionalPercent = depoConds.AdditionalProcent,

                Comment = depoConds.Comment,
            };
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

        public static CarModel Map(this Car car)
        {
            return new CarModel()
            {
                Id = car.Id,
                CarAccountId = car.CarAccountId,
                Title = car.Title,
                IssueYear = car.IssueYear,
                Vin = car.Vin,
                StateRegNumber = car.StateRegNumber,
                PurchaseDate = car.PurchaseDate,
                PurchaseMileage = car.PurchaseMileage,
                SaleDate = car.SaleDate,
                SaleMileage = car.SaleMileage,
                SupposedSalePrice = car.SupposedSalePrice,
            };
        }

        public static YearMileageModel Map(this YearMileage yearMileage)
        {
            return new YearMileageModel()
            {
                Id = yearMileage.Id,
                CarId = yearMileage.CarId,
                YearNumber = yearMileage.YearNumber,
                Year = yearMileage.Year,
                Odometer = yearMileage.Odometer,
            };
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