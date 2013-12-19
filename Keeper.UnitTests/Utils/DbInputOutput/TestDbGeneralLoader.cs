using System.Windows;
using FakeItEasy;
using FluentAssertions;
using Keeper.DomainModel;
using Keeper.Utils.DbInputOutput;
using Keeper.Utils.Dialogs;
using Keeper.Utils.FileSystem;

using NUnit.Framework;

namespace Keeper.UnitTests.Utils.DbInputOutput
{
	[TestFixture]
	public sealed class TestDbGeneralLoader
	{
		IMessageBoxer mMessageBoxer;
		IMyOpenFileDialog mOpenFileDialog;
		IDbSerializer mDbSerializer;
		IDbFromTxtLoader mFromTxtLoader;
		IDbFromZipLoader mFromZipLoader;
		IFileSystem mFileSystem;
		IFile mFile;
		KeeperDb mKeeperDb;

		[SetUp]
		public void SetUp()
		{
			mMessageBoxer = A.Fake<IMessageBoxer>();
			mOpenFileDialog = A.Fake<IMyOpenFileDialog>();
			mDbSerializer = A.Fake<IDbSerializer>();
			mFromTxtLoader = A.Fake<IDbFromTxtLoader>();
			mFromZipLoader = A.Fake<IDbFromZipLoader>();
			mFileSystem = A.Fake<IFileSystem>();
			
			mFile = A.Fake<IFile>();
			mKeeperDb = new KeeperDb();
		}

		[Test]
		public void Ctor_Should_CombinePath_DecryptAndDeserialize_DbFile_And_Set_Db_Property()
		{
			// Arrange
      A.CallTo(() => mFileSystem.PathCombine(A<string>.Ignored, A<string>.Ignored)).Returns("fullpath.dbx");
      A.CallTo(() => mDbSerializer.DecryptAndDeserialize("fullpath.dbx")).Returns(mKeeperDb);
      A.CallTo(() => mFileSystem.GetFile("fullpath.dbx")).Returns(mFile);
      A.CallTo(() => mFile.Exists).Returns(true);

			// Act
			var underTest = new DbGeneralLoader(mMessageBoxer, mOpenFileDialog, mDbSerializer, mFromTxtLoader, mFileSystem, mFromZipLoader);

			// Assert
			underTest.Db.Should().Be(mKeeperDb);
		}

		[Test] 
		public void Ctor_When_Default_File_Doesnt_Exist_Should_Warn_User()
		{
			// Arrange
      A.CallTo(() => mFileSystem.PathCombine(A<string>.Ignored, A<string>.Ignored)).Returns("fullpath.dbx");
      A.CallTo(() => mFileSystem.GetFile("fullpath.dbx")).Returns(mFile);
      A.CallTo(() => mFile.Exists).Returns(false);
			
			// Act
      new DbGeneralLoader(mMessageBoxer, mOpenFileDialog, mDbSerializer, mFromTxtLoader, mFileSystem, mFromZipLoader);

      A.CallTo(() => mMessageBoxer.Show("File 'fullpath.dbx' not found. " + 
			                                  "\n\n You will be offered to choose database file.",
			                                  "Error!", MessageBoxButton.OK, MessageBoxImage.Warning)).MustHaveHappened();
		}

		[Test] 
		public void Ctor_When_Default_File_Doesnt_Exist_Should_Ask_User_To_Change_FileName()
		{
			// Arrange
      A.CallTo(() => mFileSystem.PathCombine(A<string>.Ignored, A<string>.Ignored)).Returns("fullpath.dbx");
      A.CallTo(() => mFileSystem.GetFile("fullpath.dbx")).Returns(mFile);
      A.CallTo(() => mFile.Exists).Returns(false);


			// Act
      new DbGeneralLoader(mMessageBoxer, mOpenFileDialog, mDbSerializer, mFromTxtLoader, mFileSystem, mFromZipLoader);			

			// Assert
      A.CallTo(() => mOpenFileDialog.Show("*.*",
                                          "All files (*.*)|*.*|Keeper Database (.dbx)|*.dbx|Zip archive (with keeper database .zip)|*.zip|Text files (with data for keeper .txt)|*.txt",
                                          A<string>.Ignored)).MustHaveHappened();
    }

