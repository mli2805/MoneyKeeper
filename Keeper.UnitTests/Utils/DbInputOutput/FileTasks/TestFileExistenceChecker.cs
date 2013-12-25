using FakeItEasy;
using FluentAssertions;
using Keeper.Properties;
using Keeper.Utils.DbInputOutput.FileTasks;
using Keeper.Utils.DbInputOutput.ZipTasks;
using Keeper.Utils.FileSystem;
using NUnit.Framework;

namespace Keeper.UnitTests.Utils.DbInputOutput
{
  [TestFixture]
  public sealed class TestFileExistenceChecker
  {
    FileExistenceChecker _underTest;
    IFileSystem _fileSystem;
    IFile _file;


    [SetUp]
    public void SetUp()
    {
      _fileSystem = A.Fake<IFileSystem>();
      _file = A.Fake<IFile>();
      _underTest = new FileExistenceChecker(_fileSystem);
    }

    [Test]
    public void Check_Should_Return_Null_If_All_Right()
    {
      // Arrange
      A.CallTo(() => _fileSystem.GetFile(A<string>.Ignored)).Returns(_file);
      A.CallTo(() => _file.Exists).Returns(true);

      //Action & Assert
      _underTest.Check(TxtFilesForDb.Dict).Should().BeNull();
    }

    [Test]
    public void Check_When_File_Is_Absent_Should_Return_Negative_DbLoadResult_With_His_Name_And_Appropriate_In_Explanation()
    {
      // Arrange
      A.CallTo(() => _fileSystem.GetFile(_fileSystem.PathCombine(Settings.Default.TemporaryTxtDbPath, "Account.txt"))).Returns(_file);
      A.CallTo(() => _file.Exists).Returns(false);

      //Action & Assert
      var testResult = _underTest.Check(TxtFilesForDb.Dict);
      testResult.Explanation.Should().Be("Accounts.txt not found!");
      testResult.Code.Should().Be(215);
    }
  }
}