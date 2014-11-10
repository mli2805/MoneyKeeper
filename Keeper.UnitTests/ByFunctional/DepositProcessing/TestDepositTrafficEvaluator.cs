using System.IO;
using System.Windows;
using Keeper.ByFunctional.AccountEditing;
using Keeper.ByFunctional.DepositProcessing;
using Keeper.DomainModel.Deposit;
using Keeper.Utils.DbInputOutput.CompositeTasks;
using Keeper.Utils.DbInputOutput.TxtTasks;
using Keeper.Utils.Rates;
using NUnit.Framework;
using FluentAssertions;

namespace Keeper.UnitTests.ByFunctional.DepositProcessing
{
    [TestFixture]
    public sealed class TestDepositTrafficEvaluator
    {
        DbLoadResult _loadResult;
        DepositTrafficEvaluator _underTest;

        [SetUp]
        public void SetUp()
        {
            _loadResult = new DbFromTxtLoader(new DbClassesInstanceParser(), new AccountTreeStraightener()).
                                               LoadDbFromTxt(Path.GetFullPath(@"TestDb\forDepositCalculationTest"));

            if (_loadResult.Code != 0) MessageBox.Show("Database wasn't loaded properly!");
            _underTest = new DepositTrafficEvaluator(new RateExtractor(_loadResult.Db));
            
        }

        [Test]
        public void EvaluateTraffic_Should_Evaluate()
        {
             var deposit = new Deposit();
            _underTest.EvaluateTraffic(deposit).CalculationData.DailyTable.Count.Should().Be(3);
        }
    }
}
