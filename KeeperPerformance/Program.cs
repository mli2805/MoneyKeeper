﻿using System;
using System.Diagnostics;
using System.IO;
using Keeper.DomainModel;
using Keeper.DomainModel.DbTypes;
using Keeper.DomainModel.Enumes;
using Keeper.DomainModel.Trans;
using Keeper.DomainModel.WorkTypes;
using Keeper.Utils.AccountEditing;
using Keeper.Utils.BalanceEvaluating;
using Keeper.Utils.DbInputOutput.TxtTasks;

namespace Perf
{
    class Program
    {
        static void Main(string[] args)
        {
            mKeeperDb = new DbFromTxtLoader(new DbClassesInstanceParser(), new AccountTreeStraightener()).LoadDbFromTxt(Path.GetFullPath("TestDb")).Db;
            MeasureTime();
            Console.ReadKey();
        }

        static AccountBalanceCalculator mUnderTest;
        static readonly Random mRnd = new Random();
        static readonly Period sPeriod = new Period(new DateTime(2000, 1, 1), new DateTime(3000, 1, 1));
        static KeeperDb mKeeperDb;

        static void RunOld(int iterations, Stopwatch sw)
        {
            var targetAccount = mKeeperDb.Accounts[mRnd.Next(mKeeperDb.Accounts.Count)];
            FillDb(targetAccount);

            sw.Start();
            for (int i = 0; i < iterations; i++)
            {
                mUnderTest = new AccountBalanceCalculator(mKeeperDb);
                foreach (var _ in mUnderTest.GetAccountBalancePairs(targetAccount, sPeriod)) { }
            }
            sw.Stop();
        }

        static void RunNew(int iterations, Stopwatch sw)
        {
            var targetAccount = mKeeperDb.Accounts[mRnd.Next(mKeeperDb.Accounts.Count)];
            FillDb(targetAccount);
            sw.Start();
            for (int i = 0; i < iterations; i++)
            {
                foreach (var _ in mKeeperDb.Transactions.Balance(targetAccount, sPeriod)) { }
            }
        }

        static void FillDb(Account targetAccount)
        {
            for (int i = 0; i < 10000; i++)
                mKeeperDb.Transactions.Add(new Transaction
                {
                    Debet = GetRandomAccount(mKeeperDb, targetAccount),
                    Credit = GetRandomAccount(mKeeperDb, targetAccount),
                    Amount = 53,
                    Currency = CurrencyCodes.EUR,
                    Timestamp = mRnd.Next(5) == 0 ? new DateTime(1000, 1, 1) : new DateTime(2500, 1, 1),
                });
        }

        static Account GetRandomAccount(KeeperDb keeperDb, Account targetAccount)
        {
            switch (mRnd.Next(2))
            {
                case 0:
                    return keeperDb.Accounts[mRnd.Next(keeperDb.Accounts.Count)];
                default:
                    return targetAccount;
            }
        }

        public static void MeasureTime()
        {
            var sw = new Stopwatch();
            const int count = 10;
            const int iterations = 10;
            for (int i = 0; i < count; i++)
            {
                RunOld(iterations, sw);
            }
            sw.Stop();
            Console.WriteLine("One call to GetAccountBalancePairs takes: {0} ms", sw.Elapsed.TotalMilliseconds / (count * iterations));
        }
        public static void MeasureNewTime()
        {
            var sw = new Stopwatch();
            const int count = 10;
            const int iterations = 10;
            for (int i = 0; i < count; i++)
            {
                RunNew(iterations, sw);
            }
            sw.Stop();
            Console.WriteLine("One call to GetAccountBalancePairs takes: {0} ms", sw.Elapsed.TotalMilliseconds / (count * iterations));
        }
    }
}
