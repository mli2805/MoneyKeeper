using FakeItEasy;
using FluentAssertions;
using Keeper.DomainModel;
using Keeper.Utils.DbInputOutput;
using NUnit.Framework;

namespace Keeper.UnitTests.Utils.DbInputOutput.FileTasks
{
  [TestFixture]
  public sealed class TestZippedTxtLoader
  {
    ZippedTxtLoader _underTest;
    IDbUnzipper _unzipper;
    IDbFromTxtLoader _txtLoader;

    [SetUp]
    public void SetUp()
    {
      _unzipper = A.Fake<IDbUnzipper>();
      _txtLoader = A.Fake<IDbFromTxtLoader>();

      _underTest = new ZippedTxtLoader(_unzipper, _txtLoader);
    }

    [Test]
    public void Load_Should_Unzip_Txt_And_Load_Db()
    {
      // Arrangements
      A.CallTo(() => _unzipper.UnzipArchive(A<string>.Ignored)).Returns(null);
      var db = new KeeperDb();
      var loadResult = new DbLoadResult(db);
      A.CallTo(() => _txtLoader.LoadDbFromTxt(A<string>.Ignored)).Returns(loadResult);

      // Act and Assert
      _underTest.Load(A<string>.Ignored).Db.Should().Be(db);
    }

    [Test]
    public void Load_When_Unzipper_Returns_Error_Should_Translates_Error()
    {
      // Arrangements
      A.CallTo(() => _unzipper.UnzipArchive(A<string>.Ignored)).Returns(new DbLoadResult(1, "error during unzipping"));

      // Act and Assert
      var testResult = _underTest.Load(A<string>.Ignored);
      testResult.Code.Should().Be(1);
      testResult.Explanation.Should().Be("error during unzipping");
    }

    [Test]
    public void Load_When_TxtLoader_Returns_Error_Should_Translates_Error()
    {
      // Arrangements
      A.CallTo(() => _unzipper.UnzipArchive(A<string>.Ignored)).Returns(null);
      var db = new KeeperDb();
      A.CallTo(() => _txtLoader.LoadDbFromTxt("")).WithAnyArguments().Returns(new DbLoadResult(1, "error during loading"));

      // Act and Assert
      var testResult = _underTest.Load(A<string>.Ignored);
      testResult.Code.Should().Be(1);
      testResult.Explanation.Should().Be("error during loading");
    }
  }
}