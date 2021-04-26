using System.Collections.Generic;
using KeeperDomain;

namespace Keeper2018
{
    public static class ModelsToEntities
    {
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

        private static string MapTags(this List<AccountModel> tags)
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

        public static Account Map(this AccountModel model)
        {
            return new Account()
            {
                Id = model.Id,
                OwnerId = model.Owner?.Id ?? 0,
                Header = (string)model.Header,
                IsExpanded = model.IsExpanded,
                AssociatedIncomeId = model.AssociatedIncomeId,
                AssociatiedExpenseId = model.AssociatiedExpenseId,
                AssociatiedExternalId = model.AssociatiedExternalId,
            };
        }

        public static DepositOffer Map(this DepositOfferModel depositOfferModel)
        {
            return new DepositOffer()
            {
                Id = depositOfferModel.Id,
                BankId = depositOfferModel.Bank.Id,
                Title = depositOfferModel.Title,
                IsNotRevocable = depositOfferModel.IsNotRevocable,
                MainCurrency = depositOfferModel.MainCurrency,
                Comment = depositOfferModel.Comment,
            };
        }

        public static DepoNewConds Map(this  DepoCondsModel depoCondsModel)
        {
            return new DepoNewConds()
            {
                Id = depoCondsModel.Id,
                DepositOfferId = depoCondsModel.DepositOfferId,
                DateFrom = depoCondsModel.DateFrom,
            
                IsFactDays = depoCondsModel.IsFactDays,
                EveryStartDay = depoCondsModel.EveryStartDay,
                EveryFirstDayOfMonth = depoCondsModel.EveryFirstDayOfMonth,
                EveryLastDayOfMonth = depoCondsModel.EveryLastDayOfMonth,
                IsCapitalized = depoCondsModel.IsCapitalized,
                IsRateFixed = depoCondsModel.IsRateFixed,
                HasAdditionalProcent = depoCondsModel.HasAdditionalProcent,
                AdditionalProcent = depoCondsModel.AdditionalProcent,
            
                Comment = depoCondsModel.Comment,
            };
        }


        public static Car Map(this CarModel carModel)
        {
            return new Car()
            {
                Id = carModel.Id,
                CarAccountId = carModel.CarAccountId,
                Title = carModel.Title,
                IssueYear = carModel.IssueYear,
                Vin = carModel.Vin,
                StateRegNumber = carModel.StateRegNumber,
                PurchaseDate = carModel.PurchaseDate,
                PurchaseMileage = carModel.PurchaseMileage,
                SaleDate = carModel.SaleDate,
                SaleMileage = carModel.SaleMileage,
                SupposedSalePrice = carModel.SupposedSalePrice,
            };
        }

        public static YearMileage Map(this YearMileageModel yearMileageModel)
        {
            return new YearMileage()
            {
                Id = yearMileageModel.Id,
                CarId = yearMileageModel.CarId,
                YearNumber = yearMileageModel.YearNumber,
                Year = yearMileageModel.Year,
                Odometer = yearMileageModel.Odometer
            };
        }

        public static Fuelling Map(this FuellingModel fuellingModel)
        {
            return new Fuelling()
            {
                Id = fuellingModel.CarAccountId,
                TransactionId = fuellingModel.Transaction.Id,
                Volume = fuellingModel.Volume,
                FuelType = fuellingModel.FuelType,
            };
        }
    }
}