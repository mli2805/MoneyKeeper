using System;
using System.Globalization;
using KeeperDomain.Exchange;

namespace KeeperDomain
{
    public static class TxtParser
    {
        public static OfficialRates CurrencyRateFromString(this string s)
        {
            var rate = new OfficialRates();
            var substrings = s.Split(';');
            rate.Id = int.Parse(substrings[0]);
            rate.Date = DateTime.ParseExact(substrings[1].Trim(), "dd.MM.yyyy", CultureInfo.InvariantCulture);
            rate.NbRates = NbRbRatesFromString(substrings[2]);
            rate.CbrRate.Usd = OneRateFromString(substrings[3]);
            return rate;
        }

        public static ExchangeRates ExchangeRatesFromString(this string s)
        {
            var rate = new ExchangeRates();
            var substrings = s.Split(';');
            rate.Id = int.Parse(substrings[0]);
            rate.Date = DateTime.ParseExact(substrings[1].Trim(), "dd.MM.yyyy", CultureInfo.InvariantCulture);
            rate.UsdToByn = double.Parse(substrings[2], new CultureInfo("en-US"));
            rate.BynToUsd = double.Parse(substrings[3], new CultureInfo("en-US"));
            rate.EurToByn = double.Parse(substrings[4], new CultureInfo("en-US"));
            rate.BynToEur = double.Parse(substrings[5], new CultureInfo("en-US"));
            rate.RubToByn = double.Parse(substrings[6], new CultureInfo("en-US"));
            rate.BynToRub = double.Parse(substrings[7], new CultureInfo("en-US"));
            rate.EurToUsd = double.Parse(substrings[8], new CultureInfo("en-US"));
            rate.UsdToEur = double.Parse(substrings[9], new CultureInfo("en-US"));
            rate.UsdToRub = double.Parse(substrings[10], new CultureInfo("en-US"));
            rate.RubToUsd = double.Parse(substrings[11], new CultureInfo("en-US"));
            rate.EurToRub = double.Parse(substrings[12], new CultureInfo("en-US"));
            rate.RubToEur = double.Parse(substrings[13], new CultureInfo("en-US"));
            return rate;
        }

        public static MinfinMetalRate MetalRateFromString(this string s)
        {
            var rate = new MinfinMetalRate();
            var substrings = s.Split(';');
            rate.Id = int.Parse(substrings[0]);
            rate.Date = DateTime.ParseExact(substrings[1].Trim(), "dd.MM.yyyy", CultureInfo.InvariantCulture);
            rate.Metal = (Metal)Enum.Parse(typeof(Metal), substrings[2]);
            rate.Proba = int.Parse(substrings[3]);
            rate.Price = double.Parse(substrings[4], new CultureInfo("en-US"));
            return rate;
        }

        public static RefinancingRate RefinancingRateFromString(this string s)
        {
            var rate = new RefinancingRate();
            var substrings = s.Split(';');
            rate.Id = int.Parse(substrings[0]);
            rate.Date = DateTime.ParseExact(substrings[1].Trim(), "dd.MM.yyyy", CultureInfo.InvariantCulture);
            rate.Value = double.Parse(substrings[2], new CultureInfo("en-US"));
            return rate;
        }

        public static InvestmentAsset InvestmentAssetFromString(this string s)
        {
            var ticker = new InvestmentAsset();
            var substrings = s.Split(';');
            ticker.Id = int.Parse(substrings[0]);
            ticker.TrustAccountId = int.Parse(substrings[1]);
            ticker.Ticker = substrings[2].Trim();
            ticker.Title = substrings[3].Trim();
            ticker.StockMarket = (StockMarket)Enum.Parse(typeof(StockMarket), substrings[4]);
            ticker.AssetType = (AssetType)Enum.Parse(typeof(AssetType), substrings[5]);
            ticker.Nominal = decimal.Parse(substrings[6], new CultureInfo("en-US"));
            ticker.BondCouponPeriod = CalendarPeriod.Parse(substrings[7]);
            ticker.CouponRate = double.Parse(substrings[8], new CultureInfo("en-US"));
            ticker.PreviousCouponDate = DateTime.ParseExact(substrings[9].Trim(), "dd.MM.yyyy", CultureInfo.InvariantCulture);
            ticker.BondExpirationDate = DateTime.ParseExact(substrings[10].Trim(), "dd.MM.yyyy", CultureInfo.InvariantCulture);
            ticker.Comment = substrings[11].Trim();

            return ticker;
        }

