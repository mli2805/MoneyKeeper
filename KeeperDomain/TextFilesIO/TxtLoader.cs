using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
                    ExchangeRates = ReadFileLines("ExchangeRates.txt", TxtParser.ExchangeRatesFromString),
                    OfficialRates = ReadFileLines("OfficialRates.txt", TxtParser.CurrencyRateFromString),
                    MetalRates = ReadFileLines("MetalRates.txt", TxtParser.MetalRateFromString),
                    RefinancingRates = ReadFileLines("RefinancingRates.txt", TxtParser.RefinancingRateFromString),
                   
                    InvestmentAssets = ReadFileLines("InvestmentAssets.txt", TxtParser.InvestmentAssetFromString),
                    AssetRates = ReadFileLines("AssetRates.txt", TxtParser.AssetRateFromString),
                    TrustAccounts = ReadFileLines("TrustAccounts.txt", TxtParser.TrustAccountFromString),
                    InvestmentTransactions = ReadFileLines("InvestmentTransactions.txt", TxtParser.InvestmentTransactionFromString),
                   
                    AccountPlaneList = ReadFileLines("Accounts.txt", TxtParser.AccountFromString),
                    BankAccounts = ReadFileLines("BankAccounts.txt", TxtParser.BankAccountFromString),
                    Deposits = ReadFileLines("Deposits.txt", TxtParser.DepositFromString),
                    PayCards = ReadFileLines("PayCards.txt", TxtParser.CardFromString),
                    ButtonCollections = ReadFileLines("ButtonCollections.txt", TxtParser.ButtonCollectionFromString),
                   
                    DepositRateLines = ReadFileLines("depoRateLines.txt", TxtParser.NewDepoRateLineFromString),
                    DepoNewConds = ReadFileLines("depoConds.txt", TxtParser.DepoNewCondsFromString),
                    DepositOffers = ReadFileLines("depoOffers.txt", TxtParser.DepositOfferFromString),
                   
                    Transactions = ReadFileLines("Transactions.txt", TxtParser.TransactionFromString),
                    Fuellings = ReadFileLines("Fuellings.txt", TxtParser.FuellingFromString),
                    Cars = ReadFileLines("Cars.txt", TxtParser.CarFromString),
                    YearMileages = ReadFileLines("CarYearMileages.txt", TxtParser.YearMileageFromString),

                    CardBalanceMemos = ReadFileLines<CardBalanceMemo>("MemosCardBalance.txt"),
                };

                return new LibResult(true, keeperBin);
            }
            catch (Exception e)
            {
                return new LibResult(e);
            }
        }

        private static List<T> ReadFileLines<T>(string filename, Func<string, T> func)
        {
            return File.ReadAllLines(Path.Combine(_backupFolder, filename)).Select(func).ToList();
        }

        // теперь если парсинг перенести в каждый класс, то можно вызывать этот ReadFileLines
        // см пример с CardBalanceMemo
        private static List<T> ReadFileLines<T>(string filename) where T : IParsable<T>, new()
        {
            return File.ReadAllLines(Path.Combine(_backupFolder, filename)).Select(l => new T().FromString(l)).ToList();
        }
    }
}