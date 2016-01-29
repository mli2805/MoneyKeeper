using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Keeper.ByFunctional.AccountEditing;
using Keeper.DomainModel;

namespace Keeper.ViewModels.Transactions
{
    public class ListsForComboboxes : PropertyChangedBase
    {
        public List<Account> ItemsForDebit { get; set; }


        private List<Account> _myAccounts;
        private List<CurrencyCodes> _currencyList;
        private List<Account> _myAccountsForShopping;
        private List<Account> _accountsWhoTakesMyMoney;
        private List<Account> _accountsWhoGivesMeMoney;
        private List<Account> _incomeArticles;
        private List<Account> _expenseArticles;
        private List<Account> _bankAccounts;

        public List<CurrencyCodes> CurrencyList
        {
            get { return _currencyList; }
            private set
            {
                if (Equals(value, _currencyList)) return;
                _currencyList = value;
                NotifyOfPropertyChange(() => CurrencyList);
            }
        }
        public List<Account> MyAccounts
        {
            get { return _myAccounts; }
            set
            {
                if (Equals(value, _myAccounts)) return;
                _myAccounts = value;
                NotifyOfPropertyChange(() => MyAccounts);
            }
        }
        public List<Account> MyAccountsForShopping
        {
            get { return _myAccountsForShopping; }
            set
            {
                if (Equals(value, _myAccountsForShopping)) return;
                _myAccountsForShopping = value;
                NotifyOfPropertyChange(() => MyAccountsForShopping);
            }
        }
        public List<Account> AccountsWhoTakesMyMoney
        {
            get { return _accountsWhoTakesMyMoney; }
            set
            {
                if (Equals(value, _accountsWhoTakesMyMoney)) return;
                _accountsWhoTakesMyMoney = value;
                NotifyOfPropertyChange(() => AccountsWhoTakesMyMoney);
            }
        }
        public List<Account> AccountsWhoGivesMeMoney
        {
            get { return _accountsWhoGivesMeMoney; }
            set
            {
                if (Equals(value, _accountsWhoGivesMeMoney)) return;
                _accountsWhoGivesMeMoney = value;
                NotifyOfPropertyChange(() => AccountsWhoGivesMeMoney);
            }
        }
        public List<Account> IncomeArticles
        {
            get { return _incomeArticles; }
            set
            {
                if (Equals(value, _incomeArticles)) return;
                _incomeArticles = value;
                NotifyOfPropertyChange(() => IncomeArticles);
            }
        }
        public List<Account> ExpenseArticles
        {
            get { return _expenseArticles; }
            set
            {
                if (Equals(value, _expenseArticles)) return;
                _expenseArticles = value;
                NotifyOfPropertyChange(() => ExpenseArticles);
            }
        }
        public List<Account> BankAccounts
        {
            get { return _bankAccounts; }
            set
            {
                if (Equals(value, _bankAccounts)) return;
                _bankAccounts = value;
                NotifyOfPropertyChange(() => BankAccounts);
            }
        }
        public List<Account> DebetAccounts { get; set; }
        public List<Account> CreditAccounts { get; set; }
        public List<Account> ArticleAccounts { get; set; }
        public void InitializeListsForCombobox(KeeperDb db, AccountTreeStraightener accountTreeStraightener)
        {
            CurrencyList = Enum.GetValues(typeof(CurrencyCodes)).OfType<CurrencyCodes>().ToList();
            MyAccounts = (accountTreeStraightener.Flatten(db.Accounts).
              Where(a => (a.IsLeaf("Мои") || a.Name == "Для ввода стартовых остатков"))).ToList();
            MyAccountsForShopping = (accountTreeStraightener.Flatten(db.Accounts).
              Where(a => a.IsLeaf("Мои") && !a.IsLeaf("Депозиты"))).ToList();
            BankAccounts = accountTreeStraightener.Flatten(db.Accounts).
              Where(a => a.IsLeaf("Банки") || a.Is("Мой кошелек")).ToList();
            AccountsWhoTakesMyMoney = (accountTreeStraightener.Flatten(db.Accounts).
              Where(a => a.IsLeaf("ДеньгоПолучатели") || a.IsLeaf("Банки") || a.IsLeaf("Государство"))).ToList();
            AccountsWhoGivesMeMoney = (accountTreeStraightener.Flatten(db.Accounts).
              Where(a => a.IsLeaf("ДеньгоДатели") || a.IsLeaf("Банки") || a.IsLeaf("Государство"))).ToList();
            IncomeArticles = (accountTreeStraightener.Flatten(db.Accounts).
              Where(a => a.IsLeaf("Все доходы"))).ToList();
            ExpenseArticles = (accountTreeStraightener.Flatten(db.Accounts).
              Where(a => a.IsLeaf("Все расходы"))).ToList();
            DebetAccounts = AccountsWhoGivesMeMoney;
            DebetAccounts.AddRange(MyAccounts);
            CreditAccounts = AccountsWhoTakesMyMoney;
            CreditAccounts.AddRange(MyAccounts);
            ArticleAccounts = IncomeArticles;
            ArticleAccounts.AddRange(ExpenseArticles);

            ItemsForDebit = db.Accounts.Where(account => account.Name != "ДеньгоПолучатели").ToList();
        }

        public bool FilterOnlyActiveAccounts;
        public void ChangeComboboxFilter(KeeperDb db, AccountTreeStraightener accountTreeStraightener)
        {
            FilterOnlyActiveAccounts = !FilterOnlyActiveAccounts;
            InitializeListsForCombobox(db, accountTreeStraightener);
        }
    }
}
