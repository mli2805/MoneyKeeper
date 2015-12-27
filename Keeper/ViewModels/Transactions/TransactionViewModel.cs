using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Composition;
using System.Linq;
using System.Windows.Data;
using Caliburn.Micro;
using Keeper.ByFunctional.AccountEditing;
using Keeper.ByFunctional.BalanceEvaluating;
using Keeper.DomainModel;
using Keeper.Utils.CommonKeeper;
using Keeper.Utils.Rates;
using Keeper.ViewModels.TransactionViewFilters;

namespace Keeper.ViewModels.Transactions
{
    [Export]
    [Shared] // для того чтобы в классе Transaction можно было обратиться к здешнему свойству SelectedTransaction
    public class TransactionViewModel : Screen
    {
        private readonly KeeperDb _db;

        private readonly RateExtractor _rateExtractor;
        private readonly BalancesForTransactionsCalculator _balancesForTransactionsCalculator;
        private readonly AccountTreeStraightener _accountTreeStraightener;
        private readonly TransactionChangesVisualizer _transactionChangesVisualizer;
        private readonly AssociationFinder _associationFinder;

        public static IWindowManager WindowManager { get { return IoC.Get<IWindowManager>(); } }
        public ObservableCollection<Transaction> Rows { get; set; }
        public bool IsCollectionChanged { get; set; }
        public ICollectionView SortedRows { get; set; }

        #region  фильтрация и переход к дате
        public List<AccountFilter> DebetFilterList { get; set; }
        public List<AccountFilter> CreditFilterList { get; set; }
        public List<AccountFilter> ArticleFilterList { get; set; }

        private OperationTypesFilter _selectedOperationTypeFilter;
        private AccountFilter _selectedDebetFilter;
        private AccountFilter _selectedCreditFilter;
        private AccountFilter _selectedArticleFilter;
        private string _commentFilter;
        private DateTime _dateToGo;

        private void InitializeFiltersLists()
        {
            DebetFilterList = new List<AccountFilter>();

            // <no filter>
            var filter = new AccountFilter();
            DebetFilterList.Add(filter);

            var debetAccounts = (_accountTreeStraightener.Flatten(_db.Accounts).Where(account =>
                    (account.Is("Мои") || account.Is("Внешние")) && account.Children.Count == 0)).ToList();
            foreach (var account in debetAccounts)
            {
                filter = new AccountFilter(account);
                DebetFilterList.Add(filter);
            }

            CreditFilterList = DebetFilterList;

            ArticleFilterList = new List<AccountFilter>();
            // <no filter>
            filter = new AccountFilter();
            ArticleFilterList.Add(filter);

            var articleAccounts = (_accountTreeStraightener.Flatten(_db.Accounts).Where(account =>
                    (account.Is("Все доходы") || account.Is("Все расходы")) && account.Children.Count == 0)).ToList();
            foreach (var account in articleAccounts)
            {
                filter = new AccountFilter(account);
                ArticleFilterList.Add(filter);
            }
        }

        public void ClearAllFilters()
        {
            SelectedOperationTypeFilter = OperationTypesFilerListForCombo.FilterList.First(f => !f.IsOn);
            SelectedDebetFilter = DebetFilterList.First(f => !f.IsOn);
            SelectedCreditFilter = CreditFilterList.First(f => !f.IsOn);
            SelectedArticleFilter = ArticleFilterList.First(f => !f.IsOn);
            CommentFilter = "";
        }

        private bool OnFilter(object o)
        {
            var transaction = (Transaction)o;
            if (SelectedOperationTypeFilter.IsOn && transaction.Operation != SelectedOperationTypeFilter.Operation) return false;
            if (SelectedDebetFilter.IsOn && transaction.Debet != SelectedDebetFilter.WantedAccount) return false;
            if (SelectedCreditFilter.IsOn && transaction.Credit != SelectedCreditFilter.WantedAccount) return false;
            if (SelectedArticleFilter.IsOn && transaction.Article != SelectedArticleFilter.WantedAccount) return false;
            if (CommentFilter != "" && transaction.Comment.IndexOf(CommentFilter, StringComparison.Ordinal) == -1) return false;

            return true;
        }

