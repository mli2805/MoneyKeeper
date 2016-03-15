using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using Caliburn.Micro;
using Keeper.DomainModel.DbTypes;
using Keeper.DomainModel.Enumes;
using Keeper.DomainModel.Transactions;
using Keeper.DomainModel.WorkTypes;
using Keeper.Utils.AccountEditing;

namespace Keeper.ViewModels.TransWithTags
{
    [Export]
    class IncomeTranViewModel : Screen, IOneTranView
    {
        private readonly KeeperDb _db;
        private readonly AccountTreeStraightener _accountTreeStraightener;
        private readonly BalanceDuringTransactionHinter _balanceDuringTransactionHinter;

        public List<AccName> AccNamesListForIncome { get; }
        public List<AccName> AccNamesListForIncomeTag { get; set; }
        public List<CurrencyCodes> Currencies { get; set; } = Enum.GetValues(typeof(CurrencyCodes)).OfType<CurrencyCodes>().ToList();

        private AccName _myAccName;
        public AccName MyAccName
        {
            get { return _myAccName; }
            set
            {
                if (Equals(value, _myAccName)) return;
                _myAccName = value;
                NotifyOfPropertyChange();
                TranInWork.MyAccount = _accountTreeStraightener.Seek(_myAccName.Name, _db.Accounts);
            }
        }

        public TranWithTags TranInWork { get; set; }

        public string MyAccountBalance { get { return _balanceDuringTransactionHinter.GetMyAccountBalance(TranInWork); } }
        public string AmountInUsd { get { return _balanceDuringTransactionHinter.GetAmountInUsd(TranInWork); } }

        public Account Tag { get; set; }

        [ImportingConstructor]
        public IncomeTranViewModel(KeeperDb db, AccountTreeStraightener accountTreeStraightener, BalanceDuringTransactionHinter balanceDuringTransactionHinter)
        {
            _db = db;
            _accountTreeStraightener = accountTreeStraightener;
            _balanceDuringTransactionHinter = balanceDuringTransactionHinter;
            ListsForComboTrees.InitializeLists(_db);
            AccNamesListForIncome = ListsForComboTrees.MyAccNamesForIncome;
            AccNamesListForIncomeTag = ListsForComboTrees.AccNamesForIncomeTags;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Income transaction with tags";
        }

        public TranWithTags GetTran()
        {
            return TranInWork;
        }

        public void SetTran(TranWithTags tran)
        {
            TranInWork = tran.Clone();
            TranInWork.PropertyChanged += TranInWork_PropertyChanged;
            MyAccName = AccNamesListForIncome.FindThroughTheForest(TranInWork.MyAccount.Name)
                          ?? AccNamesListForIncome.FirstOrDefault(an => Equals(an.Name,"Мой кошелек"));
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
        #region горячие кнопки выбора из списков
        public void IncomeToMyWallet()    { MyAccName = AccNamesListForIncome.FindThroughTheForest("Мой кошелек"); }
        public void IncomeToBibZplGold() { MyAccName = AccNamesListForIncome.FindThroughTheForest("БИБ Зарплатная GOLD"); }
        public void IncomeToWardrobe()    { MyAccName = AccNamesListForIncome.FindThroughTheForest("Шкаф"); }
        public void IncomeToJuliaWallet() { MyAccName = AccNamesListForIncome.FindThroughTheForest("Юлин кошелек"); } 

//        public void IncomeFromIit() { Tag = ListsForComboboxes.AccountsWhoGivesMeMoney.FirstOrDefault(a => a.Name == "ИИТ"); }
//        public void IncomeFromBib() { Tag = ListsForComboboxes.AccountsWhoGivesMeMoney.FirstOrDefault(a => a.Name == "БИБ"); }
//        public void IncomeFromBgpb() { Tag = ListsForComboboxes.AccountsWhoGivesMeMoney.FirstOrDefault(a => a.Name == "БГПБ"); }
//        public void IncomeFromRelatives() { Tag = ListsForComboboxes.MyAccountsForShopping.FirstOrDefault(a => a.Name == "Родственники"); }
        #endregion
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
