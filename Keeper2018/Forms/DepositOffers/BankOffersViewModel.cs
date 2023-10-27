using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;
using Caliburn.Micro;
using KeeperDomain;

namespace Keeper2018
{
    public class BankOffersViewModel : Screen
    {
        private readonly IWindowManager _windowManager;
        private readonly KeeperDataModel _dataModel;
        private readonly OneBankOfferViewModel _oneBankOfferViewModel;
        public ObservableCollection<DepositOfferModel> Rows { get; set; }

        private DepositOfferModel _selectedDepositOffer;
        public DepositOfferModel SelectedDepositOffer
        {
            get => _selectedDepositOffer;
            set
            {
                if (Equals(value, _selectedDepositOffer)) return;
                _selectedDepositOffer = value;
                NotifyOfPropertyChange();
            }
        }

        public BankOffersViewModel(IWindowManager windowManager, KeeperDataModel dataModel,
            OneBankOfferViewModel oneBankOfferViewModel)
        {
            _windowManager = windowManager;
            _dataModel = dataModel;
            _oneBankOfferViewModel = oneBankOfferViewModel;
        }

        // 161 - папка Карточки
        // 166 - папка Депозиты
        // 902 - папка Трастовые

        public SolidColorBrush DepositBrush { get; set; } = Brushes.PaleGreen;
        public SolidColorBrush CardBrush { get; set; } = Brushes.PeachPuff;
        public SolidColorBrush TrustBrush { get; set; } = Brushes.LightPink;
        public SolidColorBrush ClosedBrush { get; set; } = Brushes.LightGray;
        public SolidColorBrush NotInUseBrush { get; set; } = Brushes.Transparent;
        public void Initialize()
        {
            Rows = new ObservableCollection<DepositOfferModel>();

            foreach (var depositOfferModel in _dataModel.DepositOffers)
            {
                var account = _dataModel.AcMoDict.Values.FirstOrDefault(a =>
                    (a.IsDeposit && a.BankAccount.DepositOfferId == depositOfferModel.Id)
                    || (a.IsCard && a.BankAccount.DepositOfferId == depositOfferModel.Id));

                if (account != null)
                {
                    if (account.Is(166))
                        depositOfferModel.BackgroundColor = DepositBrush;
                    else if (account.Is(161))
                        depositOfferModel.BackgroundColor = CardBrush;
                    else if (account.Is(902))
                        depositOfferModel.BackgroundColor = TrustBrush;
                    else // счет к такой оферте существует, но не в этих папках, значит закрыт 
                        depositOfferModel.BackgroundColor = ClosedBrush;
                }
                else
                {
                    // для этой оферты нет счета вообще (счет открыт не как депозит), желательно переделать каким-то образом
                    depositOfferModel.BackgroundColor = NotInUseBrush;
                }
                Rows.Add(depositOfferModel);
            }

            SelectedDepositOffer = Rows.Last();
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Банковские депозиты";
        }

        public void AddOffer()
        {
            var offerModel = new DepositOfferModel
            {
                Id = Rows.Max(l => l.Id) + 1,
                Bank = SelectedDepositOffer.Bank,
                MainCurrency = CurrencyCode.BYN,
                DepositTerm = new DurationModel(1, Durations.Years),
            };

            _oneBankOfferViewModel.Initialize(offerModel);
            _windowManager.ShowDialog(_oneBankOfferViewModel);
            if (_oneBankOfferViewModel.IsCancelled) return;

            Rows.Add(_oneBankOfferViewModel.ModelInWork);
            _dataModel.DepositOffers = Rows.ToList();
            SelectedDepositOffer = Rows.Last();
        }

        public void EditSelectedOffer()
        {
            var offerModel = SelectedDepositOffer.DeepCopyExceptBank();
            _oneBankOfferViewModel.Initialize(offerModel);
            _windowManager.ShowDialog(_oneBankOfferViewModel);
            if (_oneBankOfferViewModel.IsCancelled) return;

            var index = Rows.IndexOf(SelectedDepositOffer);
            Rows.Remove(SelectedDepositOffer);
            Rows.Insert(index, offerModel);
            SelectedDepositOffer = Rows[index];
            _dataModel.DepositOffers = Rows.ToList();
        }

        public void RemoveSelectedOffer()
        {
            if (_dataModel.AcMoDict.Values.Any(a => a.IsDeposit && a.BankAccount.DepositOfferId == SelectedDepositOffer.Id))
            {
                var strs = new List<string> { "Существует как минимум один депозит открытый по этой оферте.", "", "Сначала удалите депозиты." };
                var vm = new MyMessageBoxViewModel(MessageType.Error, strs);
                _windowManager.ShowDialog(vm);
                return;
            }
            Rows.Remove(SelectedDepositOffer);
            _dataModel.DepositOffers = Rows.ToList();
        }

        public override void CanClose(Action<bool> callback)
        {
            _dataModel.DepositOffers = Rows.ToList();
            base.CanClose(callback);
        }
    }
}
