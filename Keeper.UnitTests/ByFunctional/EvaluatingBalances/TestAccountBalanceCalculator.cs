using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using Keeper.ByFunctional.EditingAccounts;
using Keeper.ByFunctional.EvaluatingBalances;
using Keeper.DomainModel;
using Keeper.Utils.DbInputOutput.CompositeTasks;
using Keeper.Utils.DbInputOutput.TxtTasks;

using NUnit.Framework;
using FluentAssertions;


namespace Keeper.UnitTests.Utils.Balances
{
	[TestFixture]
	public sealed class TestAccountBalanceCalculator
	{
		DbLoadResult _loadResult;
        AccountBalanceCalculator _underTest;

        [SetUp]
		public void SetUp()
		{
            _loadResult = new DbFromTxtLoader(new DbClassesInstanceParser(), 
            new AccountTreeStraightener()).LoadDbFromTxt(Path.GetFullPath(@"TestDb\forBalanceEvaluationTest"));

            if (_loadResult.Code != 0) MessageBox.Show("Database wasn't loaded properly!");

            _underTest = new AccountBalanceCalculator(_loadResult.Db);
		}

		[Test]
        public void GetAccountBalancePairsWithTimeChecking_Without_Appropriate_Transactions_Should_Return_Empty_List()
		{
			    var testedAccount = new Account("Тумбочка");
                _underTest.GetAccountBalancePairsWithTimeChecking(testedAccount,
                new Period(new DateTime(2014, 10, 30), new DateTime(2014, 11, 3))).ShouldBeEquivalentTo(new List<MoneyPair>());
		}

		[Test]
        public void GetAccountBalancePairsWithTimeChecking_Should_Use_Time_Sections_Of_Period_Boundaries()
		{
			var expectation = new List<MoneyPair> 
			{
				new MoneyPair{Amount = 270, Currency = CurrencyCodes.USD},
				new MoneyPair{Amount = 150, Currency = CurrencyCodes.EUR},
				new MoneyPair{Amount = 204500, Currency = CurrencyCodes.BYR}
			};

            var testedAccount = new Account("Мой кошелек");
            _underTest.GetAccountBalancePairsWithTimeChecking(testedAccount,
			new Period(new DateTime(2014, 10, 30, 9, 5, 0), new DateTime(2014, 11, 3, 0, 0, 0))).ShouldBeEquivalentTo(expectation);
		}

		[Test]
        public void GetAccountBalancePairsFromMidnightToMidnight_Should_Expand_Period_Boundaries_To_Midnights()
		{
			var expectation = new List<MoneyPair> 
			{
				new MoneyPair{Amount = 270, Currency = CurrencyCodes.USD},
				new MoneyPair{Amount = 150, Currency = CurrencyCodes.EUR},
				new MoneyPair{Amount = 6650, Currency = CurrencyCodes.BYR}
			};

            var testedAccount = new Account("Мой кошелек");
            _underTest.GetAccountBalancePairsFromMidnightToMidnight(testedAccount,
            new Period(new DateTime(2014, 10, 30, 9, 5, 0), new DateTime(2014, 11, 3, 0, 0, 0))).ShouldBeEquivalentTo(expectation);
        }

	}
}