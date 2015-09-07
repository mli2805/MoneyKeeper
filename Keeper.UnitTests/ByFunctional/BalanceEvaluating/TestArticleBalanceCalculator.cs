using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using FluentAssertions;
using Keeper.ByFunctional.AccountEditing;
using Keeper.ByFunctional.BalanceEvaluating;
using Keeper.DomainModel;
using Keeper.Utils.DbInputOutput.CompositeTasks;
using Keeper.Utils.DbInputOutput.TxtTasks;
using NUnit.Framework;

namespace Keeper.UnitTests.ByFunctional.BalanceEvaluating
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

        [Test]
        public void GetArticleBalanceInUsdPlusFromMidnightToMidnight_Should_Evaluate()
        {
            var expectedSum = 26.120448179271708683473389355;
            var expectedTransactions = new List<string> { "  30.10.2014 $8 ", 
                                                          "  30.10.2014 $17 ", 
                                                          "  03.11.2014 $2 щетка" };

            var testedArticle = new Account("Одежда");

            var transactions = new List<string>();
            _underTest.GetArticleBalanceInUsdPlusFromMidnightToMidnight(testedArticle,
                                                  new Period(new DateTime(2014, 10, 30, 9, 5, 0), 
                                                             new DateTime(2014, 11, 3, 0, 0, 0)),
                                                  transactions).ShouldBeEquivalentTo(expectedSum);
           transactions.ShouldBeEquivalentTo(expectedTransactions); 

        }

    }
}
