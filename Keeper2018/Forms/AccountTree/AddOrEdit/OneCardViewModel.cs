using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using KeeperDomain;

namespace Keeper2018
{
    public class OneCardViewModel : Screen
    {
        private readonly KeeperDataModel _dataModel;
        private bool _isInAddMode;
        public AccountItemModel AccountItemModel { get; set; }
        public PayCard CardInWork { get; set; } = new PayCard();

        private List<DepositOfferModel> _depositOffers;
        public List<DepositOfferModel> DepositOffers
        {
            get => _depositOffers;
            set
            {
                if (Equals(value, _depositOffers)) return;
                _depositOffers = value;
                NotifyOfPropertyChange();
            }
        }

        private DepositOfferModel _selectedDepositOffer;
        public DepositOfferModel SelectedDepositOffer
        {
            get => _selectedDepositOffer;
            set
            {
                if (Equals(value, _selectedDepositOffer) || value == null) return;
                _selectedDepositOffer = value;
                CardInWork.DepositOfferId = _selectedDepositOffer.Id;
                NotifyOfPropertyChange();
            }
        }

        public string ParentName { get; set; }
        public string AccountName { get; set; }

        public List<PaymentSystem> PaymentSystems { get; set; }
        public bool IsSavePressed { get; set; }

        public OneCardViewModel(KeeperDataModel dataModel)
        {
            _dataModel = dataModel;
        }

        public void Initialize(AccountItemModel accountItemModel, bool isInAddMode)
        {
            IsSavePressed = false;
            _isInAddMode = isInAddMode;
            AccountItemModel = accountItemModel;
            DepositOffers = _dataModel.DepositOffers;
            CardInWork = AccountItemModel.PayCard;
            CardInWork.MyAccountId = accountItemModel.Id;
            PaymentSystems = Enum.GetValues(typeof(PaymentSystem)).OfType<PaymentSystem>().ToList();
            ParentName = accountItemModel.Parent.Name;

            if (isInAddMode)
            {
                SelectedDepositOffer = DepositOffers.Last();
                CardInWork.StartDate = DateTime.Today;
                CardInWork.FinishDate = DateTime.Today.AddYears(3).GetEndOfMonth();
            }
            else
            {
                _selectedDepositOffer = DepositOffers.FirstOrDefault(o => o.Id == CardInWork.DepositOfferId);
                AccountName = AccountItemModel.Name;
            }
        }

        protected override void OnViewLoaded(object view)
        {
            var cap = _isInAddMode ? "Добавить карточку" : "Изменить карточку";
            DisplayName = $"{cap} (id = {AccountItemModel.Id})";
        }

        public void SaveCard()
        {
            IsSavePressed = true;

            AccountItemModel.Name = string.IsNullOrEmpty(AccountName) ? "Безымянная" : AccountName;
            CardInWork.DepositOfferId = SelectedDepositOffer.Id;
            TryClose();
        }

        public void Cancel()
        {
            IsSavePressed = false;
            TryClose();
        }

    }
}
