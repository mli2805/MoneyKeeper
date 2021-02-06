using System;
using System.Globalization;

namespace KeeperDomain
{
    public static class TxtParser
    {
        public static CurrencyRates CurrencyRateFromString(this string s)
        {
            var rate = new CurrencyRates();
            var substrings = s.Split(';');
            rate.Id = Convert.ToInt32(substrings[0]);
            rate.Date = DateTime.ParseExact(substrings[1].Trim(), "dd.MM.yyyy", CultureInfo.InvariantCulture);
            rate.NbRates = NbRbRatesFromString(substrings[2]);
            rate.CbrRate.Usd = OneRateFromString(substrings[3]);
            rate.MyUsdRate = OneRateFromString(substrings[4]);
            rate.MyEurUsdRate = OneRateFromString(substrings[5]);
            return rate;
        }

        private static NbRbRates NbRbRatesFromString(string s)
        {
            var rate = new NbRbRates();
            var substrings = s.Split('|');
            rate.Usd = OneRateFromString(substrings[0]);
            rate.Euro = OneRateFromString(substrings[1]);
            rate.Rur = OneRateFromString(substrings[2]);
            return rate;
        }

        private static OneRate OneRateFromString(string s)
        {
            var substrings = s.Split('/');
            return new OneRate()
            {
                Value = double.Parse(substrings[0], new CultureInfo("en-US")),
                Unit = int.Parse(substrings[1]),
            };
        }

        public static Account AccountFromString(this string s)
        {
            var account = new Account();
            var substrings = s.Split(';');
            account.Id = Convert.ToInt32(substrings[0]);
            account.Header = substrings[1].Trim();
            account.OwnerId = Convert.ToInt32(substrings[2]);
            account.IsFolder = Convert.ToBoolean(substrings[3]);
            account.IsExpanded = Convert.ToBoolean(substrings[4]);
            account.Comment = substrings[5].Trim();
            return account;
        }

        public static Deposit DepositFromString(this string s)
        {
            var deposit = new Deposit();
            var substrings = s.Split(';');
            deposit.MyAccountId = Convert.ToInt32(substrings[0]);
            deposit.DepositOfferId = Convert.ToInt32(substrings[1]);
            deposit.Serial = substrings[2].Trim();
            deposit.StartDate = DateTime.ParseExact(substrings[3].Trim(), "dd.MM.yyyy", CultureInfo.InvariantCulture);
            deposit.FinishDate = DateTime.ParseExact(substrings[4].Trim(), "dd.MM.yyyy", CultureInfo.InvariantCulture);
            deposit.ShortName = substrings[5].Trim();
            deposit.Comment = substrings[6].Replace("|", "\r\n");
            return deposit;
        }

        public static PayCard CardFromString(this string s)
        {
            var card = new PayCard();
            var substrings = s.Split(';');
            card.MyAccountId = Convert.ToInt32(substrings[0]);
            card.CardNumber = substrings[1].Trim();
            card.CardHolder = substrings[2].Trim();
            card.PaymentSystem = (PaymentSystem)Enum.Parse(typeof(PaymentSystem), substrings[3]);
            card.IsPayPass = Convert.ToBoolean(substrings[4]);
            return card;
        }

