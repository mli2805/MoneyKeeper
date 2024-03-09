using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KeeperDomain.Exchange;

namespace KeeperDomain
{
    public static class TxtLoader
    {
        private static string _backupFolder;

        public static LibResult LoadAllFromNewTxt(string backupFolder)
        {
            _backupFolder = backupFolder;

            try
            {
                var keeperBin = new KeeperBin
                {
                    ExchangeRates = ReadFileLines<ExchangeRates>(),
                    OfficialRates = ReadFileLines<OfficialRates>(),
                    MetalRates = ReadFileLines<MetalRate>(),
                    RefinancingRates = ReadFileLines<RefinancingRate>(),

                    TrustAssets = ReadFileLines<TrustAsset>(),
                    TrustAssetRates = ReadFileLines<TrustAssetRate>(),
                    TrustAccounts = ReadFileLines<TrustAccount>(),
                    TrustTransactions = ReadFileLines<TrustTransaction>(),

                    AccountPlaneList = ReadFileLines<Account>(),
                    BankAccounts = ReadFileLines<BankAccount>(),
                    Deposits = ReadFileLines<Deposit>(),
                    PayCards = ReadFileLines<PayCard>(),
                    ButtonCollections = ReadFileLines<ButtonCollection>(),

                    DepositRateLines = ReadFileLines<DepositRateLine>(),
                    DepositConditions = ReadFileLines<DepositConditions>(),
                    DepositOffers = ReadFileLines<DepositOffer>(),

                    Transactions = ReadFileLines<Transaction>(),
                    Fuellings = ReadFileLines<Fuelling>(),
                    Cars = ReadFileLines<Car>(),
                    CarYearMileages = ReadFileLines<CarYearMileage>(),

                    CardBalanceMemos = ReadFileLines<CardBalanceMemo>("MemosCardBalance.txt"),
                    SalaryChanges = ReadFileLines<SalaryChange>(),
                    LargeExpenseThresholds = ReadFileLines<LargeExpenseThreshold>(),
                };

                return new LibResult(true, keeperBin);
            }
            catch (Exception e)
            {
                return new LibResult(e);
            }
        }

        private static List<T> ReadFileLines<T>(string filename = "") where T : IParsable<T>, new()
        {
            if (filename == "")
                filename = typeof(T).Name + "s.txt";
            return File.ReadAllLines(Path.Combine(_backupFolder, filename)).Select(l => new T().FromString(l)).ToList();
        }
    }
}