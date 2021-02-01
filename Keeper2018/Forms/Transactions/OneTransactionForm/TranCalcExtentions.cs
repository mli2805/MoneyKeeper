﻿using System;
using System.Collections.Generic;
using KeeperDomain;

namespace Keeper2018
{
    static class TranCalcExtentions
    {
        public static decimal AmountForAccount(this Transaction tran, KeeperDataModel dataModel, AccountModel account, CurrencyCode? currency, DateTime upToDateTime)
        {
            return tran.Timestamp <= upToDateTime ? AmountForAccount(tran, dataModel, account, currency) : 0;
        }

        private static decimal AmountForAccount(this Transaction tran, KeeperDataModel dataModel, AccountModel account, CurrencyCode? currency)
        {
            var isAccount = dataModel.AcMoDict[tran.MyAccount].Is(account);
            bool isSecondAccount;
            switch (tran.Operation)
            {
                case OperationType.Доход:
                    return isAccount && tran.Currency == currency ? tran.Amount : 0;
                case OperationType.Расход:
                    return isAccount && tran.Currency == currency ? -tran.Amount : 0;
                case OperationType.Перенос:
                    if (isAccount && tran.Currency == currency) return -tran.Amount;
                    isSecondAccount = tran.MySecondAccount != -1 && dataModel.AcMoDict[tran.MySecondAccount].Is(account);
                    return isSecondAccount && tran.Currency == currency ? tran.Amount : 0;
                case OperationType.Обмен:
                    if (isAccount && tran.Currency == currency) return -tran.Amount;
                    isSecondAccount = tran.MySecondAccount != -1 && dataModel.AcMoDict[tran.MySecondAccount].Is(account);
                    if (isSecondAccount && tran.CurrencyInReturn == currency) return tran.AmountInReturn;
                    return 0;
                default:
                    return 0;
            }
        }


        public static decimal AmountForAccount(this TransactionModel tran, AccountModel account, CurrencyCode? currency)
        {
            var isAccount = tran.MyAccount.Is(account);
            var isSecondAccount = tran.MySecondAccount != null && tran.MySecondAccount.Is(account);
            switch (tran.Operation)
            {
                case OperationType.Доход:
                    return isAccount && tran.Currency == currency ? tran.Amount : 0;
                case OperationType.Расход:
                    return isAccount && tran.Currency == currency ? -tran.Amount : 0;
                case OperationType.Перенос:
                    if (isAccount && tran.Currency == currency) return -tran.Amount;
                    return isSecondAccount && tran.Currency == currency ? tran.Amount : 0;
                case OperationType.Обмен:
                    if (isAccount && tran.Currency == currency) return -tran.Amount;
                    if (isSecondAccount && tran.CurrencyInReturn == currency) return tran.AmountInReturn;
                    return 0;
                default:
                    return 0;
            }
        }

        public static Balance BalanceForTag(this Transaction tran, KeeperDataModel dataModel, int tagId)
        {
            if (!CollectionContainsTag(dataModel, tran.Tags, tagId)) return null;
            switch (tran.Operation)
            {
                case OperationType.Доход:
                    return new Balance(tran.Currency, tran.Amount);
                case OperationType.Расход:
                    return new Balance(tran.Currency, -tran.Amount);
                case OperationType.Перенос:
                    return new Balance(tran.Currency, 0);
                case OperationType.Обмен:
                    var balance = new Balance(tran.Currency, -tran.Amount);
                    // ReSharper disable once PossibleInvalidOperationException
                    balance.Add((CurrencyCode)tran.CurrencyInReturn, tran.AmountInReturn);
                    return balance;
                default:
                    return null;
            }
        }

        private static bool CollectionContainsTag(KeeperDataModel dataModel, List<int> collection, int tagId)
        {
            foreach (var id in collection)
            {
                var tag = dataModel.AcMoDict[id];
                if (tag.Is(tagId)) return true;
            }
            return false;
        }

    }
}
