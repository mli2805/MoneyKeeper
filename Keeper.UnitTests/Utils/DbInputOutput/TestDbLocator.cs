using FakeItEasy;

using Keeper.Utils.DbInputOutput;
using Keeper.Utils.Dialogs;
using Keeper.Utils.FileSystem;

using NUnit.Framework;

namespace Keeper.UnitTests.Utils.DbInputOutput
{
	[TestFixture]
	public sealed class TestDbLocator
	{
		DbLocator mUnderTest;
		IMessageBoxer mMessageBoxer;
		IMyOpenFileDialog mMyOpenFileDialog;
		IFileSystem mFileSystem;

		[SetUp]
		public void SetUp()
		{
			mMessageBoxer = A.Fake<IMessageBoxer>();
			mMyOpenFileDialog = A.Fake<IMyOpenFileDialog>();
			mFileSystem = A.Fake<IFileSystem>();
			mUnderTest = new DbLocator(mMessageBoxer, mMyOpenFileDialog, mFileSystem);
		}

		[Test]
		public void X_When_Y_Should_Z()
		{
		}
	}
}