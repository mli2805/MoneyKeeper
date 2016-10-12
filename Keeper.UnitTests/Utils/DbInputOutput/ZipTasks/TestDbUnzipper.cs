using System.Collections.Generic;

using FakeItEasy;

using Ionic.Zip;

using Keeper.Properties;
using Keeper.Utils.DbInputOutput.CompositeTasks;
using Keeper.Utils.DbInputOutput.FileTasks;
using Keeper.Utils.DbInputOutput.ZipTasks;
using Keeper.Utils.FileSystem;

using NUnit.Framework;

using FluentAssertions;

namespace Keeper.UnitTests.Utils.DbInputOutput.ZipTasks
{
	[TestFixture]
	public sealed class TestDbUnzipper
	{
		DbUnzipper mUnderTest;
		IFileSystem mFileSystem;
		IFile mZipFile;
		IZipFile mZip;
		IZipEntry mZipEntry;
		IFileExistenceChecker mFileExistenceChecker;
		readonly DbLoadResult mDbLoadResult = new DbLoadResult(0,"");

		[SetUp]
		public void SetUp()
		{
			mFileSystem = A.Fake<IFileSystem>();
			mZipFile = A.Fake<IFile>();
			A.CallTo(() => mFileSystem.GetFile("zip file name")).Returns(mZipFile);
			mZip = A.Fake<IZipFile>();
			A.CallTo(() => mZipFile.ReadZip()).Returns(mZip);
			mZipEntry = A.Fake<IZipEntry>();
			IEnumerator<IZipEntry> enumerator = new List<IZipEntry>() {mZipEntry}.GetEnumerator();
			A.CallTo(() => mZip.GetEnumerator()).Returns(enumerator);
			mFileExistenceChecker = A.Fake<IFileExistenceChecker>();
			mUnderTest = new DbUnzipper(mFileSystem, mFileExistenceChecker);
			Settings.Default.TemporaryTxtDbPath = "unpack directory";
		}

		[Test]
		public void UnzipArchive_Should_Unpack_Files_To_The_Temporary_Folder()
		{
			mUnderTest.UnzipArchive("zip file name");

			A.CallTo(() => mZipEntry.ExtractWithPassword("unpack directory", ExtractExistingFileAction.OverwriteSilently, "!opa1526"))
				.MustHaveHappened();
		}

		[Test]
		public void UnzipArchive_When_Password_Is_Wrong_Should_Return_Bad_Password_Result()
		{
			A.CallTo(() => mZipEntry.ExtractWithPassword("unpack directory", ExtractExistingFileAction.OverwriteSilently, "!opa1526"))
			 .Returns(false);

			var result = mUnderTest.UnzipArchive("zip file name");
			result.ShouldBeEquivalentTo(new DbLoadResult(21, "Bad password!"));
		}

		[Test]
		public void UnzipArchive_Should_Return_What_ExistenceChecker_Returns()
		{
			A.CallTo(() => mZipEntry.ExtractWithPassword("unpack directory", ExtractExistingFileAction.OverwriteSilently, "!opa1526"))
			 .Returns(true);
			A.CallTo(() => mFileExistenceChecker.Check(TxtFilesForDb.Dict)).Returns(mDbLoadResult);

			var result = mUnderTest.UnzipArchive("zip file name");
			result.Should().BeSameAs(mDbLoadResult);
		}
	}

}