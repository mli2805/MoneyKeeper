using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Windows.Media;
using Caliburn.Micro;
using Keeper.ViewModels;

namespace Keeper.DomainModel
{
  public class Transaction : PropertyChangedBase
  {
    private DateTime _timestamp;
    private OperationType _operation;
    private Account _debet;
    private Account _credit;
    private Account _article;
    private decimal _amount;
    private CurrencyCodes _currency;
    private string _comment;

    public int Id { get; set; }
    public DateTime Timestamp
    {
      get { return _timestamp; }
      set
      {
        if (value.Equals(_timestamp)) return;
        _timestamp = value;
        NotifyOfPropertyChange(() => Timestamp);
      }
    }
    public OperationType Operation
    {
      get { return _operation; }
      set
      {
        if (Equals(value, _operation)) return;
        _operation = value;
        NotifyOfPropertyChange(() => TransactionFontColor);
      }
    }
    public Account Debet
    {
      get { return _debet; }
      set
      {
        if (Equals(value, _debet)) return;
        _debet = value;
        NotifyOfPropertyChange(() => Debet);
      }
    }
    public Account Credit
    {
      get { return _credit; }
      set
      {
        if (Equals(value, _credit)) return;
        _credit = value;
        NotifyOfPropertyChange(() => Credit);
      }
    }
    public Account Article
    {
      get { return _article; }
      set
      {
        if (Equals(value, _article)) return;
        _article = value;
        NotifyOfPropertyChange(() => Article);
      }
    }
    public decimal Amount
    {
      get { return _amount; }
      set
      {
        if (value == _amount) return;
        _amount = value;
        NotifyOfPropertyChange(() => Amount);
      }
    }
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
    public string Comment
    {
      get { return _comment; }
      set
      {
        if (value == _comment) return;
        _comment = value;
        NotifyOfPropertyChange(() => Comment);
      }
    }

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
        if (_isSelected) IoC.Get<TransactionsViewModel>().SelectedTransaction = this;
      }
    }
    #endregion
    
    #region // ��� ���������� ���� ���������� ���� ������ � ���� ��� ����������� ����������
    [NotMapped]
    public Brush DayBackgroundColor
    {
      get
      {
        TimeSpan DaysFrom = Timestamp.Date - new DateTime(1972, 5, 28);
        if (DaysFrom.Days % 2 == 0) return Brushes.Cornsilk;
        else return Brushes.Azure;
      }
    }

    [NotMapped]
    public Brush TransactionFontColor
    {
      get 
      {
        if (Operation == OperationType.�����) return Brushes.Blue;
        if (Operation == OperationType.������) return Brushes.Red;
        if (Operation == OperationType.�����) return Brushes.DarkGreen;
        if (Operation == OperationType.�������) return Brushes.Black;
        return Brushes.Gray; 
      }
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


    /// <summary>
    /// ���������� ������ ����� ������� ��������, ����� Id
    /// </summary>
    /// <returns></returns>
    public Transaction Clone()
    {
      var cloneTransaction = new Transaction();

      cloneTransaction.Timestamp = Timestamp;
      cloneTransaction.Operation = Operation;
      cloneTransaction.Debet     = Debet;
      cloneTransaction.Credit    = Credit;
      cloneTransaction.Article   = Article;
      cloneTransaction.Amount    = Amount;
      cloneTransaction.Currency  = Currency;
      cloneTransaction.Comment   = Comment;

      return cloneTransaction;
    }

    /// <summary>
    /// ���������� � ������ ������� ��� ���� �� ��������-��������� (����� Id)
    /// </summary>
    /// <param name="storage"></param>
    public void CloneFrom(Transaction storage)
    {
      this.Timestamp = storage.Timestamp;
      this.Operation = storage.Operation;
      this.Debet     = storage.Debet;
      this.Credit    = storage.Credit;
      this.Article   = storage.Article;
      this.Amount    = storage.Amount;
      this.Currency  = storage.Currency;
      this.Comment   = storage.Comment;
    }


    /// <summary>
    ///  ���������� ��������� ��������� �� ������ ����� ��������, Amount � Comment ����������� �������
    /// </summary>
    /// <returns></returns>
    public Transaction Preform(string param)  
    {
      var preformTransaction = new Transaction();

      if (param == "SameDate") preformTransaction.Timestamp = this.Timestamp.AddMinutes(1);
      if (param == "NextDate") preformTransaction.Timestamp = this.Timestamp.Date.AddDays(1);
      preformTransaction.Operation = this.Operation;
      preformTransaction.Debet     = this.Debet;
      preformTransaction.Credit    = this.Credit;
      preformTransaction.Article   = this.Article;
      preformTransaction.Currency  = this.Currency;

      return preformTransaction;
    }
  }
}