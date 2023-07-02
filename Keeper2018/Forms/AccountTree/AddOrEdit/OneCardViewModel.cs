using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using KeeperDomain;

namespace Keeper2018
{
    public class OneCardViewModel : Screen
    {
        private bool _isInAddMode;
        public AccountItemModel AccountItemModel { get; set; }
        public PayCard CardInWork { get; set; } = new PayCard();

        public string ParentName { get; set; }

        private string _junction;

        public string Junction
        {
            get => _junction;
            set
            {
                if (value == _junction) return;
                _junction = value;
                NotifyOfPropertyChange();
            }
        }

        public List<PaymentSystem> PaymentSystems { get; set; }
        public bool IsSavePressed { get; set; }

        public void InitializeForm(AccountItemModel accountItemModel, bool isInAddMode)
        {
            IsSavePressed = false;
            _isInAddMode = isInAddMode;
            AccountItemModel = accountItemModel;
            CardInWork = AccountItemModel.PayCard;
            PaymentSystems = Enum.GetValues(typeof(PaymentSystem)).OfType<PaymentSystem>().ToList();
            ParentName = accountItemModel.Parent.Name;

            if (isInAddMode)
            {
                CardInWork.StartDate = DateTime.Today;
                CardInWork.FinishDate = DateTime.Today.AddYears(3).GetEndOfMonth();
                Junction = "";
            }
            else
            {
                Junction = AccountItemModel.Name;
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
            TryClose();
        }

        public void Cancel()
        {
            IsSavePressed = false;
            TryClose();
        }

    }
}
