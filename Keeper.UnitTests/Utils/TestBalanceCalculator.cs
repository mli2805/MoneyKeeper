using System;
using System.Collections.Generic;
using System.Composition;
using System.IO;
using FakeItEasy;
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
    private KeeperDb _db;
    private BalanceCalculator _underTest;

    public TestBalanceCalculator()
    {
//      _db = new DbSerializer().DecryptAndDeserialize(Path.GetFullPath(@"..\..\..\TestDb\Keeper.dbx"));
      _db = new KeeperDb();
      new DbFromTxtLoader().LoadDbFromTxt(ref _db, Path.GetFullPath(Settings.Default.TestDbPath));
    }

    [Test]
    public void ArticleBalancePairs_Should_Evaluate_Traffic_Within_Period()
    {
      _underTest = new BalanceCalculator(_db);

      var expectation = new List<MoneyPair> 
      {
        new MoneyPair{Amount = 700, Currency = CurrencyCodes.USD},
        new MoneyPair{Amount = 6855187, Currency = CurrencyCodes.BYR}
      };

      var testedCategory = new Account("Зарплата");
      _underTest.ArticleBalancePairs(testedCategory, new Period(new DateTime(2013, 11, 1), new DateTime(2013,12,1))).ShouldBeEquivalentTo(expectation);
    }
  }
}