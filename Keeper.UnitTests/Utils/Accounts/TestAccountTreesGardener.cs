using System.Collections.ObjectModel;
using Caliburn.Micro;

using FakeItEasy;

using Keeper.DomainModel;
using Keeper.Utils.Accounts;
using Keeper.ViewModels;

using NUnit.Framework;
using FluentAssertions;

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
//			mMyFactory = A.Fake<IMyFactory>();
			mMyFactory = new MyFactory();
      var db = new KeeperDb(){Accounts = new ObservableCollection<Account>(){new Account()}};
			mUnderTest = new AccountTreesGardener(db, new AccountTreeStraightener(), 
			                                      mWindowManager, new MyFactory());

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

			A.CallTo(() => mWindowManager.ShowDialog(vm, null, null)).MustHaveHappened();
		}

    [Test]
    public void AddAccount_Should_Create_Child_For_Income_Account_And_Set_It_As_Selected()
    {
      var oldSelection = mSelectedAccount;
      // парные арренджменты

      // при создании аккаунта, создастся не какой-то новый, а именно accountInWork
			var accountInWork = new Account();
      A.CallTo(() => mMyFactory.CreateAccount(mSelectedAccount)).Returns(accountInWork);
      // тогда
      // при создании вьюмодели можно передать accountInWork
      // и получить не какую-то вьюмодель , а инстанс vm
      var vm = new AddAndEditAccountViewModel(new Account(), "");
      A.CallTo(() => mMyFactory.CreateAddAndEditAccountViewModel(accountInWork, "Добавить")).Returns(vm);
      // тогда
      // можно "запустить" именно инстанс vm
      A.CallTo(() => mWindowManager.ShowDialog(vm, null, null)).Returns(true);


      // Action
      var result = mUnderTest.AddAccount(mSelectedAccount);

      // Assert
      result.Should().Be(accountInWork);
      result.Parent.Should().Be(oldSelection);
    }
	}
}