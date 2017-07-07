using System;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace Keeper.DomainModel.WorkTypes
{
  [DataContract]
  public class RegularPayments : ICloneable
  {
    [DataMember]public ObservableCollection<RegularPayment> Income { get; set; }
    [DataMember]
    public ObservableCollection<RegularPayment> Expenses { get; set; }

    public RegularPayments()
    {
      Income = new ObservableCollection<RegularPayment>();
      Expenses = new ObservableCollection<RegularPayment>();
    }

    public object Clone()
    {
      var clone = new RegularPayments();

      foreach (var payment in Income) clone.Income.Add((RegularPayment)payment.Clone());
      foreach (var payment in Expenses) clone.Expenses.Add((RegularPayment)payment.Clone());

      return clone;
    }
  }
}