using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Composition;
using System.Linq;
using Caliburn.Micro;
using Keeper.DomainModel;
using Keeper.DomainModel.Deposit;
using Keeper.Utils.Accounts;

namespace Keeper.ViewModels
{
    [Export]
    class BankDepositOffersViewModel : Screen
    {
        private readonly KeeperDb _db;
        private readonly AccountTreeStraightener _accountTreeStraightener;
        private readonly IWindowManager _windowManager;
        public ObservableCollection<BankDepositOffer> Rows { get; set; }
        public BankDepositOffer SelectedOffer { get; set; }

        public static List<Account> BankAccounts { get; private set; }
        public static List<CurrencyCodes> CurrencyList { get; private set; }

        [ImportingConstructor]
        public BankDepositOffersViewModel(KeeperDb db, AccountTreeStraightener accountTreeStraightener, IWindowManager windowManager)
        {
            _db = db;
            _accountTreeStraightener = accountTreeStraightener;
            _windowManager = windowManager;
            if (db.BankDepositOffers == null) db.BankDepositOffers = new ObservableCollection<BankDepositOffer>();
            Rows = db.BankDepositOffers;
            InitializeListsForCombobox();
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Банковские депозитные предложения";
        }
        private void InitializeListsForCombobox()
        {
            CurrencyList = Enum.GetValues(typeof(CurrencyCodes)).OfType<CurrencyCodes>().ToList();
            BankAccounts =
              (_accountTreeStraightener.Flatten(_db.Accounts).Where(
                account => account.Is("Банки") && account.Children.Count == 0)).ToList();
        }

        public void EditRatesAndRules()
        {
            var bankDepositRatesAndRulesViewModel = IoC.Get<BankDepositRatesAndRulesViewModel>();
            bankDepositRatesAndRulesViewModel.Initialize(SelectedOffer);
            _windowManager.ShowDialog(bankDepositRatesAndRulesViewModel);
        }
        public void CloseView()
        {
            TryClose();
        }

    }
}
