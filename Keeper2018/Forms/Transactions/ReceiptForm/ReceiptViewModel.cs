using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;

namespace Keeper2018
{
    public class ReceiptViewModel : Screen
    {
        private readonly KeeperDb _db;
        private readonly AccNameSelectionControlInitializer _accNameSelectionControlInitializer;
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

        private CurrencyCode _currency;
        public CurrencyCode Currency
        {
            get { return _currency; }
            set
            {
                if (Equals(value, _currency)) return;
                _currency = value;
                NotifyOfPropertyChange(() => Currency);
            }
        }

        public List<Tuple<decimal, AccountModel, string>> ResultList { get; set; }

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
        public decimal PartialTotal => (from a in ResultList select a.Item1).Sum();

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

        public List<CurrencyCode> CurrencyList { get; private set; }

        public AccNameSelectorVm MyAccNameSelectorVm { get; set; }

        public ReceiptViewModel(KeeperDb db, AccNameSelectionControlInitializer accNameSelectionControlInitializer)
        {
            _db = db;
            _accNameSelectionControlInitializer = accNameSelectionControlInitializer;
            CurrencyList = Enum.GetValues(typeof(CurrencyCode)).OfType<CurrencyCode>().ToList();
        }

        public void Initialize(decimal totalAmount, CurrencyCode currency, AccountModel initialArticle)
        {
            ResultList = new List<Tuple<decimal, AccountModel, string>>();
            MyAccNameSelectorVm = _accNameSelectionControlInitializer.ForReceipt(initialArticle.Id);

            Currency = currency;
            TotalAmount = totalAmount;
            PartialAmount = totalAmount;

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
            foreach (var tuple in ResultList)
            {
                ReceiptFigure += $"\n {tuple.Item2.Name}   {tuple.Item1:#,0.00} {Currency.ToString().ToLower()}  {tuple.Item3}";
            }
            if (PartialTotal != 0) ReceiptFigure +=
                $"\n\n               Итого {PartialTotal:#,0.00} {Currency.ToString().ToLower()}";
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
            var partialArticle = _db.SeekAccountById(MyAccNameSelectorVm.MyAccName.Id);
            ResultList.Add(new Tuple<decimal, AccountModel, string>(PartialAmount, partialArticle, PartialComment));
            ChangeAllProperties();
        }

        public void DeleteOne()
        {
            if (ResultList.Count == 0) return;

            ResultList.RemoveAt(ResultList.Count - 1);
            ChangeAllProperties();
        }

        public void AcceptReceipt()
        {
            TryClose(true);
        }
    }
}
