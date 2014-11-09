using System.Collections.ObjectModel;
using FakeItEasy;
using FluentAssertions;
using Keeper.ByFunctional.EditingAccounts;
using Keeper.DomainModel;
using Keeper.Utils.Accounts;
using Keeper.Utils.Dialogs;
using NUnit.Framework;

namespace Keeper.UnitTests.ByFunctional.EditingAccounts
{
	[TestFixture]
	public sealed class TestAccountTreesGardener
	{
	  private AccountTreesGardener _underTest;

    private IMyFactory _myFactory;
    private TellUser _tellUser;
    private AccountOperations _accountOperations;
    private DbIntegrity _dbIntegrity;
    private AskUser _askUser;

    private KeeperDb _db;
    private Account _auxiliaryChildAccount;
    private Account _childAccountWithOperation;
    private Account _emptyChildAccount;
    private Account _parentAccount;
    private Account _rootAccount;
	  private Account _rootAccountWithoutChildren;

    [SetUp]
    public void SetUp()
    {
      PrepareTestDb();

      _myFactory = A.Fake<IMyFactory>();
      _tellUser = A.Fake<TellUser>();  // нужен мессаж боксер как параметр конструктора
      _accountOperations = A.Fake<AccountOperations>();
      _dbIntegrity = A.Fake<DbIntegrity>();
      _askUser = A.Fake<AskUser>();

      _underTest = new AccountTreesGardener(_myFactory, _tellUser, _accountOperations, _dbIntegrity, _askUser);
    }

	  private void PrepareTestDb()
	  {
	    _rootAccountWithoutChildren = new Account("RootWithoutChildren");
	    _rootAccount = new Account("Root");
	    _parentAccount = new Account("Parent account");
	    _emptyChildAccount = new Account("Empty child account");
	    _childAccountWithOperation = new Account("Child account with operation");
	    _auxiliaryChildAccount = new Account("Auxiliary child account");

	    _rootAccount.Children.Add(_parentAccount);
	    _parentAccount.Children.Add(_emptyChildAccount);
	    _parentAccount.Children.Add(_childAccountWithOperation);
	    _parentAccount.Children.Add(_auxiliaryChildAccount);

	    _db = new KeeperDb
	            {Accounts = new ObservableCollection<Account>(), Transactions = new ObservableCollection<Transaction>()};
	    _db.Accounts.Add(_rootAccount);
	    _db.Accounts.Add(_rootAccountWithoutChildren);
	    _db.Transactions.Add(new Transaction() {Debet = _childAccountWithOperation});
	  }

	  [Test]
    public void RemoveAccount_Should_Remove_Account()
    {
      // Act
      _underTest.RemoveAccount(_emptyChildAccount);

      // Assert
      _parentAccount.Children.Count.Should().Be(2);
    }
	}
}