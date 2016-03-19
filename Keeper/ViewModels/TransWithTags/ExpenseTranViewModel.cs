using System.Collections.Generic;
using System.Composition;
using System.Windows.Media;
using Caliburn.Micro;
using Keeper.DomainModel.DbTypes;
using Keeper.DomainModel.Transactions;
using Keeper.Controls;
using Keeper.Controls.AccNameSelectionControl;
using Keeper.Controls.TagPickingControl;
using Keeper.Utils.AccountEditing;

namespace Keeper.ViewModels.TransWithTags
{
    [Export]
    class ExpenseTranViewModel : Screen, IOneTranView
    {
        private readonly KeeperDb _db;
        private readonly AccountTreeStraightener _accountTreeStraightener;
        private readonly MyAccNameSelectionControlInitializer _myAccNameSelectionControlInitializer;
        private readonly BalanceDuringTransactionHinter _balanceDuringTransactionHinter;
        public TranWithTags TranInWork { get; set; }
        public AccNameSelectorVm MyAccNameSelectorVm { get; set; }
        public AmountInputControlVm MyAmountInputControlVm { get; set; }
        public TagPickerVm MyTagPickerVm { get; set; }
        public DatePickerWithTrianglesVm MyDatePickerVm { get; set; }

        public string MyAccountBalance => _balanceDuringTransactionHinter.GetMyAccountBalance(TranInWork);
        public string AmountInUsd => _balanceDuringTransactionHinter.GetAmountInUsd(TranInWork);

        [ImportingConstructor]
        public ExpenseTranViewModel(KeeperDb db, AccountTreeStraightener accountTreeStraightener,
            MyAccNameSelectionControlInitializer myAccNameSelectionControlInitializer, BalanceDuringTransactionHinter balanceDuringTransactionHinter)
        {
            _db = db;
            _accountTreeStraightener = accountTreeStraightener;
            _myAccNameSelectionControlInitializer = myAccNameSelectionControlInitializer;
            _balanceDuringTransactionHinter = balanceDuringTransactionHinter;
            MyAccNameSelectorVm = new AccNameSelectorVm();
            MyAmountInputControlVm = new AmountInputControlVm();
            ListsForComboTrees.InitializeListsForExpense(db);
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Расход";
        }

        public TranWithTags GetTran()
        {
            return TranInWork;
        }

        public void SetTran(TranWithTags tran)
        {
            TranInWork = tran.Clone();
            TranInWork.PropertyChanged += TranInWork_PropertyChanged;

            MyAccNameSelectorVm = _myAccNameSelectionControlInitializer.ForExpense(TranInWork.MyAccount.Name);
            MyAccNameSelectorVm.PropertyChanged += MyAccNameSelectorVm_PropertyChanged;

            MyAmountInputControlVm = new AmountInputControlVm
            { LabelContent = "Сколько", AmountColor = Brushes.Red, Amount = TranInWork.Amount, Currency = TranInWork.Currency };
            MyAmountInputControlVm.PropertyChanged += MyAmountInputcControlVm_PropertyChanged;


            MyTagPickerVm = new TagPickerVm();
            foreach (var tag in tran.Tags)
            {
                var alreadyChosenTag = ListsForComboTrees.AccNamesForExpenseTags.FindThroughTheForest(tag.Name);
                if (alreadyChosenTag != null)
                    MyTagPickerVm.Tags.Add(alreadyChosenTag); 
            }
            MyTagPickerVm.TagSelectorVm = _myAccNameSelectionControlInitializer.ForExpenseTags("");
            MyTagPickerVm.Tags.CollectionChanged += Tags_CollectionChanged;

            MyDatePickerVm = new DatePickerWithTrianglesVm() {SelectedDate = TranInWork.Timestamp };
            MyDatePickerVm.PropertyChanged += MyDatePickerVm_PropertyChanged;
        }

        private void Tags_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            TranInWork.Tags = new List<Account>();
            foreach (var accName in MyTagPickerVm.Tags)
            {
                TranInWork.Tags.Add(_accountTreeStraightener.Seek(accName.Name, _db.Accounts));
            }
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

        private void MyAccNameSelectorVm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "MyAccName")
                TranInWork.MyAccount = _accountTreeStraightener.Seek(MyAccNameSelectorVm.MyAccName.Name, _db.Accounts);
        }


        private void TranInWork_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            NotifyOfPropertyChange(nameof(MyAccountBalance));
            NotifyOfPropertyChange(nameof(AmountInUsd));
        }

        public void Save()
        {
            TryClose(true);
        }

        public void Cancel()
        {
            TryClose(false);
        }
    }
}