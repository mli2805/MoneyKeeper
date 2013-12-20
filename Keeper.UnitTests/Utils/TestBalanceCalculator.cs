using System;
using System.Collections.Generic;
using System.IO;
using Keeper.DomainModel;
using Keeper.Properties;
using Keeper.Utils;
using Keeper.Utils.DbInputOutput;
using NUnit.Framework;
using FluentAssertions;

namespace Keeper.UnitTests.Utils
{
  [TestFixture]
  public class TestBalanceCalculator
  {
    private readonly KeeperDb _db;
    private BalanceCalculator _underTest;

    public TestBalanceCalculator()
    {
      _db = new KeeperDb();
      new DbFromTxtLoader().LoadDbFromTxt(ref _db, Path.GetFullPath(Settings.Default.TestDbPath));
      _underTest = new BalanceCalculator(_db);
    }

    [Test]
    public void ArticleBalancePairs_Without_Appropriate_Transactions_Should_Return_Empty_List()
    {
      var testedCategory = new Account("Зарплата");
      _underTest.ArticleBalancePairs(testedCategory, new Period(new DateTime(2013, 11, 15), new DateTime(2013, 12, 1))).ShouldBeEquivalentTo(new List<MoneyPair>());
    }

    [Test]
    public void ArticleBalancePairs_Should_Evaluate_Traffic_Within_Period()
    {
      var expectation = new List<MoneyPair> 
      {
        new MoneyPair{Amount = 700, Currency = CurrencyCodes.USD},
        new MoneyPair{Amount = 6855187, Currency = CurrencyCodes.BYR}
      };

      var testedCategory = new Account("Зарплата");
      _underTest.ArticleBalancePairs(testedCategory, new Period(new DateTime(2013, 11, 1), new DateTime(2013,12,1))).ShouldBeEquivalentTo(expectation);
    }

    [Test]
    public void ArticleBalancePairs_Should_Evaluate_Returns()
    {
      var expectation = new List<MoneyPair> 
      {
        new MoneyPair{Amount = -100, Currency = CurrencyCodes.USD},
        new MoneyPair{Amount = 373700, Currency = CurrencyCodes.BYR}
      };

      var testedCategory = new Account("Одежда");
      _underTest.ArticleBalancePairs(testedCategory, new Period(new DateTime(2013, 11, 1), new DateTime(2013, 12, 1))).ShouldBeEquivalentTo(expectation);
    }

    [Test]
    public void ArticleBalancePairs_Should_Evaluate_Transactions_Made_In_Last_Day_Of_Period()
    {
      var expectation = new List<MoneyPair> 
      {
        new MoneyPair{Amount = -100, Currency = CurrencyCodes.USD},
        new MoneyPair{Amount = 373700, Currency = CurrencyCodes.BYR}
      };

      var testedCategory = new Account("Одежда");
      _underTest.ArticleBalancePairs(testedCategory, new Period(new DateTime(2013, 11, 1), new DateTime(2013, 11, 19))).ShouldBeEquivalentTo(expectation);
    }

  }
}