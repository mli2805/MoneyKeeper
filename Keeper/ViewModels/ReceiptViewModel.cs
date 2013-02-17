using System;
using System.Collections.Generic;
using Caliburn.Micro;
using Keeper.DomainModel;

namespace Keeper.ViewModels
{
  class ReceiptViewModel : PropertyChangedBase
  {
    public DateTime ReceiptDate { get; set; }
    public string Acceptor { get; set; }

    public decimal TotalAmount { get; set; }
    public CurrencyCodes Currency { get; set; }

    public List<Tuple<decimal, Account, string>> Expense { get; set; }
    public List<string> PartsList { get; set; }
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

    public decimal PartialAmount { get; set; }
    public Account PartialArticle { get; set; }
    public string PartialComment { get; set; }

    public bool Result { get; set; }

    public ReceiptViewModel(DateTime receiptDate, string acceptor, decimal totalAmount, CurrencyCodes currency)
    {
      ReceiptDate = receiptDate;
      Acceptor = acceptor;
      TotalAmount = totalAmount;
      Currency = currency;

      ReceiptFigure = String.Format("ЧЕК хххххххх  {0:dd MMMM yyyy} {1}\n", ReceiptDate, Acceptor);
    }

    public void OnceMore()
    {
      ReceiptFigure += String.Format("\n {0:#,#} {1} {2} {3}", PartialAmount, Currency, PartialArticle, PartialComment);
     
    }

  }
}
