using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Windows.Media;
using Caliburn.Micro;
using Keeper.ViewModels;

namespace Keeper.DomainModel
{
  public class Transaction : PropertyChangedBase
  {
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }
    public OperationType Operation { get; set; }

    public Account Debet { get; set; }
    public Account Credit { get; set; }
    public Account Article { get; set; }

    public decimal Amount { get; set; }
    public CurrencyCodes Currency { get; set; }
    public string Comment { get; set; }

    #region ' _isSelected '
    private bool _isSelected;
    [NotMapped]
    public bool IsSelected
    {
      get { return _isSelected; }
      set
      {
        if (value.Equals(_isSelected)) return;
        _isSelected = value;
        NotifyOfPropertyChange(() => IsSelected);
        IoC.Get<TransactionsViewModel>().SelectedTransaction = this;
      }
    }
    #endregion
    
    #region // ��� ���������� ���� ���������� ���� ������ � ���� ��� ����������� ����������
    private Brush _dayBackgroundColor;
    [NotMapped]
    public Brush DayBackgroundColor
    {
      get
      {
        TimeSpan DaysFrom = Timestamp.Date - new DateTime(1972, 5, 28);
        if (DaysFrom.Days % 2 == 0) return Brushes.Cornsilk;
        else return Brushes.Azure;
      }
      set { _dayBackgroundColor = value; }
    }

    private Brush _transactionFontColor;
    [NotMapped]
    public Brush TransactionFontColor
    {
      get 
      {
        if (Operation == OperationType.�����) return Brushes.Blue;
        if (Operation == OperationType.������) return Brushes.Red;
        if (Operation == OperationType.�����) return Brushes.DarkGreen;
        else return Brushes.Black;
      }
      set { _transactionFontColor = value; }
    }
    #endregion

    public string ToDumpWithIds()
    {
      return Timestamp + " ; " + Operation + " ; " + 
             Debet.Id + " ; " + Credit.Id + " ; " + Article.Id + " ; " + 
             Amount + " ; " + Currency + " ; " + Comment;
    }

    public string ToDumpWithNames()
    {
      return Timestamp + " ; " + Operation + " ; " +
             Debet + " ; " + Credit + " ; " + Article + " ; " +
             Amount + " ; " + Currency + " ; " + Comment;
    }

    public Transaction Clone()
    {
      var cloneTransaction = new Transaction();

      cloneTransaction.Timestamp = Timestamp;
      cloneTransaction.Operation = Operation;
      cloneTransaction.Debet = Debet;
      cloneTransaction.Credit = Credit;
      cloneTransaction.Article = Article;
      cloneTransaction.Amount = Amount;
      cloneTransaction.Currency = Currency;
      cloneTransaction.Comment = Comment;

      return cloneTransaction;
    }
  }
}