        public DateTime DateToGo
        {
            get { return _dateToGo; }
            set
            {
                if (value.Equals(_dateToGo)) return;
                _dateToGo = value;
                NotifyOfPropertyChange(() => DateToGo);
                SelectedTransaction = (from transaction in _db.Transactions
                                       where transaction.Timestamp > DateToGo
                                       select transaction).FirstOrDefault() ?? Rows.Last();
            }
        }

        public OperationTypesFilter SelectedOperationTypeFilter
        {
            get { return _selectedOperationTypeFilter; }
            set
            {
                if (Equals(value, _selectedOperationTypeFilter)) return;
                _selectedOperationTypeFilter = value;
                NotifyOfPropertyChange(() => SelectedOperationTypeFilter);
                var view = CollectionViewSource.GetDefaultView(SortedRows);
                view.Refresh();
            }
        }

        public AccountFilter SelectedDebetFilter
        {
            get { return _selectedDebetFilter; }
            set
            {
                if (Equals(value, _selectedDebetFilter)) return;
                _selectedDebetFilter = value;
                NotifyOfPropertyChange(() => SelectedDebetFilter);
                var view = CollectionViewSource.GetDefaultView(SortedRows);
                view.Refresh();
            }
        }

        public AccountFilter SelectedCreditFilter
        {
            get { return _selectedCreditFilter; }
            set
            {
                if (Equals(value, _selectedCreditFilter)) return;
                _selectedCreditFilter = value;
                NotifyOfPropertyChange(() => SelectedCreditFilter);
                var view = CollectionViewSource.GetDefaultView(SortedRows);
                view.Refresh();
            }
        }

        public AccountFilter SelectedArticleFilter
        {
            get { return _selectedArticleFilter; }
            set
            {
                if (Equals(value, _selectedArticleFilter)) return;
                _selectedArticleFilter = value;
                NotifyOfPropertyChange(() => SelectedArticleFilter);
                var view = CollectionViewSource.GetDefaultView(SortedRows);
                view.Refresh();
            }
        }

        public string CommentFilter
        {
            get { return _commentFilter; }
            set
            {
                if (value == _commentFilter) return;
                _commentFilter = value;
                NotifyOfPropertyChange(() => CommentFilter);
                var view = CollectionViewSource.GetDefaultView(SortedRows);
                view.Refresh();
            }
        }
        #endregion

        #region Списки для комбобоксов и их инициализация
        public ListsForComboboxes ListsForComboboxes { get; set; }
        public void ChangeComboboxFilter() { ListsForComboboxes.ChangeComboboxFilter(_db, _accountTreeStraightener);}
        #endregion

        #region группа свойств для биндинга селектов и др.

        private bool _isInTransactionSelectionProcess;
        private int _selectedTabIndex;
        private Transaction _selectedTransaction;
        private int _selectedTransactionIndex;
        private bool _isTransactionInWorkChanged;
        private Transaction _transactionInWork;
        private bool _isInAddTransactionMode;

        public int SelectedTabIndex
        {
            get { return _selectedTabIndex; }
            set
            {
                if (value == _selectedTabIndex) return;
                _selectedTabIndex = value;
                NotifyOfPropertyChange(() => SelectedTabIndex);
                AccommodateAccountsWithOperationType(SelectedTabIndex);
            }
        }

