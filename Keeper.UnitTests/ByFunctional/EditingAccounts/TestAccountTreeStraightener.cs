using System.Collections.ObjectModel;
using FluentAssertions;
using Keeper.DomainModel;
using Keeper.Utils.Accounts;
using Keeper.Utils.Common;
using NUnit.Framework;

namespace Keeper.UnitTests.Utils.Accounts
{
  [TestFixture]
  public sealed class TestAccountTreeStraightener
  {
    private AccountTreeStraightener mUnderTest;
    private KeeperDb mDb;
    Account mChild1Account;
    Account mChild2Account;
    Account mParentAccount;

    [SetUp]
    public void SetUp()
    {
      // Test DB
      mChild1Account = new Account("Existing account");
      mChild2Account = new Account("Another account");
      mParentAccount = new Account("Root");
      mParentAccount.Children.Add(mChild1Account);
      mParentAccount.Children.Add(mChild2Account);
      mDb = new KeeperDb() { Accounts = new ObservableCollection<Account>() };
      mDb.Accounts.Add(mParentAccount);

      mUnderTest = new AccountTreeStraightener();
    }

    [Test]
    public void Seek_Should_Find_Account_By_Name()
    {
      mUnderTest.Seek("Existing account", mDb.Accounts).Should().Be(mChild1Account);
    }

    [Test]
    public void Seek_If_No_Such_Name_Should_Return_Null()
    {
      mUnderTest.Seek("ExOsting account", mDb.Accounts).Should().BeNull();
    }

    [Test]
    public void Flatten_Should()
    {
      var result = mUnderTest.Flatten(mDb.Accounts);
      result.ShouldBeEquivalentTo(new[] { mParentAccount, mChild2Account, mChild1Account });// ShouldBeEquivalentTo порядок в списке не проверяет
    }

    [Test]
    public void FlattenWithLevels_Should()
    {
      var result = mUnderTest.FlattenWithLevels(mDb.Accounts);
      result.ShouldBeEquivalentTo(new []{new HierarchyItem<Account>(0, mParentAccount),
                                         new HierarchyItem<Account>(1, mChild1Account),
                                         new HierarchyItem<Account>(1, mChild2Account)});

    }

  }
}