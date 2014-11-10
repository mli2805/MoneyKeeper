using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using FluentAssertions;
using Keeper.DomainModel;
using Keeper.Utils.Accounts;
using Keeper.Utils.Balances;
using Keeper.Utils.DbInputOutput.CompositeTasks;
using Keeper.Utils.DbInputOutput.TxtTasks;
using NUnit.Framework;

namespace Keeper.UnitTests.ByFunctional.EvaluatingBalances
{
    [TestFixture]
    public sealed class TestArticleBalanceCalculator
    {
        DbLoadResult _loadResult;
        ArticleBalanceCalculator _underTest;

        [SetUp]
        public void SetUp()
        {
            _loadResult = new DbFromTxtLoader(new DbClassesInstanceParser(),
            new AccountTreeStraightener()).LoadDbFromTxt(Path.GetFullPath(@"TestDb\forBalanceEvaluationTest"));

            if (_loadResult.Code != 0) MessageBox.Show("Database wasn't loaded properly!");

            _underTest = new ArticleBalanceCalculator(_loadResult.Db);
        }

        public void GetArticleBalanceInUsdPlus_Should_Evaluate()
        {
            const decimal expectedSum = 57;
            var expectedTransactions = new List<string>{ "", ""};

            var testedArticle = new Account("Одежда");
            _underTest.GetArticleBalanceInUsdPlus(testedArticle,
                                                  new Period(new DateTime(2014, 10, 30, 9, 5, 0), new DateTime(2014, 11, 3, 0, 0, 0)),
                                                  expectedTransactions).ShouldBeEquivalentTo(expectedSum);
            
        }

    }
}
