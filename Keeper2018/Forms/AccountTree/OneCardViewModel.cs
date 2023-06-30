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
        private AccountItemModel _accountItemModel;
        public PayCard CardInWork { get; set; } = new PayCard();

        public List<PaymentSystem> PaymentSystems { get; set; }
        public bool IsSavePressed { get; set; }

        public void InitializeForm(AccountItemModel accountItemModel, bool isInAddMode)
        {
            IsSavePressed = false;
            _isInAddMode = isInAddMode;
            _accountItemModel = accountItemModel;
            PaymentSystems = Enum.GetValues(typeof(PaymentSystem)).OfType<PaymentSystem>().ToList();

            CardInWork.DepositId = _accountItemModel.Deposit.Id;
            if (!isInAddMode)
            {
                CardInWork.CardNumber = _accountItemModel.Deposit.Card.CardNumber;
                CardInWork.CardHolder = _accountItemModel.Deposit.Card.CardHolder;
                CardInWork.IsMine = _accountItemModel.Deposit.Card.IsMine;
                CardInWork.PaymentSystem = _accountItemModel.Deposit.Card.PaymentSystem;
                CardInWork.IsVirtual = _accountItemModel.Deposit.Card.IsVirtual;
                CardInWork.IsPayPass = _accountItemModel.Deposit.Card.IsPayPass;
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
