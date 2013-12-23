using FakeItEasy;

using Keeper.DomainModel;
using Keeper.Utils.DbInputOutput;

using NUnit.Framework;

using FluentAssertions;

namespace Keeper.UnitTests.Utils.DbInputOutput
{
	[TestFixture]
	public sealed class TestDbExporter
	{
		DbExporter mUnderTest;
		IDbGeneralLoader mDbGeneralLoader;
		DbLoadResult mSpecificResult;
		KeeperDb mSpecificKeeperDb;

		[SetUp]
		public void SetUp()
		{
			mSpecificKeeperDb = new KeeperDb();
			mSpecificResult = new DbLoadResult(mSpecificKeeperDb);
			mDbGeneralLoader = A.Fake<IDbGeneralLoader>();
			A.CallTo(() => mDbGeneralLoader.LoadByExtension()).Returns(mSpecificResult);
			mUnderTest = new DbExporter(mDbGeneralLoader);
		}

		[Test]
		public void Ctor_Should_Assing_LoadResult_What_LoadByExtension_Returns()
		{
			mUnderTest.LoadResult.Should().Be(mSpecificResult);
		}

		[Test]
		public void Ctor_Should_Assing_Db_With_Db_From_LoadResult()
		{

			mUnderTest.Db.Should().Be(mSpecificKeeperDb);
		}
	}
}