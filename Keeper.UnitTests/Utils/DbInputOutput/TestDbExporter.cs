using FakeItEasy;

using Keeper.Utils.DbInputOutput;

using NUnit.Framework;

namespace Keeper.UnitTests.Utils.DbInputOutput
{
	[TestFixture]
	public sealed class TestDbExporter
	{
		DbExporter mUnderTest;
		IDbGeneralLoader mDbGeneralLoader;

		[SetUp]
		public void SetUp()
		{
			mDbGeneralLoader = A.Fake<IDbGeneralLoader>();
			mUnderTest = new DbExporter(mDbGeneralLoader);
		}

		[Test]
		public void Ctor_Should_Assing_LoadResult()
		{
			Assert.That(mUnderTest, Is.Not.Null);
		}
	}
}