        public static AssetRate AssetRateFromString(this string s)
        {
            var rate = new AssetRate();
            var substrings = s.Split(';');
            rate.Id = int.Parse(substrings[0]);
            rate.TickerId = int.Parse(substrings[1]);
            rate.Date = DateTime.ParseExact(substrings[2].Trim(), "dd.MM.yyyy", CultureInfo.InvariantCulture);
            rate.Unit = int.Parse(substrings[3]);
            rate.Value = decimal.Parse(substrings[4], new CultureInfo("en-US"));
            rate.Currency = (CurrencyCode)Enum.Parse(typeof(CurrencyCode), substrings[5]);
            return rate;
        }

        public static TrustAccount TrustAccountFromString(this string s)
        {
            var trustAccount = new TrustAccount();
            var substrings = s.Split(';');
            trustAccount.Id = int.Parse(substrings[0]);
            trustAccount.Title = substrings[1].Trim();
            trustAccount.StockMarket = (StockMarket)Enum.Parse(typeof(StockMarket), substrings[2]);
            trustAccount.Number = substrings[3].Trim();
            trustAccount.Currency = (CurrencyCode)Enum.Parse(typeof(CurrencyCode), substrings[4]);
            trustAccount.AccountId = int.Parse(substrings[5].Trim());
            trustAccount.Comment = substrings[6].Trim();
            return trustAccount;
        }

