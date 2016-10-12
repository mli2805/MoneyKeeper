using System.Collections.Generic;

using FakeItEasy;

using FluentAssertions;

using Keeper.Properties;
using Keeper.Utils.DbInputOutput.CompositeTasks;
using Keeper.Utils.DbInputOutput.FileTasks;
using Keeper.Utils.FileSystem;

using NUnit.Framework;

namespace Keeper.UnitTests.Utils.DbInputOutput.FileTasks
{
	[TestFixture]
	public sealed class TestFileExistenceChecker
	{
		FileExistenceChecker _underTest;
		IFileSystem _fileSystem;
		IFile _file;

		static readonly Dictionary<string, int> sFiles = new Dictionary<string, int>
		  {
			  { "Accounts.txt", 215 },
		  };

		[SetUp]
		public void SetUp()
		{
			_fileSystem = A.Fake<IFileSystem>();
			_file = A.Fake<IFile>();
			_underTest = new FileExistenceChecker(_fileSystem);

			Settings.Default.TemporaryTxtDbPath = "temp path";
			A.CallTo(() => _fileSystem.PathCombine("temp path", "Accounts.txt")).Returns("combined path");
			A.CallTo(() => _fileSystem.GetFile("combined path")).Returns(_file);
		}

		[Test]
		public void Check_If_All_Right_Should_Return_Null()
		{
			// Arrange
			A.CallTo(() => _file.Exists).Returns(true);

			//Action & Assert
			_underTest.Check(sFiles).Should().BeNull();
		}

		[Test]
		public void Check_When_File_Is_Absent_Should_Return_Negative_DbLoadResult_With_His_Name_And_Appropriate_Explanation()
		{
			// Arrange
			A.CallTo(() => _file.Exists).Returns(false);

			//Action & Assert
			_underTest.Check(sFiles).ShouldBeEquivalentTo(new DbLoadResult(215, "Accounts.txt not found!"));
		}
	}
}