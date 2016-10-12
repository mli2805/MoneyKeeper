using System;
using System.Collections.ObjectModel;
using Keeper.Utils.Rates;

using NUnit.Framework;

using FluentAssertions;
using Keeper.DomainModel.DbTypes;
using Keeper.DomainModel.Enumes;

namespace Keeper.UnitTests.Utils.Rates
{
	[TestFixture]
	public class TestRateExtractor
	{
		private KeeperDb _keeperDb;
		private readonly static DateTime FirstDay = new DateTime(2002, 1, 1);
		private readonly static DateTime SecondDay = new DateTime(2002, 1, 2);
		private readonly static DateTime ThirdDay = new DateTime(2002, 1, 3);
		private RateExtractor _underTest;

		[SetUp]
		public void SetUp()
		{
			_keeperDb = new KeeperDb();
			_underTest = new RateExtractor(_keeperDb);
			_keeperDb.CurrencyRates = new ObservableCollection<CurrencyRate>
				{
					new CurrencyRate
						{
							BankDay = FirstDay,
							Currency = CurrencyCodes.BYR,
							Rate = 1,
						},
					new CurrencyRate
						{
							BankDay = ThirdDay,
							Currency = CurrencyCodes.BYR,
							Rate = 2,
						},
					new CurrencyRate
						{
							BankDay = ThirdDay,
							Currency = CurrencyCodes.EUR,
							Rate = 7,
						},
				};
		}
		[Test]
		public void GetRate_When_No_Rate_For_Date_Should_Return_0()
		{
			_underTest.GetRate(CurrencyCodes.BYR, SecondDay).Should().Be(0);
		}
		[Test]
		public void GetRate_Should_Return_Rate_For_The_Day()
		{
			_underTest.GetRate(CurrencyCodes.BYR, FirstDay).Should().Be(1);
			_underTest.GetRate(CurrencyCodes.BYR, ThirdDay).Should().Be(2);
		}
	}
}