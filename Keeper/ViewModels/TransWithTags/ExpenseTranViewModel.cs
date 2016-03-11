﻿using System.Composition;
using System.Windows.Media;
using Caliburn.Micro;
using Keeper.DomainModel.DbTypes;
using Keeper.DomainModel.Transactions;
using Keeper.Controls;
using Keeper.Controls.AccNameSelectionControl;
using Keeper.Controls.TagPickingControl;
using Keeper.DomainModel.Enumes;
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

        public string MyAccountBalance { get {return _balanceDuringTransactionHinter.GetMyAccountBalance(TranInWork); } }
        public string AmountInUsd { get { return _balanceDuringTransactionHinter.GetAmountInUsd(TranInWork); } }

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
            ListsForComboTrees.InitializeLists(db);
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Расход";
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
            var alreadyChosenTagVm = new AlreadyChosenTagVm() {Tag = ListsForComboTrees.AccNamesForExpenseTags.FindThroughTheForest("Простор") };
            MyTagPickerVm.ListOfChosenTagsVm.Add(alreadyChosenTagVm);
            MyTagPickerVm.ListOfChosenTagsVm.Add(new AlreadyChosenTagVm() {Tag = ListsForComboTrees.AccNamesForExpenseTags.FindThroughTheForest("Мебель") });
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

        public void ButtonClose()
        {
            TryClose();
        }
    }
}