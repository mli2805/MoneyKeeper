using System.Windows;

using FakeItEasy;

using FluentAssertions;
using Keeper.DomainModel;
using Keeper.Properties;
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
			mFileSystem = A.Fake<IFileSystem>();
			Settings.Default.SavePath = @"C:\";
			mFile = A.Fake<IFile>();
			mKeeperDb = new KeeperDb();
			A.CallTo(() => mFileSystem.PathCombine(@"C:\", "Keeper.dbx")).Returns("full path");
			A.CallTo(() => mFileSystem.GetFile("full path")).Returns(mFile);
			A.CallTo(() => mFile.Exists).Returns(true);
		}

		[Test]
		public void Ctor_Should_CombinePath_DecryptAndDeserialize_DbFile_And_Set_Db_Property()
		{
			// Arrange
			A.CallTo(() => mDbSerializer.DecryptAndDeserialize("full path")).Returns(mKeeperDb);

			// Act
			var underTest = new DbGeneralLoader(mMessageBoxer,
			                                    mOpenFileDialog, mDbSerializer, mFromTxtLoader, mFileSystem);

			// Assert
			underTest.Db.Should().Be(mKeeperDb);
		}

		[Test] 
		public void Ctor_When_Default_File_Doesnt_Exist_Should_Warn_User()
		{
			// Arrange
			A.CallTo(() => mFile.Exists).Returns(false);
			
			// Act
			new DbGeneralLoader(mMessageBoxer,
			                    mOpenFileDialog, mDbSerializer, mFromTxtLoader, mFileSystem); 
			
			
			A.CallTo(() => mMessageBoxer.Show("File 'full path' not found. " + 
			                                  "\n\n You will be offered to choose database file.",
			                                  "Error!", MessageBoxButton.OK, MessageBoxImage.Warning)).MustHaveHappened();
		}

		[Test] 
		public void Ctor_When_Default_File_Doesnt_Exist_Should_Ask_User_To_Change_FileName()
		{
			// Arrange
			A.CallTo(() => mFile.Exists).Returns(false);
			A.CallTo(() => mOpenFileDialog.Show(".dbx",
			                                    "Keeper Database (.dbx)|*.dbx", "full path", 
			                                    @"g:\local_keeperDb\Keeper.dbx")).Returns("changed filename");

			// Act
			new DbGeneralLoader(mMessageBoxer,
			                    mOpenFileDialog, mDbSerializer, mFromTxtLoader, mFileSystem);

			// Assert
			A.CallTo(() => mDbSerializer.DecryptAndDeserialize("changed filename"))
			 .MustHaveHappened();
		}

		[Test]
		public void Ctor_When_Cannot_Deserialize_Database_Should_Warn_User()
		{
			// Arrange
			A.CallTo(() => mDbSerializer.DecryptAndDeserialize("full path")).Returns(null);

			// Act
			new DbGeneralLoader(mMessageBoxer,
			                    mOpenFileDialog, mDbSerializer, mFromTxtLoader, mFileSystem);

			A.CallTo(() => mMessageBoxer.Show("File 'full path' not found. \n Last zip will be used.",
			                                  "Error!", MessageBoxButton.OK, MessageBoxImage.Error)).MustHaveHappened();
		}

		[Test]
		public void Ctor_When_Cannot_Deserialize_Database_Should_Try_To_Load_From_Text_File()
		{
			// Arrange
			mKeeperDb = null;
			var newKeeperdb = new KeeperDb();

			A.CallTo(() => mDbSerializer.DecryptAndDeserialize("full path")).Returns(null);
			A.CallTo(() => mFromTxtLoader.LoadFromLastZip(ref mKeeperDb))
			 .Returns(new DbLoadError())
			 .AssignsOutAndRefParameters(newKeeperdb);

			// Act
			var result = new DbGeneralLoader(mMessageBoxer,
			                                 mOpenFileDialog, mDbSerializer, mFromTxtLoader, mFileSystem);

			// Assert
			result.Db.Should().Be(newKeeperdb);
		}

		[Test]
		public void Ctor_When_Cannot_Load_From_Text_File_Should_Warn_User()
		{
			// Arrange
			mKeeperDb = null;

			A.CallTo(() => mDbSerializer.DecryptAndDeserialize("full path")).Returns(null);
			A.CallTo(() => mFromTxtLoader.LoadFromLastZip(ref mKeeperDb)).Returns(new DbLoadError
				{
					Explanation = "Explanation",
					Code = 5
				});

			// Act
			new DbGeneralLoader(mMessageBoxer,
			                    mOpenFileDialog, mDbSerializer, mFromTxtLoader, mFileSystem);

			// Assert
			A.CallTo(() => mMessageBoxer.Show("Explanation. \n Application will be closed!",
			                                  "Error!", MessageBoxButton.OK, MessageBoxImage.Error)).MustHaveHappened();
		}
		[Test]
		public void Ctor_When_Cannot_Load_From_Text_File_Should_Set_Db_Property_To_Null()
		{
			// Arrange
			mKeeperDb = null;

			A.CallTo(() => mDbSerializer.DecryptAndDeserialize("full path")).Returns(null);
			A.CallTo(() => mFromTxtLoader.LoadFromLastZip(ref mKeeperDb)).Returns(new DbLoadError { Code = 5 });

			// Act
			var result = new DbGeneralLoader(mMessageBoxer,
			                                 mOpenFileDialog, mDbSerializer, mFromTxtLoader, mFileSystem);

			// Assert
			result.Db.Should().BeNull();
		}

	}
}