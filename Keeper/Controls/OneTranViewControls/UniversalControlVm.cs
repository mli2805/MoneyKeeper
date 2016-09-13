using System.Collections.Generic;
using System.Collections.Specialized;
using System.Composition;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using Keeper.Controls.OneTranViewControls.SubControls;
using Keeper.Controls.OneTranViewControls.SubControls.AccNameSelectionControl;
using Keeper.Controls.OneTranViewControls.SubControls.AmountInputControl;
using Keeper.Controls.OneTranViewControls.SubControls.TagPickingControl;
using Keeper.DomainModel.DbTypes;
using Keeper.DomainModel.Enumes;
using Keeper.DomainModel.Trans;
using Keeper.DomainModel.WorkTypes;
using Keeper.Utils.AccountEditing;
using Keeper.Utils.CommonKeeper;
using Keeper.ViewModels.TransWithTags;

namespace Keeper.Controls.OneTranViewControls
{
    [Export]
    class UniversalControlVm : PropertyChangedBase
    {
        private Visibility _visibility;
        public Visibility Visibility
        {
            get { return _visibility; }
            set
            {
                if (value == _visibility) return;
                _visibility = value;
                NotifyOfPropertyChange();
            }
        }

        private readonly KeeperDb _db;
        private readonly AccountTreeStraightener _accountTreeStraightener;
        private readonly AccNameSelectionControlInitializer _accNameSelectionControlInitializer;
        private readonly AssociationFinder _associationFinder;
        private readonly BalanceDuringTransactionHinter _balanceDuringTransactionHinter;

        private AmountInputControlVm _myAmountInputControlVm;
        private AccNameSelectorVm _mySecondAccNameSelectorVm;
        private AccNameSelectorVm _myAccNameSelectorVm;
        private AmountInputControlVm _myAmountInReturnInputControlVm;
        private TagPickerVm _myTagPickerVm;
        private DatePickerWithTrianglesVm _myDatePickerVm;

        public TranWithTags TranInWork { get; set; } = new TranWithTags();

        public AccNameSelectorVm MyAccNameSelectorVm
        {
            get { return _myAccNameSelectorVm; }
            set
            {
                if (Equals(value, _myAccNameSelectorVm)) return;
                _myAccNameSelectorVm = value;
                NotifyOfPropertyChange();
            }
        }
        public AccNameSelectorVm MySecondAccNameSelectorVm
        {
            get { return _mySecondAccNameSelectorVm; }
            set
            {
                if (Equals(value, _mySecondAccNameSelectorVm)) return;
                _mySecondAccNameSelectorVm = value;
                NotifyOfPropertyChange();
            }
        }
        public AmountInputControlVm MyAmountInputControlVm
        {
            get { return _myAmountInputControlVm; }
            set
            {
                if (Equals(value, _myAmountInputControlVm)) return;
                _myAmountInputControlVm = value;
                NotifyOfPropertyChange();
            }
        }
        public AmountInputControlVm MyAmountInReturnInputControlVm
        {
            get { return _myAmountInReturnInputControlVm; }
            set
            {
                if (Equals(value, _myAmountInReturnInputControlVm)) return;
                _myAmountInReturnInputControlVm = value;
                NotifyOfPropertyChange();
            }
        }
        public TagPickerVm MyTagPickerVm
        {
            get { return _myTagPickerVm; }
            set
            {
                if (Equals(value, _myTagPickerVm)) return;
                _myTagPickerVm = value;
                NotifyOfPropertyChange();
            }
        }
        public DatePickerWithTrianglesVm MyDatePickerVm
        {
            get { return _myDatePickerVm; }
            set
            {
                if (Equals(value, _myDatePickerVm)) return;
                _myDatePickerVm = value;
                NotifyOfPropertyChange();
            }
        }

        public string MyAccountBalance => _balanceDuringTransactionHinter.GetMyAccountBalance(TranInWork);
        public string MySecondAccountBalance => _balanceDuringTransactionHinter.GetMySecondAccountBalance(TranInWork);
        public string AmountInUsd => _balanceDuringTransactionHinter.GetAmountInUsd(TranInWork);
        public string AmountInReturnInUsd => _balanceDuringTransactionHinter.GetAmountInReturnInUsd(TranInWork);
        public string ExchangeRate => _balanceDuringTransactionHinter.GetExchangeRate(TranInWork);

        [ImportingConstructor]
        public UniversalControlVm(KeeperDb db, AccountTreeStraightener accountTreeStraightener, BalanceDuringTransactionHinter balanceDuringTransactionHinter,
                 AccNameSelectionControlInitializer accNameSelectionControlInitializer, AssociationFinder associationFinder)
        {
            _db = db;
            _accountTreeStraightener = accountTreeStraightener;
            _accNameSelectionControlInitializer = accNameSelectionControlInitializer;
            _associationFinder = associationFinder;
            _balanceDuringTransactionHinter = balanceDuringTransactionHinter;
            ListsForComboTrees.InitializeLists(_db);
        }

