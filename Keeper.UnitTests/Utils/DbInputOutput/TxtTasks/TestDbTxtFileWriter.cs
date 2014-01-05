using System.Collections.Generic;
using System.Text;
using FakeItEasy;
using Keeper.Utils;
using Keeper.Utils.DbInputOutput.TxtTasks;
using Keeper.Utils.FileSystem;
using NUnit.Framework;

namespace Keeper.UnitTests.Utils.DbInputOutput.TxtTasks
{
  [TestFixture]
  internal class TestDbTxtFileWriter
  {
    private DbTxtFileWriter mUnderTest;
    private IFileSystem _fileSystem;
    private IMySettings _mySettings;
    private IDirectory _directory;

    [SetUp]
    public void SetUp()
    {
      _fileSystem = A.Fake<IFileSystem>();
      _mySettings = A.Fake<IMySettings>();
      _directory = A.Fake<IDirectory>();

      mUnderTest = new DbTxtFileWriter(_fileSystem, _mySettings);

      A.CallTo(() => _mySettings.GetSetting("TemporaryTxtDbPath")).Returns("path");
      A.CallTo(() => _fileSystem.GetDirectory("path")).Returns(_directory);
    }

    [Test]
    public void WriteDbFile_Should_Write_Content_To_File()
    {
      //Arrange
      A.CallTo(() => _directory.Exists).Returns(true);
      A.CallTo(() => _fileSystem.PathCombine("path", "filename")).Returns("fullpath");
      var file = A.Fake<IFile>();
      A.CallTo(() => _fileSystem.GetFile("fullpath")).Returns(file);

      //Act
      var content = new List<string>();
      mUnderTest.WriteDbFile("filename",content);
      //Assert
      A.CallTo(() => file.WriteAllLines(content,Encoding.GetEncoding(1251))).MustHaveHappened();
    }

    [Test]
    public void WriteDbFile_If_Temporary_Folder_Doesnt_Exists_Should_Create_It()
    {
      //Arrange
      A.CallTo(() => _directory.Exists).Returns(false);
      //Act
      mUnderTest.WriteDbFile("filename", new List<string>());
      //Assert
      A.CallTo(() => _directory.Create()).MustHaveHappened();
      
    }
  }
}