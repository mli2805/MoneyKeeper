using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Caliburn.Micro;
using Keeper.Controls.AccNameSelectionControl;
using Keeper.Controls.TagPickingControl;
using Keeper.DomainModel.DbTypes;
using Keeper.DomainModel.Transactions;
using Keeper.Utils.AccountEditing;
using Keeper.ViewModels.TransWithTags;

namespace Keeper.Controls
{
    [Export]
    class ExpenseControlVm : PropertyChangedBase
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
        public ExpenseControlVm(KeeperDb db, AccountTreeStraightener accountTreeStraightener, BalanceDuringTransactionHinter balanceDuringTransactionHinter,
                 MyAccNameSelectionControlInitializer myAccNameSelectionControlInitializer)
        {
            _db = db;
            _accountTreeStraightener = accountTreeStraightener;
            _myAccNameSelectionControlInitializer = myAccNameSelectionControlInitializer;
            _balanceDuringTransactionHinter = balanceDuringTransactionHinter;
            ListsForComboTrees.InitializeListsForExpense(_db);
        }

        public void SetTran(TranWithTags tran)
        {
            TranInWork = tran;
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

            MyDatePickerVm = new DatePickerWithTrianglesVm() { SelectedDate = TranInWork.Timestamp };
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
            switch (e.PropertyName)
            {
                case "MyAccount":
                    NotifyOfPropertyChange(nameof(MyAccountBalance));
                    break;
                case "Amount":
                case "Currency":
                    NotifyOfPropertyChange(nameof(AmountInUsd));
                    NotifyOfPropertyChange(nameof(MyAccountBalance));
                    break;
            }
        }
    }
}
