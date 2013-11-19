using System;
using System.Collections.ObjectModel;
using FakeItEasy;
using Keeper.DomainModel;
using Keeper.Utils;
using NUnit.Framework;
using FluentAssertions;

namespace Keeper.UnitTests.Utils
{
	[TestFixture]
	public class TestRate
	{
		private IKeeperDb _keeperDb;
		private readonly static DateTime Monday = new DateTime(2000, 1, 1);
		private readonly static DateTime Tuesday = new DateTime(2000, 1, 2);
		private readonly static DateTime Wednesday = new DateTime(2000, 1, 3);
		private RateExtractor _underTest;

		[SetUp]
		public void SetUp()
		{
			_keeperDb = A.Fake<IKeeperDb>();
			_underTest = new RateExtractor(_keeperDb);
			A.CallTo(() => _keeperDb.CurrencyRates).Returns(new ObservableCollection<CurrencyRate>
				{
					new CurrencyRate
						{
							BankDay = Monday,
							Currency = CurrencyCodes.BYR,
							Rate = 1,
						},
					new CurrencyRate
						{
							BankDay = Wednesday,
							Currency = CurrencyCodes.BYR,
							Rate = 2,
						},
				});
		}
		[Test]
		public void GetRate_When_Invalid_Date_Should_Be_0()
		{
			_underTest.GetRate(CurrencyCodes.BYR, Tuesday).Should().Be(0);
		}
		[Test]
		public void GetRate_Should_Return_Rate_For_The_Day()
		{
			_underTest.GetRate(CurrencyCodes.BYR, Monday).Should().Be(1);
			_underTest.GetRate(CurrencyCodes.BYR, Wednesday).Should().Be(2);
		}
	}

	[TestFixture]
	public class TestBalance
	{
		private BalanceCalculator _underTest;
		private IRate _rate;
		private readonly static DateTime Monday = new DateTime(2000, 1, 1);

		[SetUp]
		public void SetUp()
		{
			_rate = A.Fake<IRate>();
			_underTest = new BalanceCalculator(A.Fake<IKeeperDb>(), _rate);
		
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