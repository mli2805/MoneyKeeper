using System;
using System.Collections.Generic;
using FakeItEasy;
using Keeper.DomainModel;
using Keeper.Utils;
using NUnit.Framework;
using FluentAssertions;

namespace Keeper.UnitTests.Utils
{
  [TestFixture]
  public class TestBalanceCalculator
  {
    private IKeeperDb _db;
    private BalanceCalculator _underTest;

    [Test]
    public void ArticleBalancePairs_Should_Evaluate_Traffic_Within_Period()
    {
      _db = A.Fake<IKeeperDb>();
      _underTest = new BalanceCalculator(_db);

      var wantedResult = new List<MoneyPair> {new MoneyPair{Amount = 100, Currency = CurrencyCodes.EUR}};

      var testedCategory  = new Account("Test category");
      _underTest.ArticleBalancePairs(testedCategory, new Period(new DateTime(0), DateTime.Today)).ShouldBeEquivalentTo(wantedResult);
    }
  }
}