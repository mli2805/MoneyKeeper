using System.Collections.ObjectModel;
using FluentAssertions;
using Keeper.ByFunctional.AccountEditing;
using Keeper.DomainModel;
using Keeper.Utils.Common;
using NUnit.Framework;

namespace Keeper.UnitTests.ByFunctional.AccountEditing
{
  [TestFixture]
  public sealed class TestAccountTreeStraightener
  {
    private AccountTreeStraightener _underTest;
    private KeeperDb _db;
    Account _child1Account;
    Account _child2Account;
    Account _parentAccount;

    [SetUp]
    public void SetUp()
    {
      // Test DB
      _child1Account = new Account("Existing account");
      _child2Account = new Account("Another account");
      _parentAccount = new Account("Root");
      _parentAccount.Children.Add(_child1Account);
      _parentAccount.Children.Add(_child2Account);
      _db = new KeeperDb() { Accounts = new ObservableCollection<Account>() };
      _db.Accounts.Add(_parentAccount);

      _underTest = new AccountTreeStraightener();
    }

    [Test]
    public void Seek_Should_Find_Account_By_Name()
    {
      _underTest.Seek("Existing account", _db.Accounts).Should().Be(_child1Account);
    }

    [Test]
    public void Seek_If_No_Such_Name_Should_Return_Null()
    {
      _underTest.Seek("ExOsting account", _db.Accounts).Should().BeNull();
    }

    [Test]
    public void Flatten_Should()
    {
      var result = _underTest.Flatten(_db.Accounts);
      result.ShouldBeEquivalentTo(new[] { _parentAccount, _child2Account, _child1Account });// ShouldBeEquivalentTo порядок в списке не проверяет
    }

    [Test]
    public void FlattenWithLevels_Should()
    {
      var result = _underTest.FlattenWithLevels(_db.Accounts);
      result.ShouldBeEquivalentTo(new []{new HierarchyItem<Account>(0, _parentAccount),
                                         new HierarchyItem<Account>(1, _child1Account),
                                         new HierarchyItem<Account>(1, _child2Account)});

    }

  }
}