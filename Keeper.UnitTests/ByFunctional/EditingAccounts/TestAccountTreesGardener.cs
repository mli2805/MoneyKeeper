using System.Collections.ObjectModel;
using FakeItEasy;
using FluentAssertions;
using Keeper.ByFunctional.EditingAccounts;
using Keeper.DomainModel;
using Keeper.Utils.Accounts;
using NUnit.Framework;

namespace Keeper.UnitTests.ByFunctional.EditingAccounts
{
	[TestFixture]
	public sealed class TestAccountTreesGardener
	{
        private AccountTreesGardener _underTest;

        private IAccountFactory _accountFactory;
        private IUserInformator _userInformator;
        private IAccountLowLevelOperator _accountLowLevelOperator;
        private IAccountCantBeDeletedChecker _accountCantBeDeletedChecker;
        private IUserEquirer _userEquirer;

        private KeeperDb _db;
        private Account _childAccountWithOperation;
        private Account _emptyChildAccount;
        private Account _parentAccount;
        private Account _rootAccount;
        private Account _rootAccountWithoutChildren;

        [SetUp]
        public void SetUp()
        {
            PrepareTestDb();

            _accountFactory = A.Fake<IAccountFactory>();
            _userInformator = A.Fake<IUserInformator>(); 
            _accountLowLevelOperator = A.Fake<IAccountLowLevelOperator>();
            _accountCantBeDeletedChecker = A.Fake<IAccountCantBeDeletedChecker>();
            _userEquirer = A.Fake<IUserEquirer>();

            _underTest = new AccountTreesGardener(_accountFactory, _userInformator, _accountLowLevelOperator, _accountCantBeDeletedChecker, _userEquirer);
        }

        private void PrepareTestDb()
        {
            _rootAccountWithoutChildren = new Account("RootWithoutChildren");
            _rootAccount = new Account("Root");
            _parentAccount = new Account("Parent account");
            _emptyChildAccount = new Account("Empty child account");
            _childAccountWithOperation = new Account("Child account with operation");

            _rootAccount.Children.Add(_parentAccount);
            _parentAccount.Children.Add(_emptyChildAccount);
            _parentAccount.Children.Add(_childAccountWithOperation);

            _db = new KeeperDb { Accounts = new ObservableCollection<Account>(), Transactions = new ObservableCollection<Transaction>() };
            _db.Accounts.Add(_rootAccount);
            _db.Accounts.Add(_rootAccountWithoutChildren);
            _db.Transactions.Add(new Transaction() { Debet = _childAccountWithOperation });
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