        private void AccommodateAccountsWithOperationType(int newTab)
        {
            if (newTab != 3) TransactionInWork.Operation = (OperationType)newTab; else TransactionInWork.Operation = OperationType.Расход;
            switch (newTab)
            {
                case 0: //Доход
                    if (!ListsForComboboxes.AccountsWhoGivesMeMoney.Contains(TransactionInWork.Debet))
                    {
                        TransactionInWork.Debet = ListsForComboboxes.AccountsWhoGivesMeMoney.First();
                        TransactionInWork.Article = _associationFinder.GetAssociation(TransactionInWork.Debet);
                    }
                    if (!ListsForComboboxes.MyAccounts.Contains(TransactionInWork.Credit))
                        TransactionInWork.Credit = ListsForComboboxes.MyAccounts.First();
                    break;
                case 1: //Расход
                    if (!ListsForComboboxes.AccountsWhoTakesMyMoney.Contains(TransactionInWork.Credit))
                    {
                        TransactionInWork.Credit = ListsForComboboxes.AccountsWhoTakesMyMoney.First();
                        TransactionInWork.Article = _associationFinder.GetAssociation(TransactionInWork.Credit);
                    }
                    if (!ListsForComboboxes.MyAccountsForShopping.Contains(TransactionInWork.Debet))
                        TransactionInWork.Debet = ListsForComboboxes.MyAccountsForShopping.First();
                    break;
                case 2: //Перенос
                    if (!ListsForComboboxes.MyAccounts.Contains(TransactionInWork.Debet))
                        TransactionInWork.Debet = ListsForComboboxes.MyAccounts.First();
                    if (!ListsForComboboxes.MyAccounts.Contains(TransactionInWork.Credit))
                        TransactionInWork.Credit = ListsForComboboxes.MyAccounts.First();
                    break;
                case 3: //Обмен
                    if (!ListsForComboboxes.MyAccounts.Contains(TransactionInWork.Debet))
                        TransactionInWork.Debet = ListsForComboboxes.MyAccounts.First();

                    if (RelatedTransactionInWork.Credit == null) RelatedTransactionInWork.Credit = ListsForComboboxes.MyAccounts.First();

                    var bank = _associationFinder.GetBank(RelatedTransactionInWork.Credit) ??
                               _associationFinder.GetBank(TransactionInWork.Debet);
                    if (bank != null) TransactionInWork.Credit = bank;

                    if (!ListsForComboboxes.CurrencyList.Contains(TransactionInWork.Currency))
                    {
                        TransactionInWork.Currency = CurrencyCodes.BYR;
                        RelatedTransactionInWork.Currency = CurrencyCodes.USD;
                    }
                    if (IsInAddTransactionMode) RelatedTransactionInWork.Amount = 0;
                    break;
            }
        }

        public Transaction SelectedTransaction
        {
            set
            {
                if (_selectedTransaction != null) _selectedTransaction.SetIsSelectedWithoutNotification(false);

                _selectedTransaction = value ?? Rows.Last();
                _selectedTransaction.IsSelected = true;

                _isInTransactionSelectionProcess = true;

                if (_selectedTransaction.IsExchange())
                {
                    RelatedTransaction =
                        (from t in Rows where t.Guid == _selectedTransaction.Guid && t != _selectedTransaction select t)
                            .FirstOrDefault();

                    if (_selectedTransaction.Operation == OperationType.Расход)
                    {
                        TransactionInWork.CloneFrom(_selectedTransaction);
                        RelatedTransactionInWork.CloneFrom(RelatedTransaction);
                    }
                    else
                    {
                        TransactionInWork.CloneFrom(RelatedTransaction);
                        RelatedTransactionInWork.CloneFrom(_selectedTransaction);
                    }
                }
                else
                {
                    TransactionInWork.CloneFrom(_selectedTransaction);
                    RelatedTransaction = new Transaction();
                }

                SelectedTabIndex = GetTabIndexFromTransaction(_transactionInWork);
                _isInTransactionSelectionProcess = false;
                NotifyOfPropertyChange(() => AmountInUsd);
                NotifyOfPropertyChange(() => DebetAccountBalance);
                NotifyOfPropertyChange(() => CreditAccountBalance);
                NotifyOfPropertyChange(() => ExchangeRate);
                DayResults = _balancesForTransactionsCalculator.CalculateDayResults(SelectedTransaction.Timestamp);
                EndDayBalances = _balancesForTransactionsCalculator.EndDayBalances(SelectedTransaction.Timestamp);
            }
            get { return _selectedTransaction; }
        }

