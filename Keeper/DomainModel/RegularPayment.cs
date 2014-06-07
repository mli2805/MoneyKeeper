using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Composition;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using Keeper.Utils.Common;

namespace Keeper.DomainModel
{
  [DataContract]
  public class RegularPayment : ICloneable
  {
    [DataMember]public String Comment { get; set; }
    [DataMember]public Decimal Amount { get; set; }
    [DataMember]public CurrencyCodes Currency { get; set; }
    [DataMember]public bool ShouldBeReminded { get; set; }
    [DataMember]public int DayOfMonth { get; set; }

    public object Clone()
    {
      return this.MemberwiseClone();
    }
  }

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

  [Export]
  [Shared]
  public class RegularPaymentsProvider
  {
    private readonly string _filename;

    [Export]
    public RegularPayments RegularPayments { get; set; }


    [ImportingConstructor]
    public RegularPaymentsProvider(IMySettings mySettings)
    {
      _filename = Path.Combine((string) mySettings.GetSetting("DbPath"),
                               (string) mySettings.GetSetting("RegularPaymentsFile"));

      RegularPayments = Read() ?? new RegularPayments();
    }

    private RegularPayments Read()
    {
      if (!File.Exists(_filename)) return null;
      var stream = new FileStream(_filename, FileMode.Open);
      var jsonSerializer = new DataContractJsonSerializer(typeof (RegularPayments));
      return (RegularPayments) jsonSerializer.ReadObject(stream);
    }

    public void Write()
    {
      var stream = new FileStream(_filename, FileMode.Create);
      var jsonSerializer = new DataContractJsonSerializer(typeof(RegularPayments));
      jsonSerializer.WriteObject(stream, RegularPayments);

    }
  }
}
