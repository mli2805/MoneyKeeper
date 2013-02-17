using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Keeper.DomainModel;

namespace Keeper.ViewModels
{
  class ReceiptViewModel : Screen
  {
    public DateTime ReceiptDate { get; set; }
    public string Acceptor { get; set; }

    public decimal TotalAmount { get; set; }
    public CurrencyCodes Currency { get; set; }
    public decimal PartialTotal { get { return (from a in Expense select a.Item1).Sum(); } }

    public List<Tuple<decimal, Account, string>> Expense { get; set; }

    private string _receiptFigure;
    private decimal _partialAmount;
    private Account _partialArticle;
    private string _partialComment;
    private bool _canAcceptReceipt;

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

    public bool Result { get; set; }

    public ReceiptViewModel(DateTime receiptDate, string acceptor, CurrencyCodes currency, decimal totalAmount, Account article)
    {
      ReceiptDate = receiptDate;
      Acceptor = acceptor;
      Currency = currency;

      TotalAmount = totalAmount;
      PartialAmount = totalAmount;
      PartialArticle = article;

      Expense = new List<Tuple<decimal, Account, string>>();
      ChangeAllProperties();

      Result = false;
    }

    private void BuildReceiptFigure()
    {
      ReceiptFigure = String.Format("                {0:dd-MM-yyyy}  {1}\n", ReceiptDate, Acceptor);
      foreach (var tuple in Expense)
      {
        ReceiptFigure += String.Format("\n {0}   {1:#,#} {2}  {3}",
                           tuple.Item2.Name, tuple.Item1, Currency.ToString().ToLower(), tuple.Item3);
      }
      if (PartialTotal != 0) ReceiptFigure += String.Format("\n\n               Итого {0:#,0} {1}", 
                                                            PartialTotal, Currency.ToString().ToLower());
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

      Expense.RemoveAt(Expense.Count-1);
      ChangeAllProperties();
    }

    public void AcceptReceipt()
    {
      Result = true;
      TryClose();
    }

    public void CancelReceipt()
    {
      Result = false;
      TryClose();
    }
  }
}
