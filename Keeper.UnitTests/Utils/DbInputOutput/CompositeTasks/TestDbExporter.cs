﻿using FakeItEasy;
using Keeper.Utils.DbInputOutput.CompositeTasks;

using NUnit.Framework;

using FluentAssertions;
using Keeper.DomainModel.DbTypes;

namespace Keeper.UnitTests.Utils.DbInputOutput.CompositeTasks
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