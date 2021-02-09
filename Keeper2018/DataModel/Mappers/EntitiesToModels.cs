using System.Collections.Generic;
using System.Linq;
using KeeperDomain;

namespace Keeper2018
{
    public static class EntitiesToModels
    {
        public static AccountModel Map(this Account account)
        {
            return new AccountModel(account.Header)
            {
                Id = account.Id,
                IsExpanded = account.IsExpanded,
                Deposit = account.Deposit,
            };
        }

        public static DepositOfferModel Map(this DepositOffer depositOffer, List<Account> accountPlaneList)
        {
            return new DepositOfferModel(depositOffer.Id)
            {
                Bank = accountPlaneList.First(a=>a.Id == depositOffer.BankId),
                Title = depositOffer.Title,
                IsNotRevocable = depositOffer.IsNotRevocable,
                MainCurrency = depositOffer.MainCurrency,
                Comment = depositOffer.Comment,
            };
        }
      
        public static TransactionModel Map(this Transaction transaction, Dictionary<int, AccountModel> acMoDict)
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
                // Tags = transaction.Tags.Select(t => acMoDict[t]).ToList(),
                Tags = transaction.Tags.MapTags(acMoDict),
                Comment = transaction.Comment,
            };
        }

        private static List<AccountModel> MapTags(this string tagStr, Dictionary<int, AccountModel> acMoDict)
        {
            var tags = new List<AccountModel>();
            if (tagStr == "" || tagStr == " ") return tags;

            var substrings = tagStr.Split('|');
            tags.AddRange(substrings
                .Select(substring => int.Parse(substring.Trim()))
                .Select(i=>acMoDict[i]));

            return tags;
        }

        public static CarVm Map(this Car car)
        {
            return new CarVm()
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

        public static YearMileageVm Map(this YearMileage yearMileage)
        {
            return new YearMileageVm()
            {
                Id = yearMileage.Id,
                CarId = yearMileage.CarId,
                YearNumber = yearMileage.YearNumber,
                Mileage = yearMileage.Mileage
            };
        }
    }
}