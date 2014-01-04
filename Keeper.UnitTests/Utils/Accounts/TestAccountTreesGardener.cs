using System.Collections.ObjectModel;
using System.Windows;
using Caliburn.Micro;
using FakeItEasy;
using Keeper.DomainModel;
using Keeper.Utils.Accounts;
using Keeper.Utils.Dialogs;
using Keeper.ViewModels;
using NUnit.Framework;
using FluentAssertions;

namespace Keeper.UnitTests.Utils.Accounts
{
  [TestFixture]
  public sealed class TestAccountTreesGardener
  {
    AccountTreesGardener mUnderTest;
    IWindowManager mWindowManager;
    Account mSelectedAccount, accountInWork;
    IMyFactory mMyFactory;
    private IMessageBoxer mMessageBoxer;
    private KeeperDb mDb;


    [SetUp]
    public void SetUp()
    {
      mWindowManager = A.Fake<IWindowManager>();
      mMyFactory = A.Fake<IMyFactory>();
      mMessageBoxer = A.Fake<IMessageBoxer>();
      mDb = new KeeperDb() { Accounts = new ObservableCollection<Account>() };
      mUnderTest = new AccountTreesGardener(mDb, new AccountTreeStraightener(), mWindowManager, mMyFactory, mMessageBoxer);

      mSelectedAccount = new Account("Selected account");
    }

    [Test]
    public void AddAccount_Should_Create_And_Return_Child_For_Incoming_Account()
    {
      mDb.Accounts.Add(new Account("example"));
      var oldSelection = mSelectedAccount;

      // парный арренджмент. нужен если в коде есть = new TypeCtor();
      // тогда получаем в переменную результат этого конструктора и
      // говорим что фейковый конструктор вернет именно эту переменную

      // при создании аккаунта, создастся не какой-то новый, а именно accountInWork
      accountInWork = new Account() { Parent = mSelectedAccount };                            // это не аренджмент БД, на которой будет проводиться тест, это повторение кода ?
      A.CallTo(() => mMyFactory.CreateAccount(mSelectedAccount)).Returns(accountInWork);
      // тогда дальше по коду теста можно использовать инстанс, якобы созданный через NEW 

      var vm = new AddAndEditAccountViewModel(new Account(), "");
      A.CallTo(() => mMyFactory.CreateAddAndEditAccountViewModel(accountInWork, "Добавить")).Returns(vm);
      A.CallTo(() => mWindowManager.ShowDialog(vm, null, null)).Returns(true);

      // Action
      var result = mUnderTest.AddAccount(mSelectedAccount);

      // Assert
      result.Should().Be(accountInWork);
      result.Parent.Should().Be(oldSelection);
    }

    [Test]
    public void AddAccount_If_User_Canceled_Should_Return_Null()
    {
      accountInWork = new Account() { Parent = mSelectedAccount };
      A.CallTo(() => mMyFactory.CreateAccount(mSelectedAccount)).Returns(accountInWork);

      var vm = new AddAndEditAccountViewModel(new Account(), "");
      A.CallTo(() => mMyFactory.CreateAddAndEditAccountViewModel(accountInWork, "Добавить")).Returns(vm);
      A.CallTo(() => mWindowManager.ShowDialog(vm, null, null)).Returns(false);

      // Action & Assert
      mUnderTest.AddAccount(mSelectedAccount).Should().Be(null);
    }

    //----------------

    [Test]
    public void ChangeAccount_If_User_Canceled_Shouldnt_Change_Incoming_Account()
    {
      var oldSelection = mSelectedAccount;

      var vm = new AddAndEditAccountViewModel(new Account(), "");
      A.CallTo(() => mMyFactory.CreateAddAndEditAccountViewModel(accountInWork, "Добавить")).Returns(vm);
      A.CallTo(() => mWindowManager.ShowDialog(vm, null, null)).Returns(false);

      // Action
      mUnderTest.ChangeAccount(mSelectedAccount);

      // Assert
      mSelectedAccount.ShouldBeEquivalentTo(oldSelection);
    }

    // если пользователь изменил надо иметь дерево для теста

    //----------------
    [Test]
    public void RemoveAccount_If_Account_Is_Root_Should_Refuse_With_Message()
    {
      // Action
      mUnderTest.RemoveAccount(mSelectedAccount);
      // Assert
      A.CallTo(() => mMessageBoxer.Show("Корневой счет нельзя удалять!", "Отказ!", MessageBoxButton.OK, MessageBoxImage.Stop)).MustHaveHappened();
    }

    [Test]
    public void RemoveAccount_If_Account_Isnt_Leaf_Should_Refuse_With_Message()
    {
      // Arrangement
      var parent = new Account();
      var child = new Account();
      mSelectedAccount.Parent = parent;
      mSelectedAccount.Children.Add(child);
      // Action
      mUnderTest.RemoveAccount(mSelectedAccount);
      // Assert
      A.CallTo(() => mMessageBoxer.Show("Удалять разрешено \n только конечные листья дерева счетов!", "Отказ!", MessageBoxButton.OK, MessageBoxImage.Stop)).MustHaveHappened();
    }

    [Test]
    public void RemoveAccount_If_Account_Used_By_Any_Transaction_Should_Refuse_With_Message()
    {
      // Arrangement
      var parent = new Account();
      mSelectedAccount.Parent = parent;
      mDb.Transactions = new ObservableCollection<Transaction>() { new Transaction() { Credit = mSelectedAccount } };
      // Action
      mUnderTest.RemoveAccount(mSelectedAccount);
      // Assert
      A.CallTo(() => mMessageBoxer.Show("Этот счет используется в проводках!", "Отказ!", MessageBoxButton.OK, MessageBoxImage.Stop)).MustHaveHappened();
    }

    [Test]
    public void RemoveAccount_If_User_Doesnt_Confirm_Should_Exit_Without_Removal()
    {
      // Arrangement
      var parent = new Account();
      mSelectedAccount.Parent = parent;
      parent.Children.Add(mSelectedAccount);
      mDb.Accounts.Add(parent);

      mDb.Transactions = new ObservableCollection<Transaction>() { new Transaction() };

      A.CallTo(() => mMessageBoxer.Show(string.Format(AccountTreesGardener.CONFIRMATION_QUESTION, mSelectedAccount.Name),
                                "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question)).Returns(MessageBoxResult.No);
      // Action
      mUnderTest.RemoveAccount(mSelectedAccount);
      // Assert
      new AccountTreeStraightener().Seek(mSelectedAccount.Name, mDb.Accounts).Should().NotBeNull();
    }

    [Test]
    public void RemoveAccount_Should_Remove_Account()
    {
      // Arrangement
      var parent = new Account("Parent");
      mSelectedAccount.Parent = parent;
      parent.Children.Add(mSelectedAccount);
      mDb.Accounts.Add(parent);

      mDb.Transactions = new ObservableCollection<Transaction>() { new Transaction() };

      A.CallTo(() => mMessageBoxer.Show(string.Format(AccountTreesGardener.CONFIRMATION_QUESTION, mSelectedAccount.Name),
                                "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question)).Returns(MessageBoxResult.Yes);
      // Action
      mUnderTest.RemoveAccount(mSelectedAccount);
      // Assert
      new AccountTreeStraightener().Seek(mSelectedAccount.Name, mDb.Accounts).Should().BeNull();
    }
  }
}