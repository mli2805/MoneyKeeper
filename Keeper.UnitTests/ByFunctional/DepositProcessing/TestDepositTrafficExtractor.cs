using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using Keeper.DomainModel;
using Keeper.DomainModel.Deposit;
using Keeper.Utils.DbInputOutput.CompositeTasks;
using Keeper.Utils.DbInputOutput.TxtTasks;
using Keeper.Utils.Rates;
using NUnit.Framework;
using FluentAssertions;
using Keeper.Utils.AccountEditing;
using Keeper.Utils.DepositProcessing;

namespace Keeper.UnitTests.ByFunctional.DepositProcessing
{
    [TestFixture]
    public sealed class TestDepositTrafficExtractor
    {
        DbLoadResult _loadResult;
        DepositTrafficExtractor _underTest;

        [SetUp]
        public void SetUp()
        {
            _loadResult = new DbFromTxtLoader(new DbClassesInstanceParser(), new AccountTreeStraightener()).
                                               LoadDbFromTxt(Path.GetFullPath(@"TestDb\forDepositCalculationTest"));

            if (_loadResult.Code != 0) MessageBox.Show("Database wasn't loaded properly!");
            _underTest = new DepositTrafficExtractor(_loadResult.Db);

        }

        [Test]
        public void EvaluateTraffic_Should_Evaluate()
        {
            var ats = new AccountTreeStraightener();
            var accounts = ats.Flatten(_loadResult.Db.Accounts);

            var account = accounts.First(a => a.Name == "ВТБ Скарбонка2 8/08/2014 - 7/09/2015 33-31%");

            var depositExtractor = new DepositTrafficExtractor(_loadResult.Db);
            depositExtractor.ExtractTraffic(account).CalculationData.Traffic.Count.Should().Be(10);
        }
    }
}