        public static InvestmentTransaction InvestmentTransactionFromString(this string s)
        {
            var trans = new InvestmentTransaction();
            var substrings = s.Split(';');
            trans.Id = int.Parse(substrings[0]);
            trans.InvestOperationType = (InvestOperationType)Enum.Parse(typeof(InvestOperationType), substrings[1]);
            trans.Timestamp = DateTime.ParseExact(substrings[2].Trim(), "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture);
            trans.AccountId = int.Parse(substrings[3]);
            trans.TrustAccountId = int.Parse(substrings[4]);

            trans.CurrencyAmount = decimal.Parse(substrings[5], new CultureInfo("en-US"));
            trans.CouponAmount = decimal.Parse(substrings[6], new CultureInfo("en-US"));
            trans.Currency = (CurrencyCode)Enum.Parse(typeof(CurrencyCode), substrings[7]);

            trans.AssetAmount = int.Parse(substrings[8]);
            trans.AssetId = int.Parse(substrings[9]);

            trans.PurchaseFee = decimal.Parse(substrings[10], new CultureInfo("en-US"));
            trans.PurchaseFeeCurrency = (CurrencyCode)Enum.Parse(typeof(CurrencyCode), substrings[11]);
            trans.FeePaymentOperationId = int.Parse(substrings[12]);

            trans.Comment = substrings[13].Trim();
            return trans;
        }

        private static NbRbRates NbRbRatesFromString(string s)
        {
            var rate = new NbRbRates();
            var substrings = s.Split('|');
            rate.Usd = OneRateFromString(substrings[0]);
            rate.Euro = OneRateFromString(substrings[1]);
            rate.Rur = OneRateFromString(substrings[2]);
            rate.Cny = OneRateFromString(substrings[3]);
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
            account.Id = int.Parse(substrings[0]);
            account.Name = substrings[1].Trim();
            account.ParentId = int.Parse(substrings[2]);
            account.IsFolder = Convert.ToBoolean(substrings[3]);
            account.IsExpanded = Convert.ToBoolean(substrings[4]);
            account.AssociatedIncomeId = int.Parse(substrings[5]);
            account.AssociatedExpenseId = int.Parse(substrings[6]);
            account.AssociatedExternalId = int.Parse(substrings[7]);
            account.ShortName = substrings[8].Trim();
            account.ButtonName = substrings[9].Trim();
            account.Comment = substrings[10].Trim();
            return account;
        }

        public static BankAccount BankAccountFromString(this string s)
        {
            var bankAccount = new BankAccount();
            var substrings = s.Split(';');
            bankAccount.Id = int.Parse(substrings[0]);

            bankAccount.BankId = int.Parse(substrings[1]);
            bankAccount.DepositOfferId = int.Parse(substrings[2]);
            bankAccount.MainCurrency = (CurrencyCode)Enum.Parse(typeof(CurrencyCode), substrings[3]);

            bankAccount.AgreementNumber = substrings[4].Trim();
            bankAccount.ReplenishDetails = substrings[5].Trim();

            bankAccount.StartDate = DateTime.ParseExact(substrings[6].Trim(), "dd.MM.yyyy", CultureInfo.InvariantCulture);
            bankAccount.FinishDate = DateTime.ParseExact(substrings[7].Trim(), "dd.MM.yyyy", CultureInfo.InvariantCulture);
            
            bankAccount.IsMine = Convert.ToBoolean(substrings[8]);

            return bankAccount;
        }

        public static Deposit DepositFromString(this string s)
        {
            var deposit = new Deposit();
            var substrings = s.Split(';');
            deposit.Id = int.Parse(substrings[0].Trim());
            deposit.IsAdditionsBanned = bool.Parse(substrings[1].Trim());
            return deposit;
        }

        public static PayCard CardFromString(this string s)
        {
            var card = new PayCard();
            var substrings = s.Split(';');
            card.Id = int.Parse(substrings[0]);
         
            card.CardNumber = substrings[1].Trim();
            card.CardHolder = substrings[2].Trim();
            card.PaymentSystem = (PaymentSystem)Enum.Parse(typeof(PaymentSystem), substrings[3]);
            card.IsVirtual = Convert.ToBoolean(substrings[4]);
            card.IsPayPass = Convert.ToBoolean(substrings[5]);

            return card;
        }

        // парсер в самом классе
        // public static CardBalanceMemo CardBalanceMemoFromString(this string s)
        // {
        //     var memo = new CardBalanceMemo();
        //     var substrings = s.Split(';');
        //     memo.Id = int.Parse(substrings[0]);
        //     memo.AccountId = int.Parse(substrings[1]);
        //     memo.BalanceThreshold = Convert.ToDecimal(substrings[2], new CultureInfo("en-US"));
        //     return memo;
        // }

        public static ButtonCollection ButtonCollectionFromString(this string s)
        {
            var buttonCollection = new ButtonCollection();
            var substrings = s.Split(';');
            buttonCollection.Id = int.Parse(substrings[0]);
            buttonCollection.Name = substrings[1];
            for (int i = 2; i < substrings.Length; i++)
            {
                buttonCollection.AccountIds.Add(int.Parse(substrings[i]));
            }
            return buttonCollection;
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
            tran.Tags = substrings[11].Trim();
            tran.Comment = substrings[12].Trim();

            return tran;
        }

        public static DepositOffer DepositOfferFromString(this string s)
        {
            var substrings = s.Split(';');
            return new DepositOffer()
            {
                Id = int.Parse(substrings[0]),
                BankId = int.Parse(substrings[1]),
                Title = substrings[2].Trim(),
                IsNotRevocable = bool.Parse(substrings[3].Trim()),
                RateType = (RateType)Enum.Parse(typeof(RateType), substrings[4]),
                IsAddLimited = bool.Parse(substrings[5].Trim()),
                AddLimitInDays = int.Parse(substrings[6]),
                MainCurrency = (CurrencyCode)Enum.Parse(typeof(CurrencyCode), substrings[7]),
                DepositTerm = DurationFromStrings(substrings[8], substrings[9], substrings[10]),
                Comment = substrings[11].Trim()
            };
        }

        private static Duration DurationFromStrings(string a, string b, string c)
        {
            return Convert.ToBoolean(a) ? new Duration() : new Duration(int.Parse(b), (Durations)Enum.Parse(typeof(Durations), c));
        }

        public static DepoNewConds DepoNewCondsFromString(this string str)
        {
            var substrings = str.Split(';');
            var result = new DepoNewConds()
            {
                Id = int.Parse(substrings[0]),
                DepositOfferId = int.Parse(substrings[1]),
                DateFrom = DateTime.ParseExact(substrings[2].Trim(), "dd.MM.yyyy", CultureInfo.InvariantCulture),

                RateFormula = substrings[3].Trim(),
                IsFactDays = bool.Parse(substrings[4]),
                EveryStartDay = bool.Parse(substrings[5]),
                EveryFirstDayOfMonth = bool.Parse(substrings[6]),
                EveryLastDayOfMonth = bool.Parse(substrings[7]),

                EveryNDays = bool.Parse(substrings[8]),
                NDays = int.Parse(substrings[9]),

                IsCapitalized = bool.Parse(substrings[10]),
                HasAdditionalProcent = bool.Parse(substrings[11]),
                AdditionalProcent = double.Parse(substrings[12]),

                Comment = substrings[13].Trim()
            };

            return result;
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
            yearMileage.Year = int.Parse(array[3].Trim());
            yearMileage.Odometer = int.Parse(array[4].Trim());

            return yearMileage;
        }

        public static Fuelling FuellingFromString(this string str)
        {
            var fuelling = new Fuelling();
            var array = str.Split(';');

            fuelling.Id = int.Parse(array[0].Trim());
            fuelling.TransactionId = int.Parse(array[1].Trim());
            fuelling.CarAccountId = int.Parse(array[2].Trim());
            fuelling.Volume = double.Parse(array[3].Trim());
            fuelling.FuelType = (FuelType)Enum.Parse(typeof(FuelType), array[4]);
            return fuelling;
        }
    }
}
