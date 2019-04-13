using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;

namespace Keeper2018
{
    public class OneCardViewModel : Screen
    {
        private bool _isInAddMode;
        private AccountModel _accountModel;
        public PayCard CardInWork { get; set; } = new PayCard();

        public List<PaymentSystem> PaymentSystems { get; set; }
        public bool IsSavePressed { get; set; }

        public void InitializeForm(AccountModel accountModel, bool isInAddMode)
        {
            IsSavePressed = false;
            _isInAddMode = isInAddMode;
            _accountModel = accountModel;
            PaymentSystems = Enum.GetValues(typeof(PaymentSystem)).OfType<PaymentSystem>().ToList();


            if (!isInAddMode)
            {
                CardInWork.MyAccountId = _accountModel.Deposit.Card.MyAccountId;
                CardInWork.CardNumber = _accountModel.Deposit.Card.CardNumber;
                CardInWork.CardHolder = _accountModel.Deposit.Card.CardHolder;
                CardInWork.PaymentSystem = _accountModel.Deposit.Card.PaymentSystem;
                CardInWork.IsPayPass = _accountModel.Deposit.Card.IsPayPass;
            }
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = _isInAddMode ? "Добавить" : "Изменить";
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
