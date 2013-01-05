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
    private decimal _amount;
    private CurrencyCodes _currency;
    private decimal _amount2;
    private CurrencyCodes? _currency2;
    private Account _article;
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
    public decimal Amount2
    {
      get { return _amount2; }
      set
      {
        if (value == _amount2) return;
        _amount2 = value;
        NotifyOfPropertyChange(() => Amount2);
      }
    }

    public CurrencyCodes? Currency2
    {
      get { return _currency2; }
      set
      {
        if (value.Equals(_currency2)) return;
        _currency2 = value;
        NotifyOfPropertyChange(() => Currency2);
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

//    #region ' _isSelected '
//    private bool _isSelected;
//
//    [NotMapped]
//    public bool IsSelected
//    {
//      get { return _isSelected; }
//      set
//      {
//        if (value.Equals(_isSelected)) return;
//        _isSelected = value;
//        NotifyOfPropertyChange(() => IsSelected);
////        if (_isSelected) IoC.Get<TransactionsViewModel>().SelectedTransaction = this;
//      }
//    }
//    #endregion

    #region // два вычислимых поля содержащих цвет шрифта и фона для отображения транзакции
    [NotMapped]
    public Brush DayBackgroundColor
    {
      get
      {
        TimeSpan DaysFrom = Timestamp.Date - new DateTime(1972, 5, 28);
        if (DaysFrom.Days % 3 == 0) return Brushes.Cornsilk;
        if (DaysFrom.Days % 3 == 1) return Brushes.GhostWhite;
        return Brushes.Azure;
      }
    }

    [NotMapped]
    public Brush TransactionFontColor
    {
      get
      {
        if (Operation == OperationType.Доход) return Brushes.Blue;
        if (Operation == OperationType.Расход) return Brushes.Red;
        if (Operation == OperationType.Обмен) return Brushes.DarkGreen;
        if (Operation == OperationType.Перенос) return Brushes.Black;
        return Brushes.Gray;
      }
    }
    #endregion

    public string ToDumpWithNames()
    {
      var s = Timestamp + " ; " + Operation + " ; " +
              Debet + " ; " + Credit + " ; " +
              Amount + " ; " + Currency + " ; " + Amount2 + " ; ";
             
      if (Currency2 == null || Currency2 == 0) s = s + "null"; else s = s + Currency2;

      s = s + " ; " + Article + " ; " + Comment;

      return s;
    }

    /// <summary>
    /// создает новый инстанс и в нем возвращает полную копию данного инстанса, кроме Id
    /// </summary>
    /// <returns></returns>
    public Transaction Clone()
    {
      var cloneTransaction = new Transaction();

      cloneTransaction.Timestamp = Timestamp;
      cloneTransaction.Operation = Operation;
      cloneTransaction.Debet = Debet;
      cloneTransaction.Credit = Credit;
      cloneTransaction.Amount = Amount;
      cloneTransaction.Currency = Currency;
      cloneTransaction.Amount2 = Amount2;
      cloneTransaction.Currency2 = Currency2;
      cloneTransaction.Article = Article;
      cloneTransaction.Comment = Comment;

      return cloneTransaction;
    }

    /// <summary>
    /// засасывает в данный инстанс все поля из инстанса-хранилища (кроме Id)
    /// </summary>
    /// <param name="storage"></param>
    public void CloneFrom(Transaction storage)
    {
      this.Timestamp = storage.Timestamp;
      this.Operation = storage.Operation;
      this.Debet = storage.Debet;
      this.Credit = storage.Credit;
      this.Amount = storage.Amount;
      this.Currency = storage.Currency;
      this.Amount2 = storage.Amount2;
      this.Currency2 = storage.Currency2;
      this.Article = storage.Article;
      this.Comment = storage.Comment;
    }


    /// <summary>
    ///  возвращает заготовку созданную на основе этого инстанса, Amount и Comment оставляются пустыми
    /// </summary>
    /// <returns></returns>
    public Transaction Preform(string param)
    {
      var preformTransaction = new Transaction();

      if (param == "SameDate") preformTransaction.Timestamp = this.Timestamp;
      if (param == "SameDatePlusMinite") preformTransaction.Timestamp = this.Timestamp.AddMinutes(1);
      if (param == "NextDate") preformTransaction.Timestamp = this.Timestamp.Date.AddDays(1).AddHours(9);
      preformTransaction.Operation = this.Operation;
      preformTransaction.Debet = this.Debet;
      preformTransaction.Credit = this.Credit;
      preformTransaction.Currency = this.Currency;
      preformTransaction.Currency2 = this.Currency2;
      preformTransaction.Article = this.Article;

      return preformTransaction;
    }

    public int SignForAmount(Account a)
    {
      if (Credit == a || Credit.IsDescendantOf(a.Name)) return 1; else return -1;
    }
  }

 
}