        public int SelectedTransactionIndex
        {
            get { return _selectedTransactionIndex; }
            set
            {
                if (value == _selectedTransactionIndex) return;
                _selectedTransactionIndex = value;
                NotifyOfPropertyChange(() => SelectedTransactionIndex);
            }
        }

        public bool IsTransactionInWorkChanged
        {
            get { return _isTransactionInWorkChanged; }
            set
            {
                if (value.Equals(_isTransactionInWorkChanged)) return;
                _isTransactionInWorkChanged = value;
                CanSaveTransactionChanges = value;
                CanCancelTransactionChanges = value;
                NotifyOfPropertyChange(() => CanSaveTransactionChanges);
                NotifyOfPropertyChange(() => CanCancelTransactionChanges);
            }
        }

        public Transaction TransactionInWork
        {
            get { return _transactionInWork; }
            set
            {
                if (Equals(value, _transactionInWork)) return;
                _transactionInWork = value;
                SelectedTabIndex = GetTabIndexFromTransaction(_transactionInWork);
                NotifyOfPropertyChange(() => TransactionInWork);
            }
        }

        public Transaction RelatedTransaction { get; set; }

        public Transaction RelatedTransactionInWork
        {
            get { return _relatedTransactionInWork; }
            set
            {
                if (Equals(value, _relatedTransactionInWork)) return;
                _relatedTransactionInWork = value;
                NotifyOfPropertyChange(() => RelatedTransactionInWork);
            }
        }

        public bool IsInAddTransactionMode
        {
            get { return _isInAddTransactionMode; }
            set
            {
                _isInAddTransactionMode = value;
                if (!value) CanEditDate = true;
            }
        }
        #endregion


        #region свойства Can для нескольких кнопок
        private bool _canEditDate;
        public bool CanSaveTransactionChanges { get; set; }
        public bool CanCancelTransactionChanges { get; set; }
        public bool CanEditDate
        {
            get { return _canEditDate; }
            set
            {
                if (value.Equals(_canEditDate)) return;
                _canEditDate = value;
                NotifyOfPropertyChange(() => CanEditDate);
            }
        }

        private bool _canFillInReceipt;
        public bool CanFillInReceipt
        {
            get { return _canFillInReceipt; }
            set
            {
                if (value.Equals(_canFillInReceipt)) return;
                _canFillInReceipt = value;
                NotifyOfPropertyChange(() => CanFillInReceipt);
            }
        }

        #endregion

        #region свойства для показа сумм остатков на счетах и перевода операции в доллары
        public string AmountInUsd
        {
            get
            {
                // одинарные операции не долларах
                if (TransactionInWork.Currency == CurrencyCodes.USD && SelectedTabIndex != 3) return "";
                const string res0 = "                                                                                ";

                var res1 = _rateExtractor.GetUsdEquivalentString(TransactionInWork.Amount, TransactionInWork.Currency, TransactionInWork.Timestamp);
                // одинарные операции не в остальных валютах
                if (SelectedTabIndex != 3) return res0 + res1;

                res1 = TransactionInWork.Currency == CurrencyCodes.USD ? "                                           " :
                    _rateExtractor.GetUsdEquivalentString(TransactionInWork.Amount, TransactionInWork.Currency, TransactionInWork.Timestamp);
                var res2 = RelatedTransactionInWork.Currency == CurrencyCodes.USD ? "" :
                    _rateExtractor.GetUsdEquivalentString(RelatedTransactionInWork.Amount, RelatedTransactionInWork.Currency, RelatedTransactionInWork.Timestamp);

                return res1 + "                                      " + res2;
            }
        }

        public string DebetAccountBalance
        {
            get
            {
                return _transactionChangesVisualizer.GetDebetAccountBalance(TransactionInWork);
            }
        }

        public string CreditAccountBalance
        {
            get { return _transactionChangesVisualizer.GetCreditAccountBalance(SelectedTabIndex, TransactionInWork, RelatedTransactionInWork); }
        }

