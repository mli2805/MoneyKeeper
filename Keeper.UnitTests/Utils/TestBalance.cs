using System;
using FakeItEasy;
using FluentAssertions;
using Keeper.DomainModel;
using Keeper.Utils;
using NUnit.Framework;

namespace Keeper.UnitTests.Utils
{
  [TestFixture]
  public class TestBalance
  {
    private BalanceCalculator _underTest;
    private RateExtractor _rate;
    private readonly static DateTime Monday = new DateTime(2000, 1, 1);

    [SetUp]
    public void SetUp()
    {
      _rate = A.Fake<RateExtractor>();
      _underTest = new BalanceCalculator(A.Fake<KeeperDb>());
		
    }
    [Test]
    public void BalancePairsToUsd_Should_Convert_Amounts_To_Usd_And_Return_Totals()
    {
      A.CallTo(() => _rate.GetRateThisDayOrBefore(CurrencyCodes.BYR, Monday)).Returns(14);
      A.CallTo(() => _rate.GetRateThisDayOrBefore(CurrencyCodes.EUR, Monday)).Returns(22);
      _underTest.BalancePairsToUsd(new[]
                                     {
                                       new BalanceCalculator.BalancePair {Amount = 7, Currency = CurrencyCodes.BYR},
                                       new BalanceCalculator.BalancePair {Amount = 11, Currency = CurrencyCodes.EUR},
                                     }, Monday).Should().Be(1);
    }
    [Test]
    public void BalancePairsToUsd_When_USD_Should_Not_Call_GetRateThisDayOrBefore()
    {
      _underTest.BalancePairsToUsd(new[]
                                     {
                                       new BalanceCalculator.BalancePair {Amount = 7, Currency = CurrencyCodes.USD},
                                     }, Monday).Should().Be(7);
      A.CallTo(() => _rate.GetRateThisDayOrBefore(A<CurrencyCodes>.Ignored, A<DateTime>.Ignored))
        .MustNotHaveHappened();
    }
  }
}