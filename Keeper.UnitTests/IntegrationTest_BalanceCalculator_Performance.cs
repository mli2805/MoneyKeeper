using System;
using System.Diagnostics;
using System.IO;
using Keeper.DomainModel;
using Keeper.DomainModel.DbTypes;
using Keeper.DomainModel.Enumes;
using Keeper.DomainModel.Transactions;
using Keeper.DomainModel.WorkTypes;
using Keeper.Utils.AccountEditing;
using Keeper.Utils.BalanceEvaluating;
using Keeper.Utils.BalanceEvaluating.Ilya;
using Keeper.Utils.DbInputOutput.TxtTasks;

using NUnit.Framework;

namespace Keeper.IntegrationTests
{
	[TestFixture]
	public sealed class IntegrationTest_BalanceCalculator_Performance
	{
		AccountBalanceCalculator mUnderTest;
		readonly Random mRnd = new Random();
		KeeperDb mKeeperDb;
		static readonly Period sPeriod = new Period(new DateTime(2000, 1, 1), new DateTime(3000, 1, 1));

		[SetUp]
		public void SetUp()
		{
			mKeeperDb = new DbFromTxtLoader(new DbClassesInstanceParser(),
        new AccountTreeStraightener()).LoadDbFromTxt(Path.GetFullPath("TestDb\\forBalanceEvaluationTest")).Db;
		}

		void RunOld(int iterations, Stopwatch sw)
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
						Currency = CurrencyCodes.EUR,
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
			Console.WriteLine("One call to GetAccountBalancePairs takes: {0} ms", sw.Elapsed.TotalMilliseconds / (count * iterations));
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
			Console.WriteLine("One call to GetAccountBalancePairs takes: {0} ms", sw.Elapsed.TotalMilliseconds / (count * iterations));
		}
	}
}