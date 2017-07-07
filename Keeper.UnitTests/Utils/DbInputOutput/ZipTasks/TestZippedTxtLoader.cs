using FakeItEasy;

using FluentAssertions;

using Keeper.Properties;
using Keeper.Utils.DbInputOutput.CompositeTasks;
using Keeper.Utils.DbInputOutput.TxtTasks;
using Keeper.Utils.DbInputOutput.ZipTasks;

using NUnit.Framework;

namespace Keeper.UnitTests.Utils.DbInputOutput.ZipTasks
{
	[TestFixture]
	public sealed class TestZippedTxtLoader
	{
		ZippedTxtLoader _underTest;
		IDbUnzipper _unzipper;
		IDbFromTxtLoader _txtLoader;
		readonly DbLoadResult mDbLoadResult = new DbLoadResult(1, "error during unzipping");

		[SetUp]
		public void SetUp()
		{
			_unzipper = A.Fake<IDbUnzipper>();
			_txtLoader = A.Fake<IDbFromTxtLoader>();

			_underTest = new ZippedTxtLoader(_unzipper, _txtLoader);
			Settings.Default.TemporaryTxtDbPath = "path from settings";
		}

		[Test]
		public void Load_Should_Return_What_LoadDbFromTxt_Returns()
		{
			// Arrangements
			A.CallTo(() => _unzipper.UnzipArchive("never mind")).Returns(null);
			A.CallTo(() => _txtLoader.LoadDbFromTxt("path from settings")).Returns(mDbLoadResult);

			// Act and Assert
			_underTest.Load("never mind").Should().Be(mDbLoadResult);
		}

		[Test]
		public void Load_When_Unzipper_Returns_Error_Should_Translate_Error()
		{
			// Arrangements
			A.CallTo(() => _unzipper.UnzipArchive("never mind")).Returns(mDbLoadResult);

			// Act and Assert
			_underTest.Load("never mind").Should().Be(mDbLoadResult);
		}
	}
}