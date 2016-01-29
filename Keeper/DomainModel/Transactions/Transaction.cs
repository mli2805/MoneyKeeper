using System;
using System.Windows.Media;
using Caliburn.Micro;
using Keeper.ByFunctional.BalanceEvaluating;

namespace Keeper.DomainModel.Transactions
{
    [Serializable]
    public class Transaction : PropertyChangedBase
    {
        private Guid _guid;
        private DateTime _timestamp;
        private OperationType _operation;
        private Account _debet;
        private Account _credit;
        private decimal _amount;
        private CurrencyCodes _currency;
        private Account _article;
        private string _comment;

        public int Id { get; set; }

        public Guid Guid
        {
            get { return _guid; }
            set
            {
                if (value.Equals(_guid)) return;
                _guid = value;
                NotifyOfPropertyChange(() => Guid);
            }
        }

        public DateTime Timestamp
        {
            get { return _timestamp; }
            set
            {
                if (value.Equals(_timestamp)) return;
                _timestamp = value;
                NotifyOfPropertyChange(() => Timestamp);
            }
        }
        public OperationType Operation
        {
            get { return _operation; }
            set
            {
                if (Equals(value, _operation)) return;
                _operation = value;
                NotifyOfPropertyChange(() => TransactionFontColor);
            }
        }
        public Account Debet
        {
            get { return _debet; }
            set
            {
                if (Equals(value, _debet)) return;
                _debet = value;
                NotifyOfPropertyChange(() => Debet);
            }
        }
        public Account Credit
        {
            get { return _credit; }
            set
            {
                if (Equals(value, _credit)) return;
                _credit = value;
                NotifyOfPropertyChange(() => Credit);
            }
        }
        public decimal Amount
        {
            get { return _amount; }
            set
            {
                if (value == _amount) return;
                _amount = value;
                NotifyOfPropertyChange(() => Amount);
            }
        }
        public CurrencyCodes Currency
        {
            get { return _currency; }
            set
            {
                if (Equals(value, _currency)) return;
                _currency = value;
                NotifyOfPropertyChange(() => Currency);
            }
        }
        public Account Article
        {
            get { return _article; }
            set
            {
                if (Equals(value, _article)) return;
                _article = value;
                NotifyOfPropertyChange(() => Article);
            }
        }
        public string Comment
        {
            get { return _comment; }
            set
            {
                if (value == _comment) return;
                _comment = value;
                NotifyOfPropertyChange(() => Comment);
            }
        }

        #region ' _isSelected '
        private bool _isSelected;

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (value.Equals(_isSelected)) return;
                _isSelected = value;
                NotifyOfPropertyChange(() => IsSelected);
                //        if (_isSelected) IoC.Get<TransactionViewModel>().SelectedTransaction = this;
            }
        }

        public void SetIsSelectedWithoutNotification(bool value)
        {
            _isSelected = value;
        }
        #endregion

        #region // два вычислимых поля содержащих цвет шрифта и фона для отображения транзакции
        public Brush DayBackgroundColor
        {
            get
            {
                var daysFrom = Timestamp.Date - new DateTime(1972, 5, 28);
                if (daysFrom.Days % 4 == 0) return Brushes.Cornsilk;
                if (daysFrom.Days % 4 == 1) return new SolidColorBrush(Color.FromRgb(240, 255, 240));
                if (daysFrom.Days % 4 == 2) return Brushes.GhostWhite;
                return Brushes.Azure;
            }
        }

        public Brush TransactionFontColor
        {
            get
            {
                if (Guid != Guid.Empty) return Brushes.DarkGreen;
                if (Operation == OperationType.Доход) return Brushes.Blue;
                if (Operation == OperationType.Расход) return Brushes.Red;
                if (Operation == OperationType.Перенос) return Brushes.Black;
                return Brushes.Gray;
            }
        }
        #endregion

        public bool IsExchange() { return Guid != Guid.Empty; }

        /// <summary>
        /// создает новый инстанс и в нем возвращает полную копию данного инстанса, кроме Id 
        /// </summary>
        /// <returns></returns>
        public Transaction Clone()
        {
            var cloneTransaction = new Transaction
                                     {
                                         Timestamp = Timestamp,
                                         Operation = Operation,
                                         Debet = Debet,
                                         Credit = Credit,
                                         Amount = Amount,
                                         Currency = Currency,
                                         Article = Article,
                                     };

            cloneTransaction.Comment = Comment ?? "";

            return cloneTransaction;
        }

        /// <summary>
        /// засасывает в данный инстанс все поля из инстанса-хранилища (кроме Id)
        /// </summary>
        /// <param name="storage"></param>
        public void CloneFrom(Transaction storage)
        {
            Timestamp = storage.Timestamp;
            Operation = storage.Operation;
            Debet = storage.Debet;
            Credit = storage.Credit;
            Amount = storage.Amount;
            Currency = storage.Currency;
            Article = storage.Article;
            Comment = storage.Comment ?? "";
            Guid = storage.Guid;
        }


        /// <summary>
        ///  возвращает заготовку созданную на основе этого инстанса, Amount и Comment оставляются пустыми
        /// </summary>
        /// <returns></returns>
        public Transaction Preform(string param)
        {
            var preformTransaction = new Transaction();

            if (param == "SameDate") preformTransaction.Timestamp = Timestamp;
            if (param == "SameDatePlusMinite") preformTransaction.Timestamp = Timestamp.AddMinutes(1);
            if (param == "NextDate") preformTransaction.Timestamp = Timestamp.Date.AddDays(1).AddHours(9);
            preformTransaction.Operation = Operation;
            preformTransaction.Debet = Debet;
            preformTransaction.Credit = Credit;
            preformTransaction.Currency = Currency;
            preformTransaction.Article = Article;

            return preformTransaction;
        }

        public int SignForAmount(Account a)
        {
            if (Debet.Is(a))
            {
                return Credit.Is(a) ? 0 : -1;
            }
            else
            {
                return Credit.Is(a) ? 1 : 0;
            }
//            return Credit.Is(a) ? 1 : -1;
        }

        public int SignForAnalysis(TransactionTypeForAnalysis flag)
        {
            if (flag == TransactionTypeForAnalysis.Депозитная) return Credit.IsDeposit() ? 1 : -1;
            if (flag == TransactionTypeForAnalysis.МояНоНеДепозитная) return Credit.IsMyNonDeposit() ? 1 : -1;
            return Credit.Is("Мои") ? 1 : -1;
        }

        private bool DebetXorCreditIsDeposit()
        {
            return Debet.IsDeposit() != Credit.IsDeposit();
        }

        private bool DebetXorCreditIsMyNonDeposit()
        {
            return Debet.IsMyNonDeposit() != Credit.IsMyNonDeposit();
        }

        private bool DebetXorCreditIsMy()
        {
            return Debet.Is("Мои") != Credit.Is("Мои");
        }

        public bool IsAcceptableForMonthAnalysisAs(TransactionTypeForAnalysis flag)
        {
            switch (flag)
            {
                case TransactionTypeForAnalysis.Депозитная:
                    return DebetXorCreditIsDeposit();
                case TransactionTypeForAnalysis.МояНоНеДепозитная:
                    return DebetXorCreditIsMyNonDeposit();
                case TransactionTypeForAnalysis.ЛюбаяМоя:
                    return DebetXorCreditIsMy();
                default:
                    return false;
            }
        }


    }


}