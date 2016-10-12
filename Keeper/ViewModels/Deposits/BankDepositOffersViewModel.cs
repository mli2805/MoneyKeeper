using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Composition;
using System.Linq;
using Caliburn.Micro;
using Keeper.DomainModel.DbTypes;
using Keeper.DomainModel.Deposit;
using Keeper.DomainModel.Enumes;
using Keeper.DomainModel.Extentions;

namespace Keeper.ViewModels.Deposits
{
    [Export]
    class BankDepositOffersViewModel : Screen
    {
        private readonly KeeperDb _db;
        
        private readonly IWindowManager _windowManager;
        public ObservableCollection<BankDepositOffer> Rows { get; set; }
        public BankDepositOffer SelectedOffer { get; set; }

        public static List<Account> BankAccounts { get; private set; }
        public static List<CurrencyCodes> CurrencyList { get; private set; }

        [ImportingConstructor]
        public BankDepositOffersViewModel(KeeperDb db, IWindowManager windowManager)
        {
            _db = db;
            
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
              (_db.FlattenAccounts().Where(
                account => account.Is("Банки") && account.Children.Count == 0)).ToList();
        }

        public void EditRatesAndRules()
        {
            var bankDepositRatesAndRulesViewModel = IoC.Get<BankDepositRatesAndRulesViewModel>();
            bankDepositRatesAndRulesViewModel.Initialize(SelectedOffer);
            _windowManager.ShowDialog(bankDepositRatesAndRulesViewModel);
        }

        private void ValidateInput()
        {
            var tempList =  _db.BankDepositOffers.ToList();
            tempList.RemoveAll(item => item.IsInvalid());
            _db.BankDepositOffers = new ObservableCollection<BankDepositOffer>(tempList);
        }

        public override void CanClose(Action<bool> callback)
        {
            ValidateInput();
            base.CanClose(callback);
        }
        public void CloseView()
        {
            TryClose();
        }

    }
}
