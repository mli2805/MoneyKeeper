using FakeItEasy;
using FluentAssertions;
using Ionic.Zip;
using Keeper.Utils.DbInputOutput;
using Keeper.Utils.DbInputOutput.ZipTasks;
using Keeper.Utils.FileSystem;
using NUnit.Framework;

namespace Keeper.UnitTests.Utils.DbInputOutput.FileTasks
{
  [TestFixture]
  public sealed class TestDbUnzipper
  {
    DbUnzipper _underTest;
    IFileSystem _fileSystem;
    IFileExistenceChecker _existenceChecker;
    IZipFile _zipFile;
    IZipEntry _zipEntry;

    [SetUp]
    public void SetUp()
    {
      _fileSystem = A.Fake<IFileSystem>();
      _existenceChecker = A.Fake<IFileExistenceChecker>();
      _zipFile = A.Fake<IZipFile>();
      _zipEntry = A.Fake<IZipEntry>();

      A.CallTo(() => _fileSystem.GetFile("archive.zip").ReadZip()).Returns(_zipFile);
      A.CallTo(() => _zipFile.GetEnumerator().Current).Returns(_zipEntry);

      _underTest = new DbUnzipper(_fileSystem, _existenceChecker);
    }

    [Test]
    public void UnzipArchive_Should_Return_Null_If_All_Right()
    {
      // Arrange
      A.CallTo(
        () => _zipEntry.ExtractWithPassword(A<string>.Ignored, ExtractExistingFileAction.OverwriteSilently, "password"))
        .Returns(true);
      A.CallTo(() => _existenceChecker.Check(TxtFilesForDb.Dict)).WithAnyArguments().Returns(null);

      //Action & Assert
      _underTest.UnzipArchive("archive.zip").Should().BeNull();
    }

    public void UnzipArchive_When_Password_Is_Wrong_Should_Returns_Result_Bad_Password_Code_And_Explanation()
    {
      // Arrange
      A.CallTo(
        () => _zipEntry.ExtractWithPassword(A<string>.Ignored, ExtractExistingFileAction.OverwriteSilently, "wrong password"))
        .Returns(false);

      //Action & Assert
      _underTest.UnzipArchive("archive.zip").Code.Should().Be(21);
      _underTest.UnzipArchive("archive.zip").Explanation.Should().Be("Bad password!");
    }
  }
}