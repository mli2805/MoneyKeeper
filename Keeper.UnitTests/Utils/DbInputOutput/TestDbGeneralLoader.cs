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
			mLocator = A.Fake<IDbLocator>();

			mLoader = A.Fake<ILoader>();
			A.CallTo(() => mLoader.AssociatedExtension).Returns(".txt");

			mUnderTest = new DbGeneralLoader(mLocator, new[] { mLoader });
		}

		[Test]
		public void LoadByExtension_Should_Select_Loader_And_Load_Db()
		{
			var dbLoadResult = new DbLoadResult(null);
			A.CallTo(() => mLocator.Locate()).Returns("file.txt");
			A.CallTo(() => mLoader.Load("file.txt")).Returns(dbLoadResult);
			

			var result = mUnderTest.LoadByExtension();
			result.Should().Be(dbLoadResult);
		}
		[Test]
		public void LoadByExtension_When_Locate_Returns_Null_Should_Return_User_Refused_Error()
		{
			A.CallTo(() => mLocator.Locate()).Returns(null);
			var result = mUnderTest.LoadByExtension();
			result.Explanation.Should().Be("User refused to choose another file");
		}
		[Test]
		public void LoadByExtension_When_Locate_Returns_Wrong_Extension_Return_WrongExtension_Error()
		{
			A.CallTo(() => mLocator.Locate()).Returns("file.wrong");
			var result = mUnderTest.LoadByExtension();
			result.Explanation.Should().Be("User has chosen file with wrong extension");
		}

	}

}