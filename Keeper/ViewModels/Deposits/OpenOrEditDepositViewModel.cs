using System;
using System.Collections.Generic;
using System.Composition;
using System.Globalization;
using System.Linq;
using Caliburn.Micro;
using Keeper.DomainModel.DbTypes;
using Keeper.DomainModel.Deposit;
using Keeper.DomainModel.Enumes;
using Keeper.Utils.AccountEditing;

namespace Keeper.ViewModels.Deposits
{
    [Export]
    public class OpenOrEditDepositViewModel : Screen
    {
        public Deposit DepositInWork { get; set; }
        private string _windowTitle;
        private readonly KeeperDb _db;
        private readonly IWindowManager _windowManager;
        private readonly AccountTreeStraightener _accountTreeStraightener;

        public string Junction
        {
            get { return DepositInWork.ParentAccount.Name; }
            set
            {
                if (value == DepositInWork.ParentAccount.Name) return;
                DepositInWork.ParentAccount.Name = value;
                NotifyOfPropertyChange(() => Junction);
            }
        }

        #region Списки для комбобоксов
        private List<CurrencyCodes> _currencyList;
        private List<Account> _bankAccounts;
        private List<Account> _myFolders;
        private List<BankDepositOffer> _depositOffers;

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

        public List<Account> MyFolders
        {
            get { return _myFolders; }
            set
            {
                if (Equals(value, _myFolders)) return;
                _myFolders = value;
                NotifyOfPropertyChange(() => MyFolders);
            }
        }

        public List<BankDepositOffer> DepositOffers
        {
            get { return _depositOffers; }
            set
            {
                if (Equals(value, _depositOffers)) return;
                _depositOffers = value;
                NotifyOfPropertyChange(() => DepositOffers);
            }
        }

        private void InitializeListsForCombobox()
        {
            CurrencyList = Enum.GetValues(typeof(CurrencyCodes)).OfType<CurrencyCodes>().ToList();
            BankAccounts = _accountTreeStraightener.Flatten(_db.Accounts).Where(a => a.Is("Банки") && a.Children.Count == 0).ToList();
            MyFolders = _accountTreeStraightener.Flatten(_db.Accounts).Where(a => a.Is("Мои") && a.IsFolder).ToList();
            DepositOffers = _db.BankDepositOffers.ToList();
        }

        #endregion

        [ImportingConstructor]
        public OpenOrEditDepositViewModel(KeeperDb db, IWindowManager windowManager, AccountTreeStraightener accountTreeStraightener)
        {
            _db = db;
            _windowManager = windowManager;
            _accountTreeStraightener = accountTreeStraightener;
            InitializeListsForCombobox();
        }

        public void InitializeForm(Deposit deposit, string windowTitle)
        {
            _windowTitle = windowTitle;
            DepositInWork = deposit;

            if (windowTitle == "Добавить")
            {
                DepositInWork.DepositOffer = _db.BankDepositOffers.First();
                DepositInWork.StartDate = DateTime.Today;
                DepositInWork.FinishDate = DateTime.Today.AddMonths(1);
            }

        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = _windowTitle;
        }

        public void SaveDeposit()
        {
            TryClose(true);
        }

        public void MoveToClosed()
        {

        }

        public void CompileAccountName()
        {
            var rate = DepositInWork.DepositOffer.RateLines == null || DepositInWork.DepositOffer.RateLines.LastOrDefault() == null
                         ? 0
                         : DepositInWork.DepositOffer.RateLines.Last().Rate;
            Junction = string.Format("{0} {1} {2} - {3} {4:0.#}%",
               DepositInWork.DepositOffer.BankAccount.Name, DepositInWork.DepositOffer.DepositTitle,
               DepositInWork.StartDate.ToString("d/MM/yyyy", CultureInfo.InvariantCulture),
               DepositInWork.FinishDate.ToString("d/MM/yyyy", CultureInfo.InvariantCulture),
               rate);
        }

        public void FillDepositRatesTable()
        {
            var bankDepositRatesAndRulesViewModel = IoC.Get<BankDepositRatesAndRulesViewModel>();
            bankDepositRatesAndRulesViewModel.Initialize(DepositInWork.DepositOffer);
            _windowManager.ShowDialog(bankDepositRatesAndRulesViewModel);
        }
    }
}