        public string ExchangeRate
        {
            get
            {
                return SelectedTabIndex == 3 ?
                RateDefiner.GetExpression(TransactionInWork.Currency, TransactionInWork.Amount,
                RelatedTransactionInWork.Currency, RelatedTransactionInWork.Amount) : "";
            }
        }

        private List<string> _dayResults;
        public List<string> DayResults
        {
            get { return _dayResults; }
            set
            {
                if (Equals(value, _dayResults)) return;
                _dayResults = value;
                NotifyOfPropertyChange(() => DayResults);
            }
        }

        private string _endDayBalances;
        private Transaction _relatedTransactionInWork;

        public string EndDayBalances
        {
            get { return _endDayBalances; }
            set
            {
                if (value == _endDayBalances) return;
                _endDayBalances = value;
                NotifyOfPropertyChange(() => EndDayBalances);
            }
        }

        #endregion

        [ImportingConstructor]
        public TransactionViewModel(KeeperDb db, RateExtractor rateExtractor, AccountBalanceCalculator accountBalanceCalculator,
          BalancesForTransactionsCalculator balancesForTransactionsCalculator, AccountTreeStraightener accountTreeStraightener, TransactionChangesVisualizer transactionChangesVisualizer)
        {
            _db = db;

            _rateExtractor = rateExtractor;
            _balancesForTransactionsCalculator = balancesForTransactionsCalculator;
            _accountTreeStraightener = accountTreeStraightener;
            _transactionChangesVisualizer = transactionChangesVisualizer;
            _associationFinder = new AssociationFinder(_db);
            ListsForComboboxes = new ListsForComboboxes();
            ListsForComboboxes.FilterOnlyActiveAccounts = true;
            ListsForComboboxes.InitializeListsForCombobox(_db, accountTreeStraightener);
            TransactionInWork = new Transaction();
            RelatedTransactionInWork = new Transaction();
            Rows = _db.Transactions;
            Rows.CollectionChanged += RowsCollectionChanged;
            IsCollectionChanged = false;
            InitializeFiltersLists();
            InitializeSelectedTransactionIndex();

        }

        void RowsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            IsCollectionChanged = true;
        }

        private void InitializeSelectedTransactionIndex()
        {
            foreach (var transaction in _db.Transactions)
            {
                if (transaction.IsSelected)
                {
                    SelectedTransactionIndex = Rows.IndexOf(transaction);
                    return;
                }
            }
            SelectedTransactionIndex = Rows.Count - 1;
            Rows.Last().IsSelected = true;
        }

        /// <summary>
        /// Когда форма загружена, подписываемся на событие "Изменено свойство в инстансе TransactionInWork класса Transaction
        /// Такая возможность существует т.к. класс Transaction отнаследован от PropertyChangedBase
        /// Однако чтобы такое событие происходило, надо чтобы хотя бы одно свойство класса Transaction было нотифицирующим
        /// </summary>
        /// <param name="view"></param>
        protected override void OnViewLoaded(object view)
        {
//            InitializeListsForCombobox();
            DisplayName = "Ежедневные операции";

            SortedRows = CollectionViewSource.GetDefaultView(Rows);
            SortedRows.SortDescriptions.Add(new SortDescription("Timestamp", ListSortDirection.Ascending));
            ClearAllFilters();
            SortedRows.Filter += OnFilter;

            TransactionInWork.PropertyChanged += TransactionInWorkPropertyChanged;
            RelatedTransactionInWork.PropertyChanged += RelatedTransactionInWorkPropertyChanged;

            IsTransactionInWorkChanged = false;
            CanSaveTransactionChanges = false;
            CanCancelTransactionChanges = false;
            CanFillInReceipt = false;
            IsInAddTransactionMode = false;
        }

        void RelatedTransactionInWorkPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (_isInTransactionSelectionProcess) return;
            IsTransactionInWorkChanged = true;