        public void SetTran(TranWithTags tran)
        {
            TranInWork = tran;
            TranInWork.PropertyChanged += TranInWork_PropertyChanged;

            MyAccNameSelectorVm = _accNameSelectionControlInitializer.ForMyAccount(TranInWork);
            MyAccNameSelectorVm.PropertyChanged += MyAccNameSelectorVm_PropertyChanged;

            MySecondAccNameSelectorVm = _accNameSelectionControlInitializer.ForMySecondAccount(TranInWork);
            MySecondAccNameSelectorVm.PropertyChanged += MySecondAccNameSelectorVm_PropertyChanged;

            MyAmountInputControlVm = new AmountInputControlVm
            {
                LabelContent = GetAmountActionLabel(TranInWork),
                AmountColor = TranInWork.Operation.FontColor(),
                Amount = TranInWork.Amount,
                Currency = TranInWork.Currency
            };
            MyAmountInputControlVm.PropertyChanged += MyAmountInputcControlVm_PropertyChanged;

            MyAmountInReturnInputControlVm = new AmountInputControlVm
            { LabelContent = "Получил", AmountColor = TranInWork.Operation.FontColor(), Amount = TranInWork.AmountInReturn, Currency = TranInWork.CurrencyInReturn };
            MyAmountInReturnInputControlVm.PropertyChanged += MyAmountInReturnInputControlVm_PropertyChanged;


            MyTagPickerVm = new TagPickerVm { TagSelectorVm = _accNameSelectionControlInitializer.ForTags(TranInWork) };
            foreach (var tag in tran.Tags)
            {
                var alreadyChosenTag = MyTagPickerVm.TagSelectorVm.AvailableAccNames.FindThroughTheForest(tag.Name);
                if (alreadyChosenTag != null)
                    MyTagPickerVm.Tags.Add(alreadyChosenTag);
            }
            MyTagPickerVm.Tags.CollectionChanged += Tags_CollectionChanged;

            MyDatePickerVm = new DatePickerWithTrianglesVm() { SelectedDate = TranInWork.Timestamp };
            MyDatePickerVm.PropertyChanged += MyDatePickerVm_PropertyChanged;
        }

        private string GetAmountActionLabel(TranWithTags tran)
        {
            switch (tran.Operation)
            {
                case OperationType.Доход: return "Получил";
                case OperationType.Расход: return "Заплатил";
                case OperationType.Перенос: return "Перенес";
                case OperationType.Обмен:
                default: return "Сдал";
            }
        }
        private void Tags_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            //            TranInWork.Tags = new List<Account>();
            //            foreach (var accName in MyTagPickerVm.Tags)
            //            {
            //                TranInWork.Tags.Add(_accountTreeStraightener.Seek(accName.Name, _db.Accounts));
            //            }

            if (e.Action == NotifyCollectionChangedAction.Remove) ReactOnRemove();
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                if (MyTagPickerVm.TagInWork != null)
                    ReactOnUsersAdd();
                else ReactOnAssociationAdd();
            }
        }

        private void ReactOnUsersAdd()
        {
            var tag = _accountTreeStraightener.Seek(MyTagPickerVm.TagInWork.Name, _db.Accounts);
            TranInWork.Tags.Add(tag);

            var associatedTag = _associationFinder.GetAssociation(TranInWork, tag);
            if (associatedTag != null)
            {
                TranInWork.Tags.Add(associatedTag);
                MyTagPickerVm.AssociatedTag = new AccName().PopulateFromAccount(associatedTag, null);
            }

            MyTagPickerVm.TagInWork = null;
        }

        private void ReactOnAssociationAdd()
        {
            var tag = _accountTreeStraightener.Seek(MyTagPickerVm.AssociatedTag.Name, _db.Accounts);
            TranInWork.Tags.Add(tag);

            MyTagPickerVm.AssociatedTag = null;
        }

        private void ReactOnRemove()
        {
            var tag = _accountTreeStraightener.Seek(MyTagPickerVm.TagInWork.Name, _db.Accounts);
            TranInWork.Tags.Remove(tag);
            MyTagPickerVm.TagInWork = null;
        }

        private void MyDatePickerVm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            TranInWork.Timestamp = MyDatePickerVm.SelectedDate;
        }

        private void MyAmountInputcControlVm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Amount") TranInWork.Amount = MyAmountInputControlVm.Amount;
            if (e.PropertyName == "Currency") TranInWork.Currency = MyAmountInputControlVm.Currency;
        }

        private void MyAmountInReturnInputControlVm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Amount") TranInWork.AmountInReturn = MyAmountInReturnInputControlVm.Amount;
            if (e.PropertyName == "Currency") TranInWork.CurrencyInReturn = MyAmountInReturnInputControlVm.Currency;
        }

        private void MyAccNameSelectorVm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "MyAccName")
            {
                TranInWork.MyAccount = _accountTreeStraightener.Seek(MyAccNameSelectorVm.MyAccName.Name, _db.Accounts);
                MyAmountInputControlVm.Currency =
                    _db.TransWithTags.LastOrDefault(t => t.MyAccount == TranInWork.MyAccount)?.Currency ?? CurrencyCodes.BYN;
            }
        }
        private void MySecondAccNameSelectorVm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "MyAccName")
            {
                TranInWork.MySecondAccount = _accountTreeStraightener.Seek(MySecondAccNameSelectorVm.MyAccName.Name, _db.Accounts);
                MyAmountInReturnInputControlVm.Currency =
                    _db.TransWithTags.LastOrDefault(t => t.MyAccount == TranInWork.MySecondAccount)?.Currency ?? CurrencyCodes.BYN;
            }
        }

        private void TranInWork_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "MyAccount":
                    NotifyOfPropertyChange(nameof(MyAccountBalance));
                    break;
                case "MySecondAccount":
                    NotifyOfPropertyChange(nameof(MySecondAccountBalance));
                    break;
                case "Operation":
                case "Amount":
                case "AmountInReturn":
                case "Currency":
                case "CurrencyInReturn":
                    NotifyOfPropertyChange(nameof(AmountInUsd));
                    NotifyOfPropertyChange(nameof(AmountInReturnInUsd));
                    NotifyOfPropertyChange(nameof(MyAccountBalance));
                    NotifyOfPropertyChange(nameof(MySecondAccountBalance));
                    NotifyOfPropertyChange(nameof(ExchangeRate));
                    break;
            }
        }

    }
}
