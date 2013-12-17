using System;
using System.Collections.Generic;
using System.Composition;
using FakeItEasy;
using Keeper.DomainModel;
using Keeper.Utils;
using Keeper.Utils.DbInputOutput;
using NUnit.Framework;
using FluentAssertions;

namespace Keeper.UnitTests.Utils
{
  [TestFixture]
  public class TestBalanceCalculator
  {
    private IKeeperDb _db;
    private BalanceCalculator _underTest;

    public TestBalanceCalculator()
    {
      _db = new DbSerializer().DecryptAndDeserialize("BaseForUnitTests.dbx");
    }

    [Test]
    public void ArticleBalancePairs_Should_Evaluate_Traffic_Within_Period()
    {
      _underTest = new BalanceCalculator(_db);

      var wantedResult = new List<MoneyPair> 
      {
        new MoneyPair{Amount = 700, Currency = CurrencyCodes.USD},
        new MoneyPair{Amount = 6855187, Currency = CurrencyCodes.BYR}
      };

      var testedCategory = new Account("Зарплата");
      _underTest.ArticleBalancePairs(testedCategory, new Period(new DateTime(2013, 11, 1), new DateTime(2013,12,1))).ShouldBeEquivalentTo(wantedResult);
    }
  }
}