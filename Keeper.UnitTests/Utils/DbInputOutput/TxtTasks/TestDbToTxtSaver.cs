using System.Collections.Generic;
using System.IO;
using FakeItEasy;
using Keeper.Utils.DbInputOutput.TxtTasks;
using NUnit.Framework;

namespace Keeper.UnitTests.Utils.DbInputOutput.TxtTasks
{
  [TestFixture]
  class TestDbToTxtSaver
  {
    private DbToTxtSaver mUnderTest;
    private IDbEntriesToStringListsConverter _dbEntriesToStringListsConverter;
    private IDbTxtFileWriter _dbTxtFileWriter;

    [SetUp]
    public void SetUp()
    {
      _dbEntriesToStringListsConverter = A.Fake<IDbEntriesToStringListsConverter>();
      _dbTxtFileWriter = A.Fake<IDbTxtFileWriter>();


      mUnderTest = new DbToTxtSaver(_dbEntriesToStringListsConverter,_dbTxtFileWriter);
    }

    [Test]
    public void SaveDbInTxt_Should_Run_Writer_For_Db_Tables()
    {
      // Arrangement
      var accountsToList = new List<string>();
      A.CallTo(() => _dbEntriesToStringListsConverter.ConvertAccountsToFileContent()).Returns(accountsToList);
//      var transactionsToList = new List<string>();
//      A.CallTo(() => _dbEntriesToStringListsConverter.ConvertTransactionsToFileContent()).Returns(transactionsToList);
      var transToList = new List<string>();
      A.CallTo(() => _dbEntriesToStringListsConverter.ConvertTranWithTagsToFileContent()).Returns(transToList);
      var associationsToList = new List<string>();
      A.CallTo(() => _dbEntriesToStringListsConverter.ConvertArticlesAssociationsToFileContent()).Returns(associationsToList);
      var ratesToList = new List<string>();
      A.CallTo(() => _dbEntriesToStringListsConverter.ConvertCurrencyRatesToFileContent()).Returns(ratesToList);
      // Act
      mUnderTest.SaveDbInTxt();
      // Assert
      A.CallTo(() => _dbTxtFileWriter.WriteDbFile("Accounts.txt", accountsToList)).MustHaveHappened();
//      A.CallTo(() => _dbTxtFileWriter.WriteDbFile("Transactions.txt", transactionsToList)).MustHaveHappened();
      A.CallTo(() => _dbTxtFileWriter.WriteDbFile("Transactions.txt", transToList)).MustHaveHappened();
      A.CallTo(() => _dbTxtFileWriter.WriteDbFile("ArticlesAssociations.txt", associationsToList)).MustHaveHappened();
      A.CallTo(() => _dbTxtFileWriter.WriteDbFile("CurrencyRates.txt", ratesToList)).MustHaveHappened();
    }
  }
}
