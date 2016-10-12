using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using Caliburn.Micro;
using Keeper.DomainModel.DbTypes;
using Keeper.DomainModel.Enumes;
using Keeper.Utils.CommonKeeper;

namespace Keeper.ViewModels.SingleViews
{
    [Export]
    class ReceiptViewModel : Screen
    {
        private readonly ComboboxCaterer _comboboxCaterer;
        public int Top { get; set; }
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
        public int Height { get; set; }

        private decimal _totalAmount;
        public decimal TotalAmount
        {
            get { return _totalAmount; }
            set
            {
                if (value == _totalAmount) return;
                _totalAmount = value;
                NotifyOfPropertyChange(() => TotalAmount);
                PartialAmount = _totalAmount - PartialTotal;
                CanAcceptReceipt = TotalAmount == PartialTotal;
            }
        }

        private CurrencyCodes _currency;
        public CurrencyCodes Currency
        {
            get { return _currency; }
            set
            {
                if (Equals(value, _currency)) return;
                _currency = value;
                NotifyOfPropertyChange(() => Currency);
            }
        }


        public List<Tuple<decimal, Account, string>> Expense { get; set; }

        private decimal _partialAmount;
        public decimal PartialAmount
        {
            get { return _partialAmount; }
            set
            {
                if (value == _partialAmount) return;
                _partialAmount = value;
                NotifyOfPropertyChange(() => PartialAmount);
            }
        }
        public decimal PartialTotal => (from a in Expense select a.Item1).Sum();

        private Account _partialArticle;
        public Account PartialArticle
        {
            get { return _partialArticle; }
            set
            {
                if (Equals(value, _partialArticle)) return;
                _partialArticle = value;
                NotifyOfPropertyChange(() => PartialArticle);
            }
        }

        private string _partialComment;
        public string PartialComment
        {
            get { return _partialComment; }
            set
            {
                if (value == _partialComment) return;
                _partialComment = value;
                NotifyOfPropertyChange(() => PartialComment);
            }
        }

        private string _receiptFigure;
        public string ReceiptFigure
        {
            get { return _receiptFigure; }
            set
            {
                if (value == _receiptFigure) return;
                _receiptFigure = value;
                NotifyOfPropertyChange(() => ReceiptFigure);
            }
        }


        private bool _canAcceptReceipt;
        private int _left;

        public bool CanAcceptReceipt
        {
            get { return _canAcceptReceipt; }
            set
            {
                if (value.Equals(_canAcceptReceipt)) return;
                _canAcceptReceipt = value;
                NotifyOfPropertyChange(() => CanAcceptReceipt);
            }
        }

        public List<Account> ExpenseArticles { get; set; }
        public List<CurrencyCodes> CurrencyList { get; private set; }

        [ImportingConstructor]
        public ReceiptViewModel(ComboboxCaterer comboboxCaterer)
        {
            _comboboxCaterer = comboboxCaterer;
            CurrencyList = Enum.GetValues(typeof(CurrencyCodes)).OfType<CurrencyCodes>().ToList();
            ExpenseArticles = _comboboxCaterer.GetExpenseArticles();

            Expense = new List<Tuple<decimal, Account, string>>();
        }

        public void Initialize(decimal totalAmount, CurrencyCodes currency, Account initialArticle)
        {
            Currency = currency;
            TotalAmount = totalAmount;
            PartialAmount = totalAmount;
            PartialArticle = initialArticle;

            ChangeAllProperties();
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Чек";
            base.OnViewLoaded(view);
        }

        public void PlaceIt(int top, int left, int height)
        {
            Top = top;
            Left = left;
            Height = height;
        }
        private void BuildReceiptFigure()
        {
            ReceiptFigure = "";
            foreach (var tuple in Expense)
            {
                ReceiptFigure += $"\n {tuple.Item2.Name}   {tuple.Item1:#,#} {Currency.ToString().ToLower()}  {tuple.Item3}";
            }
            if (PartialTotal != 0) ReceiptFigure +=
                $"\n\n               Итого {PartialTotal:#,0} {Currency.ToString().ToLower()}";
        }

        private void ChangeAllProperties()
        {
            BuildReceiptFigure();
            PartialAmount = TotalAmount - PartialTotal;
            PartialComment = "";
            CanAcceptReceipt = PartialTotal == TotalAmount;
        }

        public void OnceMore()
        {
            Expense.Add(new Tuple<decimal, Account, string>(PartialAmount, PartialArticle, PartialComment));
            ChangeAllProperties();
        }

        public void DeleteOne()
        {
            if (Expense.Count == 0) return;

            Expense.RemoveAt(Expense.Count - 1);
            ChangeAllProperties();
        }

        public void AcceptReceipt()
        {
            TryClose(true);
        }

        //        public void CancelReceipt()
        //        {
        //            TryClose(false);
        //        }
    }
}
