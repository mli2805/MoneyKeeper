using System.Collections.Generic;
using System.Windows;
using FakeItEasy;
using Keeper.Properties;
using Keeper.Utils;
using Keeper.Utils.Common;
using Keeper.Utils.DbInputOutput;
using Keeper.Utils.DbInputOutput.FileTasks;
using Keeper.Utils.Dialogs;
using Keeper.Utils.FileSystem;
using NUnit.Framework;
using FluentAssertions;


namespace Keeper.UnitTests.Utils.DbInputOutput.FileTasks
{
  [TestFixture]
  public sealed class TestDbLocator
  {
    DbLocator mUnderTest;
    IMessageBoxer mMessageBoxer;
    IMyOpenFileDialog mOpenFileDialog;
    IFileSystem mFileSystem;
    IFile mFile;
    IMySettings mySettings;

    [SetUp]
    public void SetUp()
    {
      mMessageBoxer = A.Fake<IMessageBoxer>();
      mOpenFileDialog = A.Fake<IMyOpenFileDialog>();
      mFileSystem = A.Fake<IFileSystem>();
      mFile = A.Fake<IFile>();
      mySettings = A.Fake<IMySettings>();

      A.CallTo(() => mySettings.GetSetting("DbPath")).Returns("path");
      A.CallTo(() => mySettings.GetSetting("DbxFile")).Returns("file.dbx");
      A.CallTo(() => mFileSystem.PathCombine("path", "file.dbx")).Returns(@"path\file.dbx");
      A.CallTo(() => mFileSystem.GetFile(@"path\file.dbx")).Returns(mFile);

      mUnderTest = new DbLocator(mMessageBoxer, mOpenFileDialog, mFileSystem, mySettings);
    }

    [Test]
    public void Locate_Should_Return_Filename_From_Settings()
    {
      A.CallTo(() => mFile.Exists).Returns(true);
      mUnderTest.Locate().Should().Be(@"path\file.dbx");
    }

    [Test]
    public void Locate_When_Default_File_Doesnt_Exist_Should_Ask_User_For_Another_File()
    {
      A.CallTo(() => mFile.Exists).Returns(false);
      mUnderTest.Locate();
      A.CallTo(() => mMessageBoxer.Show("File 'path\\file.dbx' not found. \n" +
        "\n You will be offered to choose database file. \n" +
        "\n Folder with chosen file will be accepted as new location for the database. " +
        "\n When exiting the program database and archive will be saved in this new folder.\n" +
        "\n Would you like to choose database or archive file?",
        "Error!", MessageBoxButton.YesNo, MessageBoxImage.Warning)).MustHaveHappened();
    }

    [Test]
    public void Locate_When_User_Refuses_To_Choose_Another_File_Should_Return_Null()
    {
      A.CallTo(() => mFile.Exists).Returns(false);
      A.CallTo(() => mMessageBoxer.Show("", "", MessageBoxButton.YesNo, MessageBoxImage.Warning))
        .WithAnyArguments().Returns(MessageBoxResult.No);

      mUnderTest.Locate().Should().BeNull();
    }

    [Test]
    public void Locate_When_User_Agrees_To_Choose_Another_File_Should_Show_Dialog()
    {
      // Arrange
      A.CallTo(() => mFile.Exists).Returns(false);
      A.CallTo(() => mMessageBoxer.Show("", "", MessageBoxButton.YesNo, MessageBoxImage.Warning))
        .WithAnyArguments().Returns(MessageBoxResult.Yes);

      // Act
      mUnderTest.Locate();

      // Assert
      A.CallTo(() => mOpenFileDialog.Show("*.*",
        "All files (*.*)|*.*|" +
        "Keeper Database (.dbx)|*.dbx|" +
        "Zip archive (with keeper database .zip)|*.zip|" +
        "Text files (with data for keeper .txt)|*.txt", "")).MustHaveHappened();
    }

    [Test]
    public void Locate_When_User_Cancels_File_Selection_Should_Return_Null()
    {
      // Arrange
      A.CallTo(() => mFile.Exists).Returns(false);
      A.CallTo(() => mMessageBoxer.Show("", "", MessageBoxButton.YesNo, MessageBoxImage.Warning))
        .WithAnyArguments().Returns(MessageBoxResult.Yes);
      A.CallTo(() => mOpenFileDialog.Show("", "", "")).WithAnyArguments().Returns("");

      // Act & Assert
      mUnderTest.Locate().Should().BeNull();
    }

    [Test]
    public void Locate_When_User_Selects_File_Should_Return_Filename()
    {
      // Arrange
      A.CallTo(() => mFile.Exists).Returns(false);
      A.CallTo(() => mMessageBoxer.Show("", "", MessageBoxButton.YesNo, MessageBoxImage.Warning))
        .WithAnyArguments().Returns(MessageBoxResult.Yes);
      A.CallTo(() => mOpenFileDialog.Show("", "", "")).WithAnyArguments().Returns("filename");

      // Act & Assert
      mUnderTest.Locate().Should().Be("filename");
    }
  }
}