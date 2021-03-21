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


        public static Car Map(this CarVm carVm)
        {
            return new Car()
            {
                Id = carVm.Id,
                CarAccountId = carVm.CarAccountId,
                Title = carVm.Title,
                IssueYear = carVm.IssueYear,
                Vin = carVm.Vin,
                StateRegNumber = carVm.StateRegNumber,
                PurchaseDate = carVm.PurchaseDate,
                PurchaseMileage = carVm.PurchaseMileage,
                SaleDate = carVm.SaleDate,
                SaleMileage = carVm.SaleMileage,
                SupposedSalePrice = carVm.SupposedSalePrice,
            };
        }

        public static YearMileage Map(this YearMileageVm yearMileageVm)
        {
            return new YearMileage()
            {
                Id = yearMileageVm.Id,
                CarId = yearMileageVm.CarId,
                YearNumber = yearMileageVm.YearNumber,
                Mileage = yearMileageVm.Mileage
            };
        }

        public static Fuelling Map(this FuellingVm fuellingVm)
        {
            return new Fuelling()
            {
                Id = fuellingVm.CarAccountId,
                TransactionId = fuellingVm.Transaction.Id,
                Volume = fuellingVm.Volume,
                FuelType = fuellingVm.FuelType,
            };
        }
    }
}