		[Test]
		public void Ctor_When_Cannot_Deserialize_Database_Should_Return_Null()
		{
			// Arrange
      A.CallTo(() => mFileSystem.PathCombine(A<string>.Ignored, A<string>.Ignored)).Returns("illegal.dbx");
      A.CallTo(() => mDbSerializer.DecryptAndDeserialize("illegal.dbx")).Returns(mKeeperDb);
      A.CallTo(() => mFileSystem.GetFile("illegal.dbx")).Returns(mFile);
      A.CallTo(() => mFile.Exists).Returns(true);

			// Act
      new DbGeneralLoader(mMessageBoxer, mOpenFileDialog, mDbSerializer, mFromTxtLoader, mFileSystem, mFromZipLoader);

      A.CallTo(() => mDbSerializer.DecryptAndDeserialize("illegal.dbx")).Returns(null);
    }

		[Test]
    public void Ctor_When_User_Choose_Txtfile_Should_Load_From_Txt()
		{
			// Arrange
      var newKeeperdb = new KeeperDb();
      A.CallTo(() => mFileSystem.PathCombine(A<string>.Ignored, A<string>.Ignored)).Returns("illegal.dbx");
      A.CallTo(() => mFileSystem.GetFile("illegal.dbx")).Returns(mFile);
      A.CallTo(() => mFile.Exists).Returns(true);
      A.CallTo(() => mFromTxtLoader.LoadDbFromTxt(ref mKeeperDb, "file.txt"))
			 .Returns(new DbLoadError())
       .AssignsOutAndRefParameters(mKeeperDb);

			// Act
      new DbGeneralLoader(mMessageBoxer, mOpenFileDialog, mDbSerializer, mFromTxtLoader, mFileSystem, mFromZipLoader);			

			// Assert
      mKeeperDb.ShouldBeEquivalentTo(newKeeperdb);
		}

		[Test]
		public void Ctor_When_User_Choose_Zipfile_Should_Load_From_Zip()
		{
      // Arrange
      var newKeeperdb = new KeeperDb();
      A.CallTo(() => mFileSystem.PathCombine(A<string>.Ignored, A<string>.Ignored)).Returns("illegal.dbx");
      A.CallTo(() => mFileSystem.GetFile("illegal.dbx")).Returns(mFile);
      A.CallTo(() => mFile.Exists).Returns(true);
      A.CallTo(() => mFromZipLoader.LoadDbFromZip(ref mKeeperDb, "file.zip"))
       .Returns(new DbLoadError())
       .AssignsOutAndRefParameters(mKeeperDb);

      // Act
      new DbGeneralLoader(mMessageBoxer, mOpenFileDialog, mDbSerializer, mFromTxtLoader, mFileSystem, mFromZipLoader);

      // Assert
      mKeeperDb.ShouldBeEquivalentTo(newKeeperdb);
    }

		[Test]
		public void Ctor_When_Cannot_Load_From_Text_File_Should_Set_Db_Property_To_Null()
		{
			// Arrange
			mKeeperDb = null;

			A.CallTo(() => mDbSerializer.DecryptAndDeserialize("full path")).Returns(null);
      A.CallTo(() => mFromTxtLoader.LoadDbFromTxt(ref mKeeperDb, "full path")).Returns(new DbLoadError { Code = 5 });

			// Act
      var result = new DbGeneralLoader(mMessageBoxer, mOpenFileDialog, mDbSerializer, mFromTxtLoader, mFileSystem, mFromZipLoader);			

			// Assert
			result.Db.Should().BeNull();
		}

	}
}