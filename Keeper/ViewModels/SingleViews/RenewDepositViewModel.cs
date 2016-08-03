using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using Caliburn.Micro;
using Keeper.DomainModel.DbTypes;
using Keeper.DomainModel.Deposit;
using Keeper.DomainModel.Enumes;
using Keeper.DomainModel.Transactions;
using Keeper.DomainModel.WorkTypes;
using Keeper.Utils.AccountEditing;
using Keeper.Utils.BalanceEvaluating;
using Keeper.Utils.BalanceEvaluating.Ilya;

namespace Keeper.ViewModels.SingleViews
{
    [Export]
    class RenewDepositViewModel : Screen
    {
        private readonly KeeperDb _db;
        private readonly AccountBalanceCalculator _accountBalanceCalculator;
        readonly AccountTreeStraightener _accountTreeStraightener;
        private Deposit _oldDeposit;
        public Account NewDeposit { get; set; }

        public DateTime TransactionsDate { get; set; }
        public string OldDepositName { get; set; }
        public string DepositCurrency { get; set; }
        public Account BankAccount { get; set; }
        public decimal Procents { get; set; }
        public string NewDepositName { get; set; }

        public List<Account> BankAccounts { get; set; }

        [ImportingConstructor]
        public RenewDepositViewModel(KeeperDb db, AccountBalanceCalculator accountBalanceCalculator,
             AccountTreeStraightener accountTreeStraightener)
        {
            _db = db;
            _accountBalanceCalculator = accountBalanceCalculator;
            _accountTreeStraightener = accountTreeStraightener;

            BankAccounts = _accountTreeStraightener.Flatten(_db.Accounts).Where(a => a.Is("Банки") && a.Children.Count == 0).ToList();
        }

        public void SetOldDeposit(Deposit oldDeposit)
        {
            _oldDeposit = oldDeposit;

            TransactionsDate = _oldDeposit.FinishDate;
            OldDepositName = _oldDeposit.ParentAccount.Name;
            DepositCurrency = _oldDeposit.DepositOffer.Currency.ToString().ToLower();
            BankAccount = FindBankAccount();
            Procents = _oldDeposit.CalculationData.Estimations.ProcentsUpToFinish;
            NewDepositName = BuildNewName();
        }

        protected override void OnViewLoaded(object view)
        {
            BankAccounts = _accountTreeStraightener.Flatten(_db.Accounts).Where(a => a.Is("Банки") && a.Children.Count == 0).ToList();
            DisplayName = "Переоформление";
        }

        private Account FindBankAccount()
        {
            var st = OldDepositName.Substring(0, OldDepositName.IndexOf(' '));
            return _accountTreeStraightener.Seek(st, _db.Accounts);
        }

        private string BuildNewName()
        {
            var rate = _oldDeposit.DepositOffer.RateLines == null || _oldDeposit.DepositOffer.RateLines.LastOrDefault() == null
                   ? 0
                   : _oldDeposit.DepositOffer.RateLines.Last().Rate;


            var st = OldDepositName.Substring(0, OldDepositName.IndexOf('/') - 2).Trim();
            DateTime newFinish = _oldDeposit.FinishDate + (_oldDeposit.FinishDate - _oldDeposit.StartDate);
            string period = String.Format("{0:d/MM/yyyy} - {1:d/MM/yyyy}", _oldDeposit.FinishDate, newFinish).Replace('.', '/');
            return String.Format("{0} {1} {2}%", st, period, rate);
        }

        private Account AddNewAccountForDeposit()
        {
            var newDepositAccount = new Account(NewDepositName);
            newDepositAccount.Id = (from account in new AccountTreeStraightener().Flatten(_db.Accounts) select account.Id).Max() + 1;

            var parent = _accountTreeStraightener.Seek("Депозиты", _db.Accounts);
            newDepositAccount.Parent = parent;
            parent.Children.Add(newDepositAccount);

            return newDepositAccount;
        }

        private DateTime GetTimestampForTransactions()
        {
            var lastTransactionInDay =
               (from t in _db.Transactions where t.Timestamp.Date == TransactionsDate.Date select t).LastOrDefault();
            return lastTransactionInDay == null ?
              TransactionsDate.AddHours(9) :
              lastTransactionInDay.Timestamp.AddMinutes(1);
        }

        private void MakeTransactionProcents()
        {
            var transactionProcents = new Transaction
              {
                  Timestamp = GetTimestampForTransactions(),
                  Operation = OperationType.Доход,
                  Debet = BankAccount,
                  Credit = _oldDeposit.ParentAccount,
                  Amount = Procents,
                  Currency = _oldDeposit.DepositOffer.Currency,
                  Article = _accountTreeStraightener.Seek("Проценты по депозитам", _db.Accounts),
                  Comment = "причисление процентов при закрытии"
              };

            _db.Transactions.Add(transactionProcents);
        }

        private void MakeTransactionTransfer()
        {
            var period = new Period(new DateTime(0), GetTimestampForTransactions());
            MoneyBag moneyBag = _db.TransWithTags.Sum(t => t.MoneyBagForAccount(_oldDeposit.ParentAccount, period));
            var amount = moneyBag[_oldDeposit.DepositOffer.Currency];

            var transactionProcents = new Transaction
            {
                Timestamp = GetTimestampForTransactions(),
                Operation = OperationType.Перенос,
                Debet = _oldDeposit.ParentAccount,
                Credit = NewDeposit,

                Amount = amount,

//                Amount = _accountBalanceCalculator.GetAccountBalanceOnlyForCurrency(_oldDeposit.ParentAccount,
//                                    new Period(new DateTime(0), GetTimestampForTransactions()),
//                                    _oldDeposit.DepositOffer.Currency),


                Currency = _oldDeposit.DepositOffer.Currency,
                Comment = "переоформление вклада"
            };

            _db.Transactions.Add(transactionProcents);
        }

        private void RemoveOldAccountToClosed()
        {
            var parent = _accountTreeStraightener.Seek("Депозиты", _db.Accounts);
            parent.Children.Remove(_oldDeposit.ParentAccount);

            parent = _accountTreeStraightener.Seek("Закрытые депозиты", _db.Accounts);
            _oldDeposit.ParentAccount.Parent = parent;
            parent.Children.Add(_oldDeposit.ParentAccount);
        }

        public void Accept()
        {
            MakeTransactionProcents();
            NewDeposit = AddNewAccountForDeposit();
            MakeTransactionTransfer();
            RemoveOldAccountToClosed();
            TryClose();
        }

        public void Decline()
        {
        }

    }


}
