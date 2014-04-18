using System;
using System.Diagnostics;
using System.IO;

using Keeper.DomainModel;
using Keeper.Utils.Accounts;
using Keeper.Utils.Balances;
using Keeper.Utils.DbInputOutput.TxtTasks;

using NUnit.Framework;

namespace Keeper.IntegrationTests
{
	[TestFixture]
	public sealed class IntegrationTest_BalanceCalculator_Performance
	{
		BalanceCalculator mUnderTest;
		readonly Random mRnd = new Random();
		KeeperDb mKeeperDb;
		static readonly Period sPeriod = new Period(new DateTime(2000, 1, 1), new DateTime(3000, 1, 1));

		[SetUp]
		public void SetUp()
		{
			mKeeperDb = new DbFromTxtLoader(new DbClassesInstanceParser()).LoadDbFromTxt(Path.GetFullPath("TestDb")).Db;
		}

		void RunOld(int iterations, Stopwatch sw)
		{
			var targetAccount = mKeeperDb.Accounts[mRnd.Next(mKeeperDb.Accounts.Count)];
			FillDb(targetAccount);

			sw.Start();
			for (int i = 0; i < iterations; i++)
			{
				mUnderTest = new BalanceCalculator(mKeeperDb);
				foreach (var _ in mUnderTest.AccountBalancePairs(targetAccount, sPeriod)) { }
			}
			sw.Stop();
		}

		void RunNew(int iterations, Stopwatch sw)
		{
			var targetAccount = mKeeperDb.Accounts[mRnd.Next(mKeeperDb.Accounts.Count)];
			FillDb(targetAccount);
			sw.Start();
			for (int i = 0; i < iterations; i++)
			{
				foreach (var _ in mKeeperDb.Transactions.Balance(targetAccount, sPeriod)) { }
			}
		}

		void FillDb(Account targetAccount)
		{
			for (int i = 0; i < 10000; i++)
				mKeeperDb.Transactions.Add(new Transaction
					{
						Debet = GetRandomAccount(mKeeperDb, targetAccount),
						Credit = GetRandomAccount(mKeeperDb, targetAccount),
						Amount = 53,
						Amount2 = mRnd.Next(10) == 0 ? 948 : 0,
						Currency = CurrencyCodes.EUR,
						Currency2 = CurrencyCodes.BYR,
						Timestamp = mRnd.Next(5) == 0 ? new DateTime(1000, 1, 1) : new DateTime(2500, 1, 1),
					});
		}

		Account GetRandomAccount(KeeperDb keeperDb, Account targetAccount)
		{
			switch (mRnd.Next(2))
			{
				case 0:
					return keeperDb.Accounts[mRnd.Next(keeperDb.Accounts.Count)];
				default:
					return targetAccount;
			}
		}

		[Test]
		public void MeasureTime()
		{
			var sw = new Stopwatch();
			const int count = 10;
			const int iterations = 10;
			for (int i = 0; i < count; i++)
			{
				RunOld(iterations, sw);
			}
			sw.Stop();
			Console.WriteLine("One call to AccountBalancePairs takes: {0} ms", sw.Elapsed.TotalMilliseconds / (count * iterations));
		}	
		[Test]
		public void MeasureNewTime()
		{
			var sw = new Stopwatch();
			const int count = 10;
			const int iterations = 10;
			for (int i = 0; i < count; i++)
			{
				RunNew(iterations, sw);
			}
			sw.Stop();
			Console.WriteLine("One call to AccountBalancePairs takes: {0} ms", sw.Elapsed.TotalMilliseconds / (count * iterations));
		}
	}
}