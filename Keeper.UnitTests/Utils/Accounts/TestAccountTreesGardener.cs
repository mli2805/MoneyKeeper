using Caliburn.Micro;

using FakeItEasy;

using Keeper.DomainModel;
using Keeper.Utils.Accounts;
using Keeper.ViewModels;

using NUnit.Framework;

namespace Keeper.UnitTests.Utils.Balances
{
	[TestFixture]
	public sealed class TestAccountTreesGardener
	{
		AccountTreesGardener mUnderTest;
		IWindowManager mWindowManager;
		Account mSelectedAccount;
		IMyFactory mMyFactory;

		[SetUp]
		public void SetUp()
		{
			mWindowManager = A.Fake<IWindowManager>();
			mMyFactory = A.Fake<IMyFactory>();
			mUnderTest = new AccountTreesGardener(new KeeperDb(), new AccountTreeStraightener(), 
			                                      mWindowManager, mMyFactory);

			mSelectedAccount = new Account();
		}

		[Test]
		public void AddAccount_Should_Show_AddAndEditAccountView_Dialog()
		{
			var accountInWork = new Account();
			A.CallTo(() => mMyFactory.CreateAccount(mSelectedAccount)).Returns(accountInWork);
			var vm = new AddAndEditAccountViewModel(new Account(), "");
			A.CallTo(() => mMyFactory.CreateAddAndEditAccountViewModel(accountInWork, "Добавить")).Returns(vm);

			mUnderTest.AddAccount(mSelectedAccount);

			A.CallTo(() => mWindowManager.ShowDialog(vm, null, null))
			 .MustHaveHappened();

		}
	}
}