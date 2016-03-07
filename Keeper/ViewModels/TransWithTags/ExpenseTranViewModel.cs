using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using Caliburn.Micro;
using Keeper.DomainModel.DbTypes;
using Keeper.DomainModel.Enumes;
using Keeper.DomainModel.Transactions;
using Keeper.DomainModel.WorkTypes;

namespace Keeper.ViewModels.TransWithTags
{
    [Export]
    class ExpenseTranViewModel : Screen, IOneTranView
    {
        private readonly KeeperDb _db;
        private readonly BalanceDuringTransactionHinter _balanceDuringTransactionHinter;

        public List<CurrencyCodes> Currencies { get; set; } = Enum.GetValues(typeof(CurrencyCodes)).OfType<CurrencyCodes>().ToList();
        public TranWithTags TranInWork { get; set; }
        public string Result { get; set; }

        public AccName MyAccName { get; set; }
        public List<AccName> AccNamesForExpense { get; set; }
        public Dictionary<string,AccName> ButtonsDictionary { get; set; } = new Dictionary<string, AccName>();

        #region строковые хинты около полей ввода
        public string MyAccountBalance { get { return _balanceDuringTransactionHinter.GetMyAccountBalance(TranInWork); } }
        public string AmountInUsd { get { return _balanceDuringTransactionHinter.GetAmountInUsd(TranInWork); } } 
        #endregion

        [ImportingConstructor]
        public ExpenseTranViewModel(KeeperDb db, BalanceDuringTransactionHinter balanceDuringTransactionHinter)
        {
            _db = db;
            _balanceDuringTransactionHinter = balanceDuringTransactionHinter;
            ListsForComboTrees.InitializeLists(_db);
            AccNamesForExpense = ListsForComboTrees.MyAccNamesForIncome;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Expense transaction with tags";
        }

        public void SetTran(TranWithTags tran)
        {
            TranInWork = tran.Clone();
            TranInWork.PropertyChanged += TranInWork_PropertyChanged;
            MyAccName = ListsForComboTrees.FindThroughTheForest(AccNamesForExpense, TranInWork.MyAccount.Name)
                          ?? AccNamesForExpense.FirstOrDefault(an => Equals(an.Name, "Мой кошелек"));
        }

        private void TranInWork_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "MyAccount": NotifyOfPropertyChange(nameof(MyAccountBalance)); break;
                case "Amount":
                case "Currency":
                    NotifyOfPropertyChange(nameof(AmountInUsd));
                    NotifyOfPropertyChange(nameof(MyAccountBalance));
                    break;
            }
        }
        public void DecreaseTimestamp()
        {
            TranInWork.Timestamp = TranInWork.Timestamp.AddDays(-1);
        }

        public void Save()
        {
            Result = "Save";
            TryClose();
        }

        public void Cancel()
        {
            Result = "Cancel";
            TryClose();
        }
    }
}