            if (e.PropertyName == "Credit" && IsInAddTransactionMode && SelectedTabIndex == 3)
            {
                RelatedTransactionInWork.Currency = _associationFinder.GetAccountLastCurrency(RelatedTransactionInWork.Credit);
                var bank = _associationFinder.GetBank(RelatedTransactionInWork.Credit) ??
                           _associationFinder.GetBank(TransactionInWork.Debet);
                if (bank != null) TransactionInWork.Credit = bank;
            }

            if (e.PropertyName == "Amount" || e.PropertyName == "Currency")
            {
                NotifyOfPropertyChange(() => AmountInUsd);
                DayResults = _balancesForTransactionsCalculator.CalculateDayResults(SelectedTransaction.Timestamp);
            }

            NotifyOfPropertyChange(() => CreditAccountBalance);
            NotifyOfPropertyChange(() => ExchangeRate);
        }

        void TransactionInWorkPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (_isInTransactionSelectionProcess) return;
            IsTransactionInWorkChanged = true;

            if (e.PropertyName == "Comment") return;

            if (e.PropertyName == "Debet" && IsInAddTransactionMode)
            {
                if (SelectedTabIndex == 3)
                {
                    TransactionInWork.Currency = _associationFinder.GetAccountLastCurrency(TransactionInWork.Debet);
                    var bank = _associationFinder.GetBank(RelatedTransactionInWork.Credit) ??
                               _associationFinder.GetBank(TransactionInWork.Debet);
                    if (bank != null) TransactionInWork.Credit = bank;
                }
                else if (TransactionInWork.Operation == OperationType.Доход)
                    TransactionInWork.Article = _associationFinder.GetAssociation(TransactionInWork.Debet);
            }

            if (e.PropertyName == "Credit" && IsInAddTransactionMode && TransactionInWork.Operation == OperationType.Расход)
                TransactionInWork.Article = _associationFinder.GetAssociation(TransactionInWork.Credit);

            if (e.PropertyName == "Credit" && TransactionInWork.IsExchange())
                RelatedTransactionInWork.Debet = TransactionInWork.Credit;

            if (e.PropertyName == "Amount" || e.PropertyName == "Currency")
            {
                NotifyOfPropertyChange(() => AmountInUsd);
                DayResults = _balancesForTransactionsCalculator.CalculateDayResults(SelectedTransaction.Timestamp);
            }

            NotifyOfPropertyChange(() => DebetAccountBalance);
            NotifyOfPropertyChange(() => CreditAccountBalance);
            NotifyOfPropertyChange(() => ExchangeRate);
        }

        protected override void OnDeactivate(bool close)
        {
            if (IsInAddTransactionMode) DeleteTransaction();
            base.OnDeactivate(close);
        }

        public void MoveTransactionUp()
        {
            MoveTransaction(-1);
        }

        public void MoveTransactionDown()
        {
            MoveTransaction(1);
        }

        private void MoveTransaction(int direction) // -1 - up;   +1 - down
        {
            var currentTransaction = SelectedTransaction;
            SelectedTransactionIndex += direction;
            var nearbyTransaction = SelectedTransaction;

            if (nearbyTransaction.IsExchange())
            {
                Transaction relatedTransaction;
                if (direction == 1)
                {
                    SelectedTransactionIndex += direction;
                    relatedTransaction = SelectedTransaction;
                }
                else
                {
                    relatedTransaction = nearbyTransaction;
                    SelectedTransactionIndex += direction;
                    nearbyTransaction = SelectedTransaction;
                }

                nearbyTransaction.Timestamp = currentTransaction.Timestamp;
                relatedTransaction.Timestamp = nearbyTransaction.Timestamp.AddSeconds(10);
                currentTransaction.Timestamp = nearbyTransaction.Timestamp.AddMinutes(direction);
            }
            else
            {
                nearbyTransaction.Timestamp = currentTransaction.Timestamp;
                currentTransaction.Timestamp = nearbyTransaction.Timestamp.AddMinutes(direction);
            }

            SortedRows.Refresh();
            SelectedTransactionIndex += direction;
        }

        public void SaveTransactionChanges()
        {
            //     из-за смены типа операции во время ввода/редактирования в некоторых полях TransactionInWork могли остаться ненужные данные
            if (TransactionInWork.IsExchange() || TransactionInWork.Operation == OperationType.Перенос)
                TransactionInWork.Article = null;

            var isDateChanged = SelectedTransaction.Timestamp.Date != TransactionInWork.Timestamp.Date;

            if (isDateChanged)
            {
                var transactionBefore = _db.Transactions.Where(
                    transaction => transaction.Timestamp.Date == TransactionInWork.Timestamp.Date).OrderBy(
                        transaction => transaction.Timestamp).LastOrDefault();

                SelectedTransaction.CloneFrom(TransactionInWork);
                SelectedTransaction.Timestamp = transactionBefore == null
                    ? SelectedTransaction.Timestamp.AddHours(9)
                    : transactionBefore.Timestamp.AddMinutes(1);
                SortedRows.Refresh();
            }
            else
            {
                if (SelectedTabIndex != 3)
                    SelectedTransaction.CloneFrom(TransactionInWork);
                else
                {

                    RelatedTransactionInWork.Debet = TransactionInWork.Credit;
                    if (TransactionInWork.Guid == Guid.Empty) // новая операция а не редактирование старой
                    {
                        TransactionInWork.Guid = Guid.NewGuid();
                        RelatedTransactionInWork.Guid = TransactionInWork.Guid;
                        RelatedTransactionInWork.Timestamp = TransactionInWork.Timestamp.AddSeconds(10);
                        Rows.Add(RelatedTransaction);
                    }

                    if (RelatedTransaction.Operation == OperationType.Доход)
                    {
                        SelectedTransaction.CloneFrom(TransactionInWork);
                        RelatedTransaction.CloneFrom(RelatedTransactionInWork);
                    }
                    else
                    {
                        SelectedTransaction.CloneFrom(RelatedTransactionInWork);
                        RelatedTransaction.CloneFrom(TransactionInWork);
                    }
                    SortedRows.Refresh();
                    SelectedTransactionIndex++;
                }
            }

            DayResults = _balancesForTransactionsCalculator.CalculateDayResults(SelectedTransaction.Timestamp);
            EndDayBalances = _balancesForTransactionsCalculator.EndDayBalances(SelectedTransaction.Timestamp);
            IsTransactionInWorkChanged = false;
            IsInAddTransactionMode = false;
            CanFillInReceipt = false;
            IsCollectionChanged = true;
        }

        public void EscapeButtonPressed()
        {
            CancelTransactionChanges();
        }

        private int GetTabIndexFromTransaction(Transaction tr)
        {
            return tr.IsExchange() ? 3 : (int)tr.Operation;
        }
        public void CancelTransactionChanges()
        {
            if (IsInAddTransactionMode) DeleteTransaction();

            RelatedTransactionInWork.CloneFrom(RelatedTransaction);
            TransactionInWork.CloneFrom(SelectedTransaction);
            SelectedTabIndex = GetTabIndexFromTransaction(TransactionInWork);
            IsTransactionInWorkChanged = false;
            IsInAddTransactionMode = false;
            CanFillInReceipt = false;
        }

        public void AddTransactionBeforeSelected()
        {
            if (CanSaveTransactionChanges) SaveTransactionChanges();
            IsInAddTransactionMode = true;
            CanEditDate = false;

            var newTransaction = SelectedTransaction.Preform("SameDate");
            //            IncreaseNextTransactionTime();
            AnotherWayToShiftTimeInNextTransactions();

            Rows.Add(newTransaction);
            SelectedTransactionIndex--;
            IsTransactionInWorkChanged = true;
            CanFillInReceipt = true;
        }

        private void AnotherWayToShiftTimeInNextTransactions()
        {
            var tr = from t in Rows
                     where t.Timestamp.Date == SelectedTransaction.Timestamp.Date
                           && t.Timestamp.TimeOfDay >= SelectedTransaction.Timestamp.TimeOfDay
                     select t;

            foreach (var t in tr)
            {
                t.Timestamp = t.Timestamp.AddMinutes(1);
            }
        }

        /// <summary>
        /// Функция, которая обязательно увеличит Timestamp нынешней выделенной транзакции
        /// а если надо, то и последующих 
        /// </summary>
        private void IncreaseNextTransactionTime()
        {
            var workTransaction = SelectedTransaction;
            SelectedTransactionIndex++;
            if (workTransaction.Timestamp.AddMinutes(1) == SelectedTransaction.Timestamp) IncreaseNextTransactionTime();
            SelectedTransactionIndex--;
            SelectedTransaction.Timestamp = SelectedTransaction.Timestamp.AddMinutes(1);
        }

        public void AddTransactionAfterSelected()
        {
            if (CanSaveTransactionChanges) SaveTransactionChanges();
            IsInAddTransactionMode = true;
            CanEditDate = false;

            var newTransaction = SelectedTransaction.Preform("SameDatePlusMinite");

            if (SelectedTransactionIndex != Rows.Count - 1)
            {
                SelectedTransactionIndex++;
                if (newTransaction.Timestamp == SelectedTransaction.Timestamp)
                    //                    IncreaseNextTransactionTime();
                    AnotherWayToShiftTimeInNextTransactions();
                SelectedTransactionIndex--;
            }

            Rows.Add(newTransaction);
            SelectedTransactionIndex++;
            IsTransactionInWorkChanged = true;
            CanFillInReceipt = true;
        }

        public void AddNewDayTransaction()
        {
            if (CanSaveTransactionChanges) SaveTransactionChanges();
            IsInAddTransactionMode = true;

            SelectedTransactionIndex = Rows.Count - 1;
            var newTransaction = SelectedTransaction.Preform("NextDate");

            Rows.Add(newTransaction);
            SelectedTransactionIndex++;
            IsTransactionInWorkChanged = true;
            CanFillInReceipt = true;
        }

        public void DeleteTransaction()
        {
            var transactionForRemoval = SelectedTransaction;
            var guid = SelectedTransaction.Guid;

            if (SelectedTransactionIndex == 0) SelectedTransactionIndex++;
            else
                if (guid == Guid.Empty) SelectedTransactionIndex--;
                else
                {
                    if (SelectedTransaction.Operation == OperationType.Расход) SelectedTransactionIndex--;
                    else
                    {
                        SelectedTransactionIndex -= 2;
                    }
                }

            Rows.Remove(transactionForRemoval);
            if (guid != Guid.Empty)
            {
                transactionForRemoval = Rows.First(t => t.Guid == guid);
                Rows.Remove(transactionForRemoval);
            }
            if (SelectedTransactionIndex == -1) SelectedTransactionIndex = Rows.Count - 1;

            DayResults = _balancesForTransactionsCalculator.CalculateDayResults(SelectedTransaction.Timestamp);
            EndDayBalances = _balancesForTransactionsCalculator.EndDayBalances(SelectedTransaction.Timestamp);
            IsInAddTransactionMode = false;
        }

        public void IncreaseTimestamp()
        {
            TransactionInWork.Timestamp = TransactionInWork.Timestamp.AddDays(1);
        }

        public void DecreaseTimestamp()
        {
            TransactionInWork.Timestamp = TransactionInWork.Timestamp.AddDays(-1);
        }

        public void FillInReceipt()
        {
            var receiptViewModel = new ReceiptViewModel(TransactionInWork.Timestamp, TransactionInWork.Credit.Name,
                                           TransactionInWork.Currency, TransactionInWork.Amount, TransactionInWork.Article, ListsForComboboxes.ExpenseArticles);
            WindowManager.ShowDialog(receiptViewModel);
            if (receiptViewModel.Result) // добавить транзакции
            {
                foreach (var tuple in receiptViewModel.Expense)
                {
                    TransactionInWork.Amount = tuple.Item1;
                    TransactionInWork.Article = tuple.Item2;
                    TransactionInWork.Comment = tuple.Item3;
                    SaveTransactionChanges();
                    AddTransactionAfterSelected();
                }
                CancelTransactionChanges();
            }
        }

    }
}

