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
		DbGeneralLoader mUnderTest;
		ILoader mLoader;
		IDbLocator mLocator;

		[SetUp]
		public void SetUp()
		{
			mLoader = A.Fake<ILoader>();
			mLocator = A.Fake<IDbLocator>();
			mUnderTest = new DbGeneralLoader(mLocator, new[] { mLoader });
		}

		[Test]
		public void LoadByExtension_Should_Select_Loader_And_Load_Db()
		{
			var dbLoadResult = new DbLoadResult(null);
			A.CallTo(() => mLocator.Locate()).Returns("file.txt");
			A.CallTo(() => mLoader.SupportedExtension).Returns(".txt");
			A.CallTo(() => mLoader.Load("file.txt")).Returns(dbLoadResult);
			

			var result = mUnderTest.LoadByExtension();
			result.Should().Be(dbLoadResult);
		}
	}

}