        public static Transaction TransactionFromString(this string s)
        {
            var tran = new Transaction();
            var substrings = s.Split(';');
            tran.Id = int.Parse(substrings[0].Trim());
            tran.Timestamp = DateTime.ParseExact(substrings[1].Trim(), "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture);
            tran.Operation = (OperationType)Enum.Parse(typeof(OperationType), substrings[2]);
            tran.PaymentWay = (PaymentWay)Enum.Parse(typeof(PaymentWay), substrings[3]);
            tran.Receipt = int.Parse(substrings[4].Trim());

            tran.MyAccount = int.Parse(substrings[5].Trim());
            tran.MySecondAccount = int.Parse(substrings[6].Trim());

            tran.Amount = Convert.ToDecimal(substrings[7], new CultureInfo("en-US"));
            tran.Currency = (CurrencyCode)Enum.Parse(typeof(CurrencyCode), substrings[8]);
            tran.AmountInReturn = Convert.ToDecimal(substrings[9], new CultureInfo("en-US"));
            tran.CurrencyInReturn = substrings[10].Trim() != ""
                ? (CurrencyCode)Enum.Parse(typeof(CurrencyCode), substrings[10])
                : CurrencyCode.USD;
            // tran.Tags = TagsFromString(substrings[11].Trim());
            tran.Tags = substrings[11].Trim();
            tran.Comment = substrings[12].Trim();

            return tran;
        }

        // private static List<int> TagsFromString(string str)
        // {
        //     var tags = new List<int>();
        //     if (str == "") return tags;
        //
        //     var substrings = str.Split('|');
        //     tags.AddRange(substrings.Select(substring => int.Parse(substring.Trim())));
        //
        //     return tags;
        // }

        public static TagAssociation TagAssociationFromString(this string s)
        {
            var association = new TagAssociation();
            var substrings = s.Split(';');
            association.Id = int.Parse(substrings[0].Trim());
            association.ExternalAccount = int.Parse(substrings[1].Trim());
            association.Tag = int.Parse(substrings[2].Trim());
            association.OperationType = (OperationType)Enum.Parse(typeof(OperationType), substrings[3]);
            association.Destination = (AssociationType)Enum.Parse(typeof(AssociationType), substrings[4]);
            return association;
        }

        public static DepositOffer DepositOfferFromString(this string s)
        {
            var substrings = s.Split(';');
            return new DepositOffer()
            {
                Id = Convert.ToInt32(substrings[0]),
                BankId = Convert.ToInt32(substrings[1]),
                Title = substrings[2].Trim(),
                IsNotRevocable = bool.Parse(substrings[3].Trim()),
                MainCurrency = (CurrencyCode)Enum.Parse(typeof(CurrencyCode), substrings[4]),
                Comment = substrings[5].Trim()
            };
        }

        public static DepositConditions DepoConditionsFromString(this string str)
        {
            var substrings = str.Split(';');
            return new DepositConditions(
                Convert.ToInt32(substrings[0]),
                Convert.ToInt32(substrings[1]),
                DateTime.ParseExact(substrings[2].Trim(), "dd.MM.yyyy", CultureInfo.InvariantCulture))
            {
                Comment = substrings[3].Trim(),
            };
        }

        public static DepositCalculationRules NewDepoCalcRulesFromString(this string str)
        {
            var rules = new DepositCalculationRules();
            var array = str.Split(';');

            rules.Id = int.Parse(array[0].Trim());
            rules.DepositOfferConditionsId = int.Parse(array[1].Trim());
            rules.IsFactDays = bool.Parse(array[2]);
            rules.EveryStartDay = bool.Parse(array[3]);
            rules.EveryFirstDayOfMonth = bool.Parse(array[4]);
            rules.EveryLastDayOfMonth = bool.Parse(array[5]);
            rules.IsCapitalized = bool.Parse(array[6]);
            rules.IsRateFixed = bool.Parse(array[7]);
            rules.HasAdditionalProcent = bool.Parse(array[8]);

            rules.AdditionalProcent = double.Parse(array[9]);
            return rules;
        }

        public static DepositRateLine NewDepoRateLineFromString(this string s)
        {
            var depositRateLine = new DepositRateLine();
            var substrings = s.Split(';');
            depositRateLine.Id = int.Parse(substrings[0].Trim());
            depositRateLine.DepositOfferConditionsId = int.Parse(substrings[1].Trim());
            depositRateLine.DateFrom = DateTime.ParseExact(substrings[2].Trim(), "dd.MM.yyyy", CultureInfo.InvariantCulture);
            depositRateLine.AmountFrom = Convert.ToDecimal(substrings[3], new CultureInfo("en-US"));
            depositRateLine.AmountTo = Convert.ToDecimal(substrings[4], new CultureInfo("en-US"));
            depositRateLine.Rate = Convert.ToDecimal(substrings[5], new CultureInfo("en-US"));
            return depositRateLine;
        }

        public static Car CarFromString(this string str)
        {
            var car = new Car();
            var array = str.Split(';');

            car.Id = int.Parse(array[0].Trim());
            car.CarAccountId = int.Parse(array[1].Trim());
            car.Title = array[2].Trim();
            car.IssueYear = int.Parse(array[3].Trim());
            car.Vin = array[4].Trim();
            car.StateRegNumber = array[5].Trim();

            car.PurchaseDate = DateTime.ParseExact(array[6].Trim(), "dd.MM.yyyy", CultureInfo.InvariantCulture);
            car.PurchaseMileage = int.Parse(array[7].Trim());
            car.SaleDate = DateTime.ParseExact(array[8].Trim(), "dd.MM.yyyy", CultureInfo.InvariantCulture);
            car.SaleMileage = int.Parse(array[9].Trim());

            car.SupposedSalePrice = int.Parse(array[10].Trim());
            car.Comment = array[11].Trim();
            return car;
        }

        public static YearMileage YearMileageFromString(this string str)
        {
            var yearMileage = new YearMileage();
            var array = str.Split(';');

            yearMileage.Id = int.Parse(array[0].Trim());
            yearMileage.CarId = int.Parse(array[1].Trim());
            yearMileage.YearNumber = int.Parse(array[2].Trim());
            yearMileage.Mileage = int.Parse(array[3].Trim());

            return yearMileage;
        }

        public static Fuelling FuellingFromString(this string str)
        {
            var fuelling = new Fuelling();
            var array = str.Split(';');

            fuelling.Id = int.Parse(array[0].Trim());
            fuelling.TransactionId = int.Parse(array[1].Trim());
            fuelling.Volume = double.Parse(array[2].Trim());
            fuelling.FuelType = (FuelType)Enum.Parse(typeof(FuelType), array[3]);
            return fuelling;
        }
    }
}
