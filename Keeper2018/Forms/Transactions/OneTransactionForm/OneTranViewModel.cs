﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Caliburn.Micro;

namespace Keeper2018
{
    public class OneTranViewModel : Screen
    {
        public int Top { get; set; }
        private int _left;
        public int Left
        {
            get { return _left; }
            set
            {
                if (value == _left) return;
                _left = value;
                NotifyOfPropertyChange();
            }
        }
        public int Width { get; set; }
        public int Height { get; set; }

        private readonly IWindowManager _windowManager;
        private readonly KeeperDb _db;

        private string _caption;
        private TransactionModel _tranInWork;
        public TransactionModel TranInWork
        {
            get { return _tranInWork; }
            set
            {
                if (Equals(value, _tranInWork)) return;
                _tranInWork = value;
                NotifyOfPropertyChange();
            }
        }
        public List<Tuple<decimal, AccountModel, string>> ReceiptList { get; set; }

        public bool IsAddOrEdit { get; set; }
        public bool IsOneMore { get; set; }

        public UniversalControlVm MyIncomeControlVm { get; set; }
        public UniversalControlVm MyExpenseControlVm { get; set; }
        public UniversalControlVm MyTransferControlVm { get; set; }
        public UniversalControlVm MyExchangeControlVm { get; set; }

        public OperationTypeViewModel OperationTypeViewModel { get; } = new OperationTypeViewModel();

        public OneTranViewModel(IWindowManager windowManager, KeeperDb db,
            UniversalControlVm myIncomeControlVm, UniversalControlVm myExpenseControlVm,
            UniversalControlVm myTransferControlVm, UniversalControlVm myExchangeControlVm)
        {
            _windowManager = windowManager;
            _db = db;

            MyIncomeControlVm = myIncomeControlVm;
            MyExpenseControlVm = myExpenseControlVm;
            MyTransferControlVm = myTransferControlVm;
            MyExchangeControlVm = myExchangeControlVm;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = _caption;
        }

        public TransactionModel GetTran()
        {
            return TranInWork;
        }

        public void Init(TransactionModel tran, bool addOrEdit)
        {
            IsAddOrEdit = addOrEdit;
            _caption = addOrEdit ? "Добавить" : "Изменить";
            TranInWork = tran.Clone();

            InitControls();
            SetControlVisibilities(TranInWork.Operation);

            OperationTypeViewModel.PropertyChanged += OperationTypeViewModel_PropertyChanged;
        }

        private void OperationTypeViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            TranInWork.Operation = OperationTypeViewModel.SelectedOperationType;
            ValidateTranInWorkFieldsWithNewOperationType();
            InitControls();
            SetControlVisibilities(OperationTypeViewModel.SelectedOperationType);
        }

        private void ValidateTranInWorkFieldsWithNewOperationType()
        {
            TranInWork.Tags.Clear();
            if (TranInWork.MySecondAccount == null && TranInWork.Operation == OperationType.Перенос)
                TranInWork.MySecondAccount = _db.SeekAccount("Юлин кошелек");
            if (TranInWork.CurrencyInReturn == null)
                TranInWork.CurrencyInReturn = (TranInWork.Currency == CurrencyCode.BYN) ? CurrencyCode.USD : CurrencyCode.BYN;
        }

        private bool IsValid()
        {
            if (ReceiptList == null && TranInWork.HasntGotCategoryTagThoughItShould()) return false;
            if (TranInWork.Operation == OperationType.Доход || TranInWork.Operation == OperationType.Расход)
            {
                TranInWork.MySecondAccount = null;
            }
            if (TranInWork.Operation != OperationType.Обмен)
            {
                TranInWork.AmountInReturn = 0;
                TranInWork.CurrencyInReturn = null;
            }
            /* more checks
             * ...
            */
            return true;
        }

        public void Save()
        {
            if (!IsValid()) return;
            TryClose(true);
        }

        public void OneMore()
        {
            IsOneMore = true;
            Save();
        }

        public void Cancel()
        {
            TryClose(false);
        }

        private void InitControls()
        {
            MyIncomeControlVm.SetTran(TranInWork);
            MyExpenseControlVm.SetTran(TranInWork);
            MyTransferControlVm.SetTran(TranInWork);
            MyExchangeControlVm.SetTran(TranInWork);
        }
        private void SetControlVisibilities(OperationType opType)
        {
            MyIncomeControlVm.Visibility = opType == OperationType.Доход ? Visibility.Visible : Visibility.Collapsed;
            MyExpenseControlVm.Visibility = opType == OperationType.Расход ? Visibility.Visible : Visibility.Collapsed;
            MyTransferControlVm.Visibility = opType == OperationType.Перенос ? Visibility.Visible : Visibility.Collapsed;
            MyExchangeControlVm.Visibility = opType == OperationType.Обмен ? Visibility.Visible : Visibility.Collapsed;
        }

        private bool LeaveOneExternalAccountInTags()
        {
            var externalAccount = TranInWork.Tags.FirstOrDefault(a => a.Is("Внешние"));
            if (externalAccount == null)
            {
                MessageBox.Show("Должен быть хотя бы один продавец/услугодатель");
                return false;
            }
            TranInWork.Tags = new List<AccountModel>() { externalAccount };
            InitControls();
            return true;
        }

        public void Receipt()
        {
            if (!LeaveOneExternalAccountInTags()) return;

            Left = Left - 180;
            var receiptVm = new ReceiptViewModel();
            receiptVm.Initialize(TranInWork.Amount, TranInWork.Currency, _db.SeekAccount("Прочие расходы"));
            receiptVm.PlaceIt(Top, Left + Width, Height);

            if (_windowManager.ShowDialog(receiptVm) != true) return;

            ReceiptList = receiptVm.ResultList;
            Save();
